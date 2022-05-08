using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using KartGame.KartSystems;
using UnityEngine.SceneManagement;
using Photon.Pun;

public enum GameState{Play, Won, Lost}

public class GameFlowManager : MonoBehaviourPunCallbacks
{
    [Header("Parameters")]
    [Tooltip("Duration of the fade-to-black at the end of the game")]
    public float endSceneLoadDelay = 3f;
    [Tooltip("The canvas group of the fade-to-black screen")]
    public CanvasGroup endGameFadeCanvasGroup;

    [Header("Win")]
    [Tooltip("This string has to be the name of the scene you want to load when winning")]
    public string winSceneName = "WinScene";
    [Tooltip("Duration of delay before the fade-to-black, if winning")]
    public float delayBeforeFadeToBlack = 4f;
    [Tooltip("Duration of delay before the win message")]
    public float delayBeforeWinMessage = 2f;
    [Tooltip("Sound played on win")]
    public AudioClip victorySound;

    [Tooltip("Prefab for the win game message")]
    public DisplayMessage winDisplayMessage;

    public PlayableDirector raceCountdownTrigger;

    [Header("Lose")]
    [Tooltip("This string has to be the name of the scene you want to load when losing")]
    public string loseSceneName = "LoseScene";
    [Tooltip("Prefab for the lose game message")]
    public DisplayMessage loseDisplayMessage;


    public GameState gameState { get; private set; }

    public GameObject[] playerPrefabs;
    
    public float minX; 
    public float maxX;
    public float minY; 
    public float maxY; 
    public float minZ;
    public float maxZ;

    public Transform[] spawnLocs;

    public bool autoFindKarts = true;
    public Racer playerKart;

    public GameObject startButton;
    public GameObject waitingLabel;

    ObjectiveCompleteLaps[] racerStatus;

    Racer[] karts;
    ObjectiveManager m_ObjectiveManager;
    TimeManager m_TimeManager;
    float m_TimeLoadEndGameScene;
    string m_SceneToLoad;
    float elapsedTimeBeforeEndScene = 0;
    bool raceActive = false;

    void Start()
    {
        startButton.SetActive(false);
        waitingLabel.SetActive(false);

        if (PhotonNetwork.IsConnected) {
            Debug.Log("Connected, starting game");
            object[] instanceData = new object[1];
            instanceData[0] = (string)GameSettings.Username;
            
            Vector3 startpos = spawnLocs[PhotonNetwork.LocalPlayer.ActorNumber - 1].position;

            //Vector3 randpos = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));
            GameObject pl = PhotonNetwork.Instantiate(playerPrefabs[GameSettings.ColorID].name, startpos, Quaternion.identity, 0, instanceData);

            if (PhotonNetwork.IsMasterClient) {
                startButton.SetActive(true);
            } else {
                waitingLabel.SetActive(true);
            }
        }

        if (autoFindKarts)
        {
            karts = FindObjectsOfType<Racer>();
            if (karts.Length > 0)
            {
                if (!playerKart) playerKart = karts[0];
            }
            DebugUtility.HandleErrorIfNullFindObject<Racer, GameFlowManager>(playerKart, this);
        }

        m_ObjectiveManager = FindObjectOfType<ObjectiveManager>();
		DebugUtility.HandleErrorIfNullFindObject<ObjectiveManager, GameFlowManager>(m_ObjectiveManager, this);

        m_TimeManager = FindObjectOfType<TimeManager>();
        DebugUtility.HandleErrorIfNullFindObject<TimeManager, GameFlowManager>(m_TimeManager, this);

        AudioUtility.SetMasterVolume(1);

        winDisplayMessage.gameObject.SetActive(false);
        loseDisplayMessage.gameObject.SetActive(false);

        m_TimeManager.StopRace();
        foreach (Racer k in karts)
        {
			k.SetCanMove(false);
        }
        raceActive = false;
    }

    IEnumerator CountdownThenStartRaceRoutine() {
        yield return new WaitForSeconds(3f);
        UnfreezeAndBegin();
    }

    public void BeginRace()
    {
        if (PhotonNetwork.IsMasterClient) {
            photonView.RPC("StartRace", RpcTarget.All, null);
        }
    }

