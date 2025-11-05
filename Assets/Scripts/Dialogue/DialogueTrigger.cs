using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialouge dialouge;

    [SerializeField] bool DisableDialougeTriggerObj = false;
    [SerializeField] bool FreezePlayer = false;
    [SerializeField] GameObject PostDialougeTrigger;
  
    
    private void Start()
    {
        DialogueManager.OnDialogueFinished += TriggerEventAfterDia;    
    }

    public void StartDialouge(bool fadeEffectonEnd = false) 
    {
        DialogueManager.instance.StartDialouge(dialouge, FreezePlayer);
        //need both of these for compatiblity
        DialogueManager.instance.fadeOutEffectAfterDialouge(fadeEffectonEnd);
        DialogueManager.instance.fadeOutEffectAfterDialouge(dialouge.triggerFadeAfterDialouge);

        if (DisableDialougeTriggerObj) 
        { 
            this.gameObject.SetActive(false);
        }
    }

    public string ReturnSpeakerName() 
    {
        return dialouge.Name;
    }

    private void TriggerEventAfterDia() 
    {
        if(PostDialougeTrigger != null) 
        { 
            PostDialougeTrigger.SetActive(true);
        }
    }
}
