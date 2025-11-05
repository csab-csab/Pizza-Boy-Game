using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialouge 
{
    public string Name;

    public GameObject characterToSpawn;
    public Transform charTransformSpawnOn;

    [TextArea]
    public string[] Sentences;

    public bool triggerFadeAfterDialouge;
    public string PostDialogueInstructions;
}
