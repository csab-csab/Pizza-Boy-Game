using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialouge dialouge;

    [SerializeField] bool DisableDialougeTriggerObj = false;
    [SerializeField] bool FreezePlayer = false;
    [SerializeField] GameObject PostDialougeTrigger;
    
    [SerializeField]bool enableSavingsTextPostDia = false;
    
  
    
    private void Start()
    {
        DialogueManager.OnDialogueFinished += TriggerEventAfterDia;    
    }

    public void StartDialouge(bool fadeEffectonEnd = false) 
    {
        GameManager.instance.DestroyPointerArrow();
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

    //please ensure that you disable the object when its not needed as this is a global even, and will always get called,
    //regardless if this was the correct dialogue or not
    private void TriggerEventAfterDia() 
    {
        if(PostDialougeTrigger != null) 
        { 
            PostDialougeTrigger.SetActive(true);
        }

        if(!string.IsNullOrWhiteSpace(dialouge.PostDialogueInstructions))
        {
            CanvasController.instance.UpdateQuestObjectiveText(dialouge.PostDialogueInstructions);
            CanvasController.instance.ShowObjectiveText(true);
        }

        if(enableSavingsTextPostDia)
        {
            CanvasController.instance.ToggleSavingsText(true);
        }
    }
}
