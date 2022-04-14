using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KartGame.UI
{
public class HostSettingsMenu : MonoBehaviour
{
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
}
}
