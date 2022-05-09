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
        Debug.Log("got " + racerStatus.Length + " players, starting countdown");
        raceCountdownTrigger.Play(); // race countdown animation
        StartCoroutine(CountdownThenStartRaceRoutine());
       // StartCoroutine(ShowObjectivesRoutine());
         raceActive = true;
    }

    private void UnfreezeAndBegin() 
    {
        foreach (Racer k in karts)
        {
            k.SetCanMove(true);
        }
        m_TimeManager.StartRace();
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

        if (!raceActive) {return;}
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }
        int nRacersFinished = 0;
         foreach (ObjectiveCompleteLaps obj in racerStatus) {
            //  Debug.Log(obj + " " + obj.currentLap);
             if (obj.currentLap >= 4) { // 3 laps completed (+1 because at the start you cross the finish line)
                   // done
                   nRacersFinished++;
            }
          }
        if (nRacersFinished == racerStatus.Length) {
            ComputeTimes();
            photonView.RPC("EndGame", RpcTarget.All, null);
        }
        
    }

    string getTimeString(float time){
        int timeInt = (int)(time);
        int minutes = timeInt / 60;
        int seconds = timeInt % 60;
        float fraction = (time * 100) % 100;
        if (fraction > 99) fraction = 99;
        return string.Format("{0}:{1:00}:{2:00}", minutes, seconds, fraction);
    }

    private void ComputeTimes() {
        string[] timeStrings = new string[4];
        int[] idxes = new int[4];
        for (int i = 0; i < 4; i++) {
            idxes[i] = i;
        }

        float[] sums = new float[4];
        float mintime = 999999999.0f;
        for (int i = 0; i < 4; i++) {
            sums[i] = mintime;
        }
        int minIdx = 0;
        for (int i = 0; i < racerStatus.Length; i++) {
            float sum = 0.0f;
            foreach (float t in racerStatus[i].tdisp.finishedLapTimes) {
                sum += t;
            }
            sums[i] = sum;
            if (sum < mintime) {
                minIdx = i;
                mintime = sum;
            }
        }
        System.Array.Sort(sums, idxes);
        GameObject[] racers = GameObject.FindGameObjectsWithTag("NetPlayer");
        for (int i = 0; i < 4; i++) {
            Debug.Log("SUM: " + sums[i] +" i: " + i);
            if (sums[i] < 999999999.0f) {
                int idx = idxes[i];
                if (idx < racers.Length && racers[idx] != null) {
                    string ntext = racers[idx].GetComponent<VehCtrl>().nameText.text;
                    string name = (string.IsNullOrEmpty(ntext) ? "UNNAMED" : ntext);
                    timeStrings[i] = name + " " + getTimeString(sums[i]); 
                } else {
                    timeStrings[i] = "";
                }
            } else {
                timeStrings[i] = "";
            }
        }

        photonView.RPC("UpdateTimes", RpcTarget.All, timeStrings);
    }

    [PunRPC]
    public void UpdateTimes(string [] times)
    {
        for (int i = 0; i < 4; i++) {
            GameSettings.finalTimes[i] = times[i];
        }
    }

    [PunRPC]
    public void EndGame()
    {
        // unlocks the cursor before leaving the scene, to be able to click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        m_TimeManager.StopRace();

        raceActive = false;
     // Remember that we need to load the appropriate end scene after a delay
        endGameFadeCanvasGroup.gameObject.SetActive(true);

        PhotonNetwork.LoadLevel("EndScene");
    }
}
