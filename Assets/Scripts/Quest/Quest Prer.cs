using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class QuestPrerequisite : MonoBehaviour, IDataPersistance
{
    [SerializeField] public static int DreamCarCost = 6000;
    [SerializeField] QuestTrigger Quest1Trigger;
    [SerializeField] GameObject Quest2Trigger;
    public static GameObject Quest2TriggerRef;

    #region Save/Load Values
    private int currentQuestProgress = 0;
    #endregion  

    private void Start()
    {
        Quest2TriggerRef = Quest2Trigger;
    }

    private void TriggerQuest1()
    {
        Quest1Trigger.TriggerQuest();
    }

    public static void EnableQuest2()
    {
        Quest2TriggerRef.gameObject.SetActive(true);
        GameManager.instance.EnableDisableMapTriggers(false, false, false, true);
        CanvasController.instance.UpdateNotificationText("CONGRATS! You have saved enough to buy your dream car!");
        GameManager.instance.SpawnPointerArrow(GameManager.ArrowType.Objective, QuestPrerequisite.Quest2TriggerRef);
    }

    public void SaveGameData(ref GameData gameData)
    {
        throw new System.NotImplementedException();
    }

    public void LoadGameData(GameData gameData)
    {
        this.currentQuestProgress = gameData.currentQuestProgress;
        float playerMoney = gameData.playerMoney;

        if (this.currentQuestProgress == 0)
        {
            TriggerQuest1();
        }
        else if (this.currentQuestProgress == 1)
        {

            if (playerMoney >= DreamCarCost)
            {
                EnableQuest2();
            }
            else
            {
                CanvasController.instance.UpdateNotificationText("Keep doing deliveries!\n" +
                $"You are only{DreamCarCost - playerMoney} away from your dream car! ");
            }
        }
        else if (this.currentQuestProgress == 2)
        {
            //Enable quest 3
        }
        //Same for quest 3
    }

    public void SaveSettingsData(ref SettingsData settingsData)
    {
        throw new System.NotImplementedException();
    }

    public void LoadSettingsData(SettingsData settingsData)
    {
        throw new System.NotImplementedException();
    }
}

