using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;

public class CustsceneManager : MonoBehaviour
{
    public static CustsceneManager instance;

    private PlayableDirector playableDirector;

    [SerializeField] PlayableAsset[] Cutscenes;

    //stores most recently played cutscene because unity clears 
    //playable asset once its finished playing
    private PlayableAsset lastCutscene;

    [SerializeField] DialogueTrigger[] cutsceneDialougeTriggers;

    [SerializeField] QuestTrigger[] questTriggers;

    [Header("Cutscene Assets")]
    [SerializeField]GameObject Cutscene1Car;


    

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else 
        { 
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playableDirector = GetComponent<PlayableDirector>();

        //This triggers dialouge with mum when the intro sequence (cutscene 1) is over
        playableDirector.stopped += CutsceneEnded;
        
        DialogueManager.OnDialogueFinished += () => StartCoroutine(WaitBeforeTriggeringQuest(TriggerQuest1));
    }

    
    private void Update()
    {
       //get button
       //add button hold time
        if(Input.GetKey(KeyCode.Space))
        {
           SkipCutscene();
        }
    }
    

    public  void TriggerCutscene(PlayableAsset cutscene, float timeOfDay, bool freezeTime, bool shouldSetCutsceneState = true, bool setTimeOfDay = true)
    {
        playableDirector.playableAsset = cutscene;

        lastCutscene = playableDirector.playableAsset;

        if (shouldSetCutsceneState == true)
        {
            GameManager.instance.SetCutsceneState();
        }
        
        GameManager.instance.EnableDisableMapTriggers(false);

        CanvasController.instance.EnableDisableGameplayUi(false);
        
        GameManager.instance.ToggleFreeLookCamera(true, true);

        if (setTimeOfDay == true)
        {
            LightingManager.instance.SetTimeOfDay("TriggerCutscene/Cutscene Manager", timeOfDay, freezeTime);
        }

        playableDirector.Play();
    }

    
    void SkipCutscene()
    {
        if (playableDirector != null && playableDirector.state == PlayState.Playing)
        {
            //This is hard coded because the way that the 1st cutscene plays out
            //It ensures that the cutscene skips to the part where the dialouge starts
            //rather than just ending
            if (playableDirector.playableAsset == Cutscenes[0] && playableDirector.time < 42)
            {
                playableDirector.time = 42;
                playableDirector.Evaluate();
                playableDirector.Play();
            }
        }
    }

     void CutsceneEnded(PlayableDirector _playableDirector)
    {
        if(lastCutscene == Cutscenes[0])
        {
            TriggerCutscene1Dialouge();
            CanvasController.instance.ToggleCutscene1UI(false);
        }
    }


    public void TriggerCutscene1() 
    { 
        if(playableDirector == null) 
        {
            Debug.LogError("playableDirector is null");
            return;
        } 
        else if(playableDirector.playableAsset == null)
        {
            Debug.LogError("no cutscene is assigned is null");
            return;
        }
        
        CanvasController.instance.ToggleCutscene1UI(true);
        TriggerCutscene(Cutscenes[0], 20, false);
        SoundManager.instance.ToggleAmbientSounds(false);
    }



   
    private void TriggerCutscene1Dialouge()
    {
       if(cutsceneDialougeTriggers[0] != null)
       {
            cutsceneDialougeTriggers[0].StartDialouge(true);
       } 
    }

    private void TriggerQuest1()
    {
       
            if(questTriggers[0] != null && lastCutscene == Cutscenes[0]) 
            {
                if(Cutscene1Car != null)
                {
                    Destroy(Cutscene1Car);
                }
                
                questTriggers[0].TriggerQuest();
               
                GameManager.instance.ToggleFreeLookCamera(false , false);
                SoundManager.instance.ToggleAmbientSounds(true);

                //to ensure quest 1 isnt triuggered again
                lastCutscene = null;
        
            }
    }

    IEnumerator WaitBeforeTriggeringQuest(Action TriggerQuestMethod)
    {
        yield return new WaitForSeconds(CanvasController.instance.Return_Cutscene_End_Fade_Length()/2);
        TriggerQuestMethod?.Invoke();
    }
}
