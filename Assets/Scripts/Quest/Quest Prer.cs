using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class QuestPrerequisite : MonoBehaviour, IDataPersistance
{
    public static QuestPrerequisite instance;
    
    //this script handles the enabling of quest triggers as well as saving and loading the current quest progress
    
    [SerializeField] public static int DreamCarCost = 6000;
    [SerializeField] QuestTrigger Quest1Trigger;
    [SerializeField] GameObject Quest2Trigger;
    [SerializeField] GameObject Quest3Trigger;
    private static GameObject Quest2TriggerRef;

    #region Save/Load Values
    private int currentQuestProgress = 0;
    #endregion

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance);
        }
        else
        {
            instance = this;
        }
    }
    
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

    private void EnableQuest3()
    {
        Quest3Trigger.gameObject.SetActive(true);
        GameManager.instance.SpawnPointerArrow(GameManager.ArrowType.Objective, QuestPrerequisite.Quest2TriggerRef);
    }

    public void IncreaseQuestProgress()
    {
        currentQuestProgress++;
    }
    
    public void SaveGameData(ref GameData gameData)
    {
        gameData.currentQuestProgress = this.currentQuestProgress;
    }

    public void LoadGameData(GameData gameData)
    {
        this.currentQuestProgress = gameData.currentQuestProgress;
        //player money is needed here to calculate cost below
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
            EnableQuest3();
        }
    }

    public void SaveSettingsData(ref SettingsData settingsData)
    {
        
    }

    public void LoadSettingsData(SettingsData settingsData)
    {
       
    }
}

