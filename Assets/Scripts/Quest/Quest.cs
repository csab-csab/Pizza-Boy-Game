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
    public enum ObjectiveType{None ,ReachPoint, ReachValue, Dialouge, Delivery, Cutscene};

    public ObjectiveType type;

    public string Description;

    public GameObject pointToReach;

    public float valueToReach;

    public Dialouge dialouge;

    public int pizzasToDeliver;
    public float timeToDeliver;

    public PlayableAsset Cutscene;

    public enum VariableToSet {None, CarHealth, Money, Fuel};
    public VariableToSet variableToSet;    
    public float valueToSet;
}

  [System.Serializable]
  public class QuestExtras
  {
   public GameObject carToSpawn;
   public Transform carTransformToSpawnOn;
   public Transform dialougeCameraPosition;
    [Header("This is the int value of the objective where the dialogue cam \n" +
        "is needed eg.: if objective 6 is the obj that needs it then this \n" +
        "is 6 and so on")]
    public int objectiveIndxForCamPos;
  }