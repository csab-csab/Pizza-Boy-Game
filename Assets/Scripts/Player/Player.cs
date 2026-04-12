using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDataPersistance
{
    private float Money;
    private float xp;
    private float xpToNextLevel;
    private float xpLevel;

    public void GiveMoney(float amount)
    {
        Money += amount;
        DataPersistanceManager.instance.SaveGame();
    }

    public void TakeMoney(float amount) 
    {
        Money -= amount;
        DataPersistanceManager.instance.SaveGame();
    }


    public void SaveGameData(ref GameData gameData)
    {
        gameData.playerMoney = Money;
    }

    public void LoadGameData(GameData gameData)
    {
        gameData.playerMoney = this.Money;
    }

    public void SaveSettingsData(ref SettingsData settingsData)
    {
        
    }

    public void LoadSettingsData(SettingsData settingsData)
    {
        
    }
}
