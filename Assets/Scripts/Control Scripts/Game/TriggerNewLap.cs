using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerNewLap : MonoBehaviour
{
    public void OnTriggerExit(Collider other)
    {
        if (GameManager.instance.Gamemode == GameManager.gamemode.Track)
        {
            GameManager.instance.EndLapTimer();
            GameManager.instance.StartLapTimer();
        }
    }
}
