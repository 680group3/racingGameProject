using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace KartGame.UI
{
public class UsernameSettingsMenu : MonoBehaviour
{
	public TMP_InputField Usernamefield;
	
    public void StartRace() {
		GameSettings.Username = Usernamefield.text;
		Debug.Log(GameSettings.Username);
	}
}
}
