using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReachPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag(GameManager.instance.PLAYER_CAR_TAG))
        {
            //trigger objective complete method through event
            QuestManager.OnObjectiveCompleted?.Invoke(Objective.ObjectiveType.ReachPoint);
            this.gameObject.SetActive(false);
        }
    }
}