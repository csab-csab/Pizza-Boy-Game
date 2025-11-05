using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameDebug : MonoBehaviour
{
    public enum CarToSpawn{ Emma, Raiden};


    public void StartGameFreeMode() 
    {
        CanvasController.instance.ToggleCarSelectStartGame(true);
    }

    public void TriggerCutscene() 
    {
        CustsceneManager.instance.TriggerCutscene1();
        CanvasController.instance.ToggleStartGameUi(false);
    }
    
    public void StartGameFreemodeEmma()
    {
        GameManager.instance.StartGameFreemode(CarToSpawn.Emma);
        CanvasController.instance.ToggleStartGameUi(false);
    }

    public void StartGameFreemodeRaiden()
    {
        GameManager.instance.StartGameFreemode(CarToSpawn.Raiden);
        CanvasController.instance.ToggleStartGameUi(false);
    }
}
