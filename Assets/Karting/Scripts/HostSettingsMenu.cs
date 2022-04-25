using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

namespace KartGame.UI
{
public class HostSettingsMenu : MonoBehaviourPunCallbacks
{
	public TMP_InputField Usernamefield;
	public TMP_InputField Passwordfield;
	
    public void SetMaxPlayers(int maxPlayersIndex) {
		int players = maxPlayersIndex + 1;
		GameSettings.MaxPlayers = players;
		Debug.Log(players);
	}
	
    public void ChangeMapL() {
		GameSettings.MapID--;
		Debug.Log(GameSettings.MapID);
	}
	
    public void ChangeMapR() {
		GameSettings.MapID++;
		Debug.Log(GameSettings.MapID);
	}
	
    public void HostRace() {
    	GameSettings.Username = Usernamefield.text;
		Debug.Log(GameSettings.Username);
		GameSettings.GamePassword = Passwordfield.text;
		Debug.Log(GameSettings.GamePassword);
		PhotonNetwork.CreateRoom(GameSettings.GamePassword);
	}

	public override void OnJoinedRoom()
	{
		PhotonNetwork.LoadLevel("MainScene");
	}
}
}
