using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Playables;

[System.Serializable]
public class Quest
{
    [Header("Assign quest num here. \n first quest quest 0 etc.")]
    [SerializeField] int questId;
    //this is just the number of the quest
    //eg the first quest is 0
    public int id { get; private set; }

    public string QuestName;
    public int currentObjectiveIndex { get; private set; } = -1;

    public int totalObjectives { get; private set; }

    Objective currentObjective = null;

    [SerializeField] List<Objective> Objectives = new List<Objective>();

    public int MoneyReward;

    public GameObject postQuestTriggerPoint;

    public string postQuestObjectiveMessage = "This a post quest objective message.";

    public bool pointArrowToPostQuestPoint = false;

    public QuestExtras questExtras;

    public Objective NextObjective()
    {
        //minus 1 fixed it because objective starts at -1
        totalObjectives = Objectives.Count - 1;

        Debug.LogWarning("\n Total Objectives: " + totalObjectives + "\n objective index: " + currentObjectiveIndex);

        if (currentObjectiveIndex < totalObjectives)
        {
            currentObjectiveIndex++;

            currentObjective = Objectives[currentObjectiveIndex];

            return currentObjective;
        }
        else
        {
            return null;
        }
    }
    
    public Objective ReturnCurrentObjective()
    {
        return currentObjective;
    }

    public void ResetQuest()
    {
        currentObjectiveIndex = -1;
        //if current quest has a car to spawn 
        //Destroy current car so it can be respawned when quest is triggered
        //again
        if (questExtras != null && questExtras.carToSpawn != null)
        {
            GameManager.instance.ForceDestroyCurCar();
        }
    }

    public int ReturnQuestId() 
    { 
     return questId;
    }
}



[System.Serializable]
public class Objective
{
    public enum ObjectiveType{None ,ReachPoint, ReachValue, Dialouge, Delivery, Cutscene, ReachPointWithinTime};

    public ObjectiveType type;

    [Header("Objective description")]
    public string Description;

    public GameObject pointToReach;

    public float valueToReach;

    public Dialouge dialouge;
    
    [Header("Delivery Objective Values")]
    public int pizzasToDeliver;
    public float timeToDeliver;

    public PlayableAsset Cutscene;

    public enum VariableToSet {None, CarHealth, Money, Fuel, Time};
    public VariableToSet variableToSet;    
    public float valueToSet;
    
    #region  Reach point within time variables
    [Header("Reach point within time variables \n" +
            "(For point to reach, use the existing point to reach ref)"
            + " allowedTime is in seconds")]
    public float allowedTime;
    #endregion
}

  [System.Serializable]
  public class QuestExtras
  {
   public GameObject carToSpawn;
   public Transform carTransformToSpawnOn;
   public Transform[] dialougeCameraPositions;
   public float[] fieldOfViews ;
    [Header("This is the int value of the objective where the dialogue cam \n" +
        "is needed eg.: if objective 6 is the obj that needs it then this \n" +
        "is 6 and so on")]
    public int[] objectiveIndxsForCamPos;
    
    [Header("Used to enable certain objects at specified objective index")]
    public GameObject[] objsToEnable;
    public int[] objIndexForEnable;

  }