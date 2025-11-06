using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class QuestPrerequisite : MonoBehaviour
{
    [SerializeField] public static int DreamCarCost = 6000;
    [SerializeField] GameObject Quest2Trigger;
    public static GameObject Quest2TriggerRef;

    private void Start()
    {
        Quest2TriggerRef = Quest2Trigger;
    }
}

