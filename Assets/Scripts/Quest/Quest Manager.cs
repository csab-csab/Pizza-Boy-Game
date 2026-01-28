using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class QuestManager : MonoBehaviour
{
    public Quest quest;

    //Can be used to see if there is currently a quest active
    public static bool isQuestActive { get; private set; } = false;

    #region Events 
    public delegate void QuestStarted();
    public static QuestStarted OnQuestStarted;

    public delegate void ObjectiveCompleted(Objective.ObjectiveType type);
    public static ObjectiveCompleted OnObjectiveCompleted;

    public delegate void QuestCompleted();
    public static QuestCompleted OnQuestCompleted;

    public delegate void ObjectiveFailed(string FailureReason);

    public static ObjectiveFailed OnObjectiveFailed;

    public delegate void QuestFailed();
    public static QuestFailed OnQuestFailed;

    //This event is used for some objectives that need to set the value of something
    //For example, in quest 1 we need to set the fuel low after the delivery to teach the player how
    //to refuel
    public delegate void SetValue(Objective.VariableToSet variable, float Value);
    public static SetValue OnSetValue;    
    #endregion

    #region Retry
    //this variable forces the player to retry no matter what 
    private bool allowedToExit = false;
    [SerializeField] float timeUntilAutoStart = 8f;
    [SerializeField] float TimeUntilTermination = 8f;
    private float currentTimeUntilRestart;
    //used after succesful completion of quest for disabling quest object so ui displays correctly and so there is only one quest active at a time
    private float currentTimeUntilTermination;
    [SerializeField]private bool isQuestOver = false;
    private bool autoRestart = false;
    Quest lastQuest;
    #endregion

    #region Reach Value
    private bool reachValueObjective = false;
    private float currentValue;
    private float desiredValue;
    #endregion

    private void Start()
    {
        OnObjectiveCompleted += CheckObjectiveComplete;
        OnObjectiveFailed += FailQuest;
        OnQuestFailed += StartAutoRestartCountdown;
        DialogueManager.OnDialogueFinished += () => CheckObjectiveComplete(Objective.ObjectiveType.Dialouge);

    }

    private void Update()
    {
        if (autoRestart && currentTimeUntilRestart > 0)
        {
            currentTimeUntilRestart -= Time.deltaTime;
            CanvasController.instance.UpdateQuestOverTimerBar(currentTimeUntilRestart, timeUntilAutoStart);
        }
        
        if (autoRestart && currentTimeUntilRestart <= 0 || autoRestart && Input.GetKey(KeyCode.Return))
        {
            CanvasController.instance.UpdateQuestOverTimerBar(0, 1);
            autoRestart = false;
            RestartQuest();
        }

        if (isQuestOver && currentTimeUntilTermination > 0) 
        {
           currentTimeUntilTermination -= Time.deltaTime;
            CanvasController.instance.UpdateQuestOverTimerBar(currentTimeUntilTermination, TimeUntilTermination);
        }
        else if(isQuestOver && currentTimeUntilTermination <= 0)
        {
            isQuestOver = false;
            TerminateQuest();
        }

        if (reachValueObjective == true)
        {
            //this method checks every frame if the desired value 
            //is being reached, if the current objective is reach value

            CheckReachValue();
        }

       
    }

    public void StartQuest()
    {
        print("quest started");
        if (quest != null && !isQuestActive)
        {
            GameManager.instance.AssignQuestManager(this);
            GameManager.instance.SetPlayState();
            CanvasController.instance.EnableDisableGameplayUi(true);
            isQuestActive = true;

            //Tweak this method in the following way:
            //if there is an assigned car in quest extras class
            //spawn said car
            //else spawn current car in quest start location
            if (quest.questExtras.carToSpawn != null)
            {
                SpawnQuestCar();
            }
            else
            {
                //SpawnCurrentCarQuest();
            }

            //triggers quest started event
            CanvasController.instance.UpdateQuestTitleText(quest.QuestName);
            OnQuestStarted?.Invoke();
            CompleteObjective();

        }
        else
        {
            Debug.LogError("Quest is null. Please assign quest in the inspector! ");
        }
    }

    private void RestartQuest()
    {
        if (lastQuest == null)
        {
            Debug.LogError("No last quest found");
            return;
        }

        quest = lastQuest;
        lastQuest.ResetQuest();
        CanvasController.instance.FadeQuestOverTextNow();
        CanvasController.instance.Play_Cutscene_End_Fade();
        StartCoroutine(WaitBeforeRestarting());
    }
    
    private void CompleteQuest()
    {
        GrantRewards();
        isQuestOver = true;
        currentTimeUntilTermination = TimeUntilTermination;

        CanvasController.instance.UpdateQuestOverText("Quest Completed!", false);
        CanvasController.instance.IntialiseQuestOverTimerBar(false);

        if(quest.postQuestTriggerPoint != null) 
        { 
            quest.postQuestTriggerPoint.SetActive(true);
            CanvasController.instance.UpdateQuestObjectiveText(quest.postQuestObjectiveMessage);
            CanvasController.instance.ShowObjectiveText(true);

            if(quest.pointArrowToPostQuestPoint)
            {
                GameManager.instance.SpawnPointerArrow(GameManager.ArrowType.Objective, quest.postQuestTriggerPoint);
            }
        }

        OnQuestCompleted?.Invoke();

        isQuestActive = false;
    }

    //after everything is complete, disables quest
    //this is to ensure only one quest manager is active at one time
    private void TerminateQuest() 
    {
        print("Terminating quest");
        quest = null;
        this.gameObject.SetActive(false);
    }

    private void FailQuest(string reason)
    {
        lastQuest = quest;

        if (quest.id == 0)
        {
            allowedToExit = false;
        }

        GameManager.instance.DisableCar(true);
        GameManager.instance.EndDelivery();
        DialogueManager.instance.ForceEndDialouge();
        CanvasController.instance.EnableDisableGameplayUi(false);
        CanvasController.instance.ClearInteractUiText();

        quest = null;

        CanvasController.instance.UpdateQuestOverText("Quest Failed!", true);
        CanvasController.instance.UpdateQuestOverSubText(" 'Enter'-> Retry?");
        CanvasController.instance.IntialiseQuestOverTimerBar(true);

        OnQuestFailed?.Invoke();

        isQuestActive = false;
    }

    private void CompleteObjective()
    {
        if (quest == null) return;

        Objective objective = quest.NextObjective();

        GameManager.instance.EnableCar();

        if (objective != null)
        {

            GameManager.instance.DestroyPointerArrow();

            switch (objective.type)
            {
                case Objective.ObjectiveType.None:
                    return;

                case Objective.ObjectiveType.ReachPoint:
                    EnablePointToReach(objective.pointToReach);
                    GameManager.instance.SpawnPointerArrow(GameManager.ArrowType.Objective, objective.pointToReach);
                    break;
                case Objective.ObjectiveType.Dialouge:
                    GameManager.instance.DisableCar(false);
                    if(quest.questExtras.dialougeCameraPosition != null && quest.currentObjectiveIndex == quest.questExtras.objectiveIndxForCamPos) 
                    {
                        GameManager.instance.ToggleFreeLookCamera(true, true,
                            quest.questExtras.dialougeCameraPosition.position, 
                            quest.questExtras.dialougeCameraPosition.rotation);
                     
                        //creates new event handler
                        DialogueManager.DialougeFinished disableFreeLook = null;
                        
                        //assigns newly crreated event handler to an anonymous function
                        //that disables camera and unsubcribes this event from the OnDialogue finished event
                        disableFreeLook = () =>
                        {
                            // Disables camera
                            GameManager.instance.ToggleFreeLookCamera(false);
                            
                            #if UNITY_EDITOR
                            print("event is working...");
                            #endif

                            // Unsubscribe this handler
                            DialogueManager.OnDialogueFinished -= disableFreeLook;
                        };
                        
                        //subscribes disableFreelook to on dialogue finished 
                        DialogueManager.OnDialogueFinished += disableFreeLook;
                    }
                    DialogueManager.instance.StartDialouge(objective.dialouge);
                    break;
                case Objective.ObjectiveType.Delivery:
                    GameManager.instance.DisableCar(false);
                    GameManager.instance.AssignPizzasToDeliver(objective.pizzasToDeliver);
                    GameManager.instance.StartDelivery();
                    break;
                case Objective.ObjectiveType.ReachValue:
                    reachValueObjective = true;
                    desiredValue = quest.ReturnCurrentObjective().valueToReach;
                    break;
                case Objective.ObjectiveType.Cutscene:
                    PlayableAsset cutscene = quest.ReturnCurrentObjective().Cutscene;
                    if (cutscene != null)
                    {
                        CustsceneManager.instance.TriggerCutscene(quest.ReturnCurrentObjective().Cutscene, 0, false, false, false);
                    }
                   
                    //this is specific code for the first mission
                    if(quest.id == 0) 
                    {
                        EnableRefuelTriggerForObjective();
                    }
                    break;
            }

            CanvasController.instance.UpdateQuestObjectiveText(objective.Description);

            switch (objective.variableToSet)  
            { 
                case Objective.VariableToSet.None:

                    break;

                case Objective.VariableToSet.Fuel:
                    OnSetValue(Objective.VariableToSet.Fuel, objective.valueToSet);
                    break; 

            }

            //to avoid showing the quest name and objetive name the same time
            //canvas controller will handle showing objective once quest title fades out
            if (quest.currentObjectiveIndex > 0)
            {
                CanvasController.instance.ShowObjectiveText(true);
            }
        }
        else
        {
            #if UNITY_EDITOR
            Debug.LogWarning("No more objectives");
            #endif

            CompleteQuest();
        }

    }

    //method is trigger by failed quest event and starts countdown
    //to restart quest
    private void StartAutoRestartCountdown()
    {
        currentTimeUntilRestart = timeUntilAutoStart;
        autoRestart = true;
    }

    private void EnablePointToReach(GameObject poinToReach)
    {
        poinToReach.SetActive(true);
    }

    private void GrantRewards()
    {
        
        if (quest != null)
        {
            int moneyReward = quest.MoneyReward;

            GameManager.instance.ReturnPlayerManager().GiveMoney(quest.MoneyReward);

            CanvasController.instance.UpdateQuestOverSubText("+$ " + moneyReward);
        }
    }

    //made this method because i only want objecitve complete to be called on dialouge finished if there was 
    //a dialouge objective in quest
    //realised its useful to check if the objective actually was the one that was meant to be completed
    //rather than events just randomly calling objective completed
    //this method is for extra safety really
    private void CheckObjectiveComplete(Objective.ObjectiveType triggeredType)
    {
        if (quest == null) return;

        Objective currentObjective = quest.ReturnCurrentObjective();

        if (currentObjective == null) return;

        if (currentObjective.type == triggeredType)
        {
            if (currentObjective.type == Objective.ObjectiveType.Dialouge)
            {
                StartCoroutine(WaitBeforeStartingNewObjective());
            }
            else
            {
                CompleteObjective();
            }
        }
    }

    private void CheckReachValue() 
    { 
        if(currentValue >= desiredValue) 
        {
            OnObjectiveCompleted?.Invoke(Objective.ObjectiveType.ReachValue);
            reachValueObjective = false;
        }
    }

    //this method is used by scripts to feed the current value 
    //so it can be compared with the value needed to be reached
    //for example, the car script will feed the current refuel value
    //to this which can be compared to the desired value from the quest prefab
    public void FeedReachValue(float fedValue) 
    { 
        currentValue = fedValue;
    }

    public void SpawnQuestCar()
    {
        QuestExtras extras = quest.questExtras;

        if (quest.questExtras.carToSpawn != null &&
            quest.questExtras.carTransformToSpawnOn != null)
        {
            //spawn the car in 1st quest with less fuel
            if (quest.id == 0)
            {
                CarSelectorScript.triggerSpawnCar?.Invoke(extras.carTransformToSpawnOn,
                extras.carToSpawn, 4,
                  nameof(SpawnQuestCar));
            }
            else 
            {
                CarSelectorScript.triggerSpawnCar?.Invoke(extras.carTransformToSpawnOn,
                extras.carToSpawn, 1,
                nameof(SpawnQuestCar));
            }
        }
    }

    public void SpawnCurrentCarQuest()
    {
        throw new NotImplementedException();
    }

    private void EnableRefuelTriggerForObjective() 
    { 
      GameManager.instance.EnableDisableMapTriggers(false, false, false, true);
    }

    //uses end of cutscene/dialouge anim length to wait before triggering new objective
    //after dialouge
    IEnumerator WaitBeforeStartingNewObjective()
    {
        yield return new WaitForSeconds(CanvasController.instance.Return_Cutscene_End_Fade_Length() + 0.5f);
        CompleteObjective();
    }

     IEnumerator WaitBeforeRestarting()
     {
        yield return new WaitForSeconds(CanvasController.instance.Return_Cutscene_End_Fade_Length());
        StartQuest(); 
    }

    private void OnDisable()
    {
        Debug.LogError("Something disabled this script...");
    }
}