using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class QuestPrerequisite : MonoBehaviour, IDataPersistance
{
    public static QuestPrerequisite instance;
    
    //this script handles the enabling of quest triggers as well as saving and loading the current quest progress
    //also handles launching the game for the first time
    
    [Header("Refernce to the script that triggers first cutscene")]
    [SerializeField] StartGameDebug startGameDebug;
    [Space(10)]
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

    //these coroutines are there to ensure the player has fully loaded in
    //before showing messaegs or triggering quests; avoiding errors

    //Starts Game if loaded from main menu; it waits for player to exit car select
    private IEnumerator StartGameAfterLoading(float playerMoney)
    {
       yield return new WaitForSecondsRealtime(2);
       
       startGameDebug.StartGameFreemodeProd();
      
       yield return new WaitUntil(() =>  !GameManager.instance.ReturnCarSelectStatus());
       
       if (this.currentQuestProgress == 1)
       {
           StartCoroutine(EnableQuest2OrShowMessage(playerMoney));
       }
       else if (this.currentQuestProgress == 2)
       {
           EnableQuest3();
       }
    }
    
    private IEnumerator TriggerQuest1()
    {
        //launches first cutscene
        yield return new WaitForSeconds(3);
        startGameDebug.TriggerCutscene();
    }

    private IEnumerator EnableQuest2OrShowMessage(float playerMoney)
    {
        yield return new WaitForSeconds(3);
        
        if (playerMoney >= DreamCarCost)
        {
            EnableQuest2();
        }
        else
        {
            CanvasController.instance.UpdateNotificationText("Keep doing deliveries!\n" +
                                                             $"You are only ${DreamCarCost - playerMoney} away from your dream car! ");
            //Ensures delivery remains accessible
            GameManager.instance.EnableDisableMapTriggers(true);
        }
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
        
        if (this.currentQuestProgress == 0)
        {
            StartCoroutine(TriggerQuest1());
        }
        else
        {
            StartCoroutine(StartGameAfterLoading(gameData.playerMoney));
        }
    }

    public void SaveSettingsData(ref SettingsData settingsData)
    {
        
    }

    public void LoadSettingsData(SettingsData settingsData)
    {
       
    }
}

