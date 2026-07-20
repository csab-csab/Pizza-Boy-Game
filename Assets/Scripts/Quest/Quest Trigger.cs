using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    [Header("Assign correct quest manager here.")]
    [SerializeField]QuestManager questManager;

    public void TriggerQuest()
    {
        if (questManager != null)
        {
            questManager.StartQuest();
            questManager.gameObject.SetActive(true);
            if (this.gameObject.GetComponent<QuestManager>() != null) 
            {
                print("Quest manager on this go:" + this.gameObject.GetComponent<QuestManager>() != null);
            }
            else 
            {
                this.gameObject.SetActive(false);
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(GameManager.instance.PLAYER_CAR_TAG))
        {
            TriggerQuest();
            print($"Triggered through collider, {this.gameObject.name}");
        }
    }
}
