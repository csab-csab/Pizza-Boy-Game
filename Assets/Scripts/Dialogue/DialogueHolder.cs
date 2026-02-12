using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueHolder : MonoBehaviour
{
    public static DialogueHolder instance;

    #region Delivery Dialouge Holder

    [Header("Delivery Dialouge Holder")]
    //List of scriptable objs for pre and post delivery chit chat
    [SerializeField] private List<DialougeScriptable> DeliveryDialougeList = new List<DialougeScriptable>();
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

    /// <summary>
    /// for index use the following: 
    /// totalDelis var completed == pre delivery chat for that delivery
    /// totalDelis var + 1 post deli chat
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Dialouge ReturnDeliveryDialouge(int index)
    {
       if(index > DeliveryDialougeList.Count - 1) 
       {
            int random = Random.Range(1, DeliveryDialougeList.Count);
            return DeliveryDialougeList[random].Dialouge;
       }
        
        return DeliveryDialougeList[index].Dialouge;
    }
}
