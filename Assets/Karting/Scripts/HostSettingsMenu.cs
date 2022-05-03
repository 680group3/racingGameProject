using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

namespace KartGame.UI
{
public class HostSettingsMenu : MonoBehaviourPunCallbacks
{
	private ExitGames.Client.Photon.Hashtable roomProp = new ExitGames.Client.Photon.Hashtable();

	public TMP_InputField Usernamefield;
	public TMP_InputField Passwordfield;
	public RawImage[] MapImages;

    public void SetMaxPlayers(int maxPlayersIndex) {
		int players = maxPlayersIndex + 1;
		GameSettings.MaxPlayers = players;
		Debug.Log(players);
	}
	
    public void ChangeMapL() {
		GameSettings.MapID--;
		if (GameSettings.MapID < 0) {
			GameSettings.MapID = 0;
		}
		foreach (RawImage i in MapImages) {
			i.gameObject.SetActive(false);
		}
		MapImages[GameSettings.MapID].gameObject.SetActive(true);
		Debug.Log(GameSettings.MapID);
	}
	
    public void ChangeMapR() {
		GameSettings.MapID++;
		if (GameSettings.MapID > 2) {
			GameSettings.MapID = 2;
		}
		foreach (RawImage i in MapImages) {
			i.gameObject.SetActive(false);
		}
		MapImages[GameSettings.MapID].gameObject.SetActive(true);
		Debug.Log(GameSettings.MapID);
	}
	
    public void HostRace() {
    	GameSettings.Username = Usernamefield.text;
		Debug.Log(GameSettings.Username);
		GameSettings.GamePassword = Passwordfield.text;
		Debug.Log(GameSettings.GamePassword);
		GameSettings.MapID = (int) Mathf.Clamp(GameSettings.MapID, 0, 2);
		PhotonNetwork.NickName = GameSettings.Username;
		PhotonNetwork.CreateRoom(GameSettings.GamePassword, new RoomOptions { MaxPlayers = (byte) GameSettings.MaxPlayers });
	}

	public override void OnJoinedRoom()
	{
		roomProp["mapID"] = GameSettings.MapID;
		PhotonNetwork.CurrentRoom.SetCustomProperties(roomProp);
		string[] names = new string[] { "MainScene", "Map2", "Map3" };
		Debug.Log("Host: " + names[GameSettings.MapID]);
		PhotonNetwork.LoadLevel(names[GameSettings.MapID]);
	}
}
}
