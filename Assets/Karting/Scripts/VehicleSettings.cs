using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace KartGame.UI
{
public class VehicleSettings : MonoBehaviour
{
	private const int MAX_CAR_COLOR = 3; 
	private const int MAX_CAR = 1; 

    public GameObject[] playerPrefabs;

    public void ChangeCarL() {
		GameSettings.CarID--;
		GameSettings.CarID = (int) Mathf.Clamp(GameSettings.CarID, 0, MAX_CAR);
		Debug.Log(GameSettings.CarID);
	}
	
    public void ChangeCarR() {
		GameSettings.CarID++;
		GameSettings.CarID = (int) Mathf.Clamp(GameSettings.CarID, 0, MAX_CAR);
		Debug.Log(GameSettings.CarID);
	}

    public void ChangeColorL() {
		GameSettings.ColorID--;
		GameSettings.ColorID = (int) Mathf.Clamp(GameSettings.ColorID, 0, MAX_CAR_COLOR);
		foreach (GameObject i in playerPrefabs) {
			i.SetActive(false);
		}
		playerPrefabs[GameSettings.ColorID].gameObject.SetActive(true);
		Debug.Log(GameSettings.ColorID);
	}
	
    public void ChangeColorR() {
		GameSettings.ColorID++;
		GameSettings.ColorID = (int) Mathf.Clamp(GameSettings.ColorID, 0, MAX_CAR_COLOR);
		foreach (GameObject i in playerPrefabs) {
			i.SetActive(false);
		}
		playerPrefabs[GameSettings.ColorID].SetActive(true);
		Debug.Log(GameSettings.ColorID);
	}
	
}
}
