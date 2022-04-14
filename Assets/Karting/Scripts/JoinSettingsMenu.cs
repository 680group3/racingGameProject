using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KartGame.UI
{
public class JoinSettingsMenu : MonoBehaviour
{

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
}
}
