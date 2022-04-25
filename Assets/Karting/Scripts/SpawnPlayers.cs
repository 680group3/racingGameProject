using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;

public class SpawnPlayers : MonoBehaviour
{

    public GameObject playerPrefab;
    
    public float minX; 
    public float maxX;
    public float minY; 
    public float maxY; 
    public float minZ;
    public float maxZ;
    
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected) {
            object[] instanceData = new object[1];
            instanceData[0] = (string)GameSettings.Username;
   
            Vector3 randpos = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));
            GameObject pl = PhotonNetwork.Instantiate(playerPrefab.name, randpos, Quaternion.identity, 0, instanceData);

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
