using System.Collections;
using System.Collections.Generic;
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
            if (this.gameObject.GetComponent<QuestManager>() != null) 
            {
                print("Quest manager on this go:" + this.gameObject.GetComponent<QuestManager>() != null);
                //this.enabled = false;
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
            print("Triggered through collider");
        }
    }
}
