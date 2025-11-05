using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu(fileName = "Ui Text Preset", menuName = "Ui Text Presets/Ui Text Preset", order = 1)]
public class TextPresets : ScriptableObject
{
    [Header("Car Select Button Text")]
    public string BuyCarButtonText;
    [Space(10)]
    public string SelectButtonTextPc;
    public string SelectButtonTextXbox;
    public string SelectButtonTextPS;

    [Header("Interact Notification Text PC")]
    public string StartDeliveryTextPc;
    public string EndDeliveryTextPc;
    public string DeliverPizzaTextPc;
    public string EnterGarageTextPc;
    public string RefuelCarTextPc;
    public string StartDialougeTextPc;
    public string StartQuestTextPc;
    [Header("Interact Notification Text Xbox")]
    public string StartDeliveryTextXbox;
    [Header("Interact Notification Text PS")]
    public string StartDeliveryTextPS;
    
    



}
