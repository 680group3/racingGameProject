using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace KartGame.UI
{
public class JoinSettingsMenu : MonoBehaviour
{

	public TMP_InputField IPfield;
	public TMP_InputField Passwordfield;
	
    public void ChangeCarL() {
		GameSettings.CarID--;
		Debug.Log(GameSettings.CarID);
	}
	
    public void ChangeCarR() {
		GameSettings.CarID++;
		Debug.Log(GameSettings.CarID);
	}

    public void ChangeColorL() {
		GameSettings.ColorID--;
		Debug.Log(GameSettings.ColorID);
	}
	
    public void ChangeColorR() {
		GameSettings.ColorID++;
		Debug.Log(GameSettings.ColorID);
	}
	
    public void JoinRace() {
		GameSettings.IPaddress = IPfield.text;
		Debug.Log(GameSettings.IPaddress);
		
		GameSettings.GamePassword = Passwordfield.text;
		Debug.Log(GameSettings.GamePassword);
	}
}
}