    [PunRPC]
    public void StartRace() {
        startButton.SetActive(false);
        waitingLabel.SetActive(false);
        GameObject[] racers = GameObject.FindGameObjectsWithTag("NetPlayer");
        racerStatus = new ObjectiveCompleteLaps[racers.Length];

        for (int i = 0; i < racers.Length; i++) {
            racerStatus[i] = racers[i].GetComponent<ObjectiveCompleteLaps>();
            Debug.Log(i + " " + racerStatus[i] == null);
        }
        Debug.Log("got " + racerStatus.Length + " player components, starting countdown");
        raceCountdownTrigger.Play(); // race countdown animation
        StartCoroutine(CountdownThenStartRaceRoutine());
       // StartCoroutine(ShowObjectivesRoutine());
    }

    private void UnfreezeAndBegin() 
    {
        foreach (Racer k in karts)
        {
            k.SetCanMove(true);
        }
        m_TimeManager.StartRace();
        raceActive = true;
    }

    IEnumerator ShowObjectivesRoutine() {
        while (m_ObjectiveManager.Objectives.Count == 0)
            yield return null;
        yield return new WaitForSecondsRealtime(0.2f);
        for (int i = 0; i < m_ObjectiveManager.Objectives.Count; i++)
        {
           if (m_ObjectiveManager.Objectives[i].displayMessage)m_ObjectiveManager.Objectives[i].displayMessage.Display();
           yield return new WaitForSecondsRealtime(1f);
        }
    }


    void LateUpdate()
    {

        if (gameState != GameState.Play)
        {
            elapsedTimeBeforeEndScene += Time.deltaTime;
            if(elapsedTimeBeforeEndScene >= endSceneLoadDelay)
            {

                float timeRatio = 1 - (m_TimeLoadEndGameScene - Time.time) / endSceneLoadDelay;
                endGameFadeCanvasGroup.alpha = timeRatio;

                float volumeRatio = Mathf.Abs(timeRatio);
                float volume = Mathf.Clamp(1 - volumeRatio, 0, 1);
                AudioUtility.SetMasterVolume(volume);

                // See if it's time to load the end scene (after the delay)
                if (Time.time >= m_TimeLoadEndGameScene)
                {
                    SceneManager.LoadScene(m_SceneToLoad);
                    gameState = GameState.Play;
                }
            }
        }
        else
        {
            if (raceActive) {
                int nRacersFinished = 0;
                foreach (ObjectiveCompleteLaps obj in racerStatus) {
                    if (obj.currentLap == 4) { // 3 laps completed (+1 because at the start you cross the finish line)
                        // done
                        nRacersFinished++;
                    }
                }

               if (nRacersFinished == racerStatus.Length)
                   EndGame(true);

            }
            //if (m_ObjectiveManager.AreAllObjectivesCompleted())

            if (m_TimeManager.IsFinite && m_TimeManager.IsOver)
                EndGame(false);
        }
    }

    void EndGame(bool win)
    {
        raceActive = false;
        // unlocks the cursor before leaving the scene, to be able to click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        m_TimeManager.StopRace();

        // Remember that we need to load the appropriate end scene after a delay
        gameState = win ? GameState.Won : GameState.Lost;
        endGameFadeCanvasGroup.gameObject.SetActive(true);
        if (win)
        {
            m_SceneToLoad = winSceneName;
            m_TimeLoadEndGameScene = Time.time + endSceneLoadDelay + delayBeforeFadeToBlack;

            // play a sound on win
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = victorySound;
            audioSource.playOnAwake = false;
            audioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDVictory);
            audioSource.PlayScheduled(AudioSettings.dspTime + delayBeforeWinMessage);

            // create a game message
            winDisplayMessage.delayBeforeShowing = delayBeforeWinMessage;
            winDisplayMessage.gameObject.SetActive(true);
        }
        else
        {
            m_SceneToLoad = loseSceneName;
            m_TimeLoadEndGameScene = Time.time + endSceneLoadDelay + delayBeforeFadeToBlack;

            // create a game message
            loseDisplayMessage.delayBeforeShowing = delayBeforeWinMessage;
            loseDisplayMessage.gameObject.SetActive(true);
        }
    }
}
