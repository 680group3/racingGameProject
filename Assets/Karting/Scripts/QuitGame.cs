using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void quitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
