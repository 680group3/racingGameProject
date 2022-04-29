using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

namespace KartGame.UI
{
public class JoinSettingsMenu : MonoBehaviourPunCallbacks
{

	public TMP_InputField Usernamefield;
	public TMP_InputField Passwordfield;
	
    public void JoinRace() {
    	GameSettings.Username = Usernamefield.text;
		Debug.Log(GameSettings.Username);
		GameSettings.GamePassword = Passwordfield.text;
		Debug.Log(GameSettings.GamePassword);
		
		PhotonNetwork.NickName = GameSettings.Username;
		PhotonNetwork.JoinRoom(GameSettings.GamePassword);
	}

	public override void OnJoinedRoom()
	{
		PhotonNetwork.LoadLevel("MainScene");
	}
}
}
