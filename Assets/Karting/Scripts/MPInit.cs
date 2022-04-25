using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class MPInit : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
       PhotonNetwork.ConnectUsingSettings(); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnConnectedToMaster()
    {
      PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
       SceneManager.LoadScene("IntroMenu");
    }
}
