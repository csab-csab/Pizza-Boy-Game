using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{

    //Create an on dialouge end event!
    public static DialogueManager instance;
    #region Current Dialouge Variables
    //is there currently a conversation?
    private bool activeDialouge = false;
    private int dialougeIndex = 0;

    Coroutine writingLine = null; 

    [Header("The Speed at which each letter of dialouge is display in seconds.")]
    public float displaySpeed = 0.04f;

    private Dialouge cachedDialogue;

    bool fadeOutEffectAfterDia = false;

    //Events
    public delegate void DialougeFinished();
    public static DialougeFinished OnDialogueFinished;

    private GameObject temp_character;

    //used to disable player movement through dialouge trigger script
    private bool freezePlayer = false;
    #endregion

  

    private void Awake()
    {
        if (instance != null) 
        { 
            Destroy(instance);
            instance = this;
        }
        else 
        { 
            instance = this; 
        }
    }

    public void StartDialouge(Dialouge dialouge, bool FreezePlayer = false)
    {
        if (activeDialouge)
        {
            Debug.LogError("There is already active dialouge!");
           return;
        }

        activeDialouge = true;

        freezePlayer = FreezePlayer;

        if (freezePlayer) 
        { 
            GameManager.instance.DisableCar(true);
        }

        if (dialouge.triggerFadeAfterDialouge) 
        { 
            fadeOutEffectAfterDia = true;
        }

        cachedDialogue = dialouge;
        
        dialougeIndex = 0;
        
        SpawnDespawnCharacter(false, dialouge);

  
        
        CanvasController.instance.ToggleDialougeUi(true);
        writingLine = StartCoroutine(WriteOutLine(cachedDialogue.Sentences[dialougeIndex]));
    }


    private void ForwardDialouge() 
    {
        //Completes sentence allowing player to skip the scroll effect
        //returns as to not skip to next line of dialouge
        if(writingLine != null) 
        {
            StopCoroutine(writingLine);
            writingLine = null;
            CanvasController.instance.DialougeClearSentence();
            CanvasController.instance.UpdateDialogue(cachedDialogue.Name, ' ',cachedDialogue.Sentences[dialougeIndex]);
            return;
        }
        
         dialougeIndex++;
        
        if (dialougeIndex >= cachedDialogue.Sentences.Length) 
        { 
            EndDialouge();
            return;
        }
        else 
        {
            writingLine = StartCoroutine(WriteOutLine(cachedDialogue.Sentences[dialougeIndex]));
        }
    }

    //Creates a nice type out effect for dialouge by printing sentences line by line
    IEnumerator WriteOutLine(string sentence) 
    {
       CanvasController.instance.DialougeClearSentence();

       foreach(char letter in sentence.ToCharArray()) 
        {
            CanvasController.instance.UpdateDialogue(cachedDialogue.Name, letter);
            yield return new WaitForSeconds(displaySpeed);
        }

       writingLine = null;
    }

    private void EndDialouge() 
    {
        activeDialouge = false;
        dialougeIndex = 0;
        CanvasController.instance.ToggleDialougeUi(false);
        
        SpawnDespawnCharacter(true);

        if(fadeOutEffectAfterDia)
        {
            CanvasController.instance.Call_Dialogue_End_Fade(OnDialogueFinished);
        }
        else
        {
            OnDialogueFinished?.Invoke();
            DisplayPostDialougeInstructions(cachedDialogue.PostDialogueInstructions);
        }

        if (freezePlayer) 
        {
            GameManager.instance.EnableCar();
            freezePlayer = false;
        }

       
        cachedDialogue = null;
    }

    /// <summary>
    /// Used to kill dialouge such as when car gets destroyed mid dialouge
    /// </summary>
    public void ForceEndDialouge() 
    {
        if (!activeDialouge) 
        { 
            return;
        }
        activeDialouge = false;
        dialougeIndex = 0;
        CanvasController.instance.ToggleDialougeUi(false);
        cachedDialogue = null;
    }

    private void Update()
    {
        if (activeDialouge && Input.GetButtonDown("Submit")
          || activeDialouge && Input.GetButtonDown("Fire1")) 
        {
            ForwardDialouge();
        }
    }

    public void fadeOutEffectAfterDialouge(bool enabled)
    {
        fadeOutEffectAfterDia = enabled;
    }

    private void SpawnDespawnCharacter(bool despawn,Dialouge dialouge = null)
    {
        if(despawn && temp_character != null)
        {
            Destroy(temp_character);
            return;
        }
        else if(despawn && temp_character == null)
        {
            Debug.LogError("No character to despawn.");
            return;
        }
       
        if(dialouge.characterToSpawn != null && dialouge.charTransformSpawnOn != null)
        {
            GameObject character = Instantiate(dialouge.characterToSpawn);
            character.transform.position = dialouge.charTransformSpawnOn.transform.position;
            character.transform.rotation = dialouge.charTransformSpawnOn.transform.rotation;

            temp_character = character;
        }
        else
        {
            Debug.LogError("No character assigned.");
            return;
        }
    }

    #region  Return Values
    /// <summary>
    /// Returns if there is any dialogue running
    /// </summary>
    /// <returns></returns>
    public bool CheckIsActiveDialogue()
    {
        return activeDialouge;
    }
    #endregion

    private void DisplayPostDialougeInstructions(string text) 
    {
        CanvasController.instance.UpdateQuestObjectiveText(text);
    }
}
