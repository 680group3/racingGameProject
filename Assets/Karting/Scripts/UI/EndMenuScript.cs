using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;

namespace KartGame.UI
{
    public class EndMenuScript : MonoBehaviour
    {
        public TextMeshProUGUI Msg;
        public TextMeshProUGUI[] RaceList;

        void Start()
        {
           
        }

        // Update is called once per frame
        void Update()
        {
            Msg.text = "Leaderboard";  
            for (int i = 0; i < GameSettings.finalTimes.Length; i++) {
                RaceList[i].text = (i + 1) + ". " + GameSettings.finalTimes[i];
            }
        }

        public void LoadMenu()
        {
          StartCoroutine(DisconnectAndLoad());
        }
         IEnumerator DisconnectAndLoad()
        {
          PhotonNetwork.LeaveRoom();
             while (PhotonNetwork.InRoom)
              yield return null;
          SceneManager.LoadScene("IntroMenu");
        }
    }
}