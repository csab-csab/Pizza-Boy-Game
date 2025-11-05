using UnityEngine;

public class Streetlight : MonoBehaviour
{
   public void HandleDestruction() 
   { 
        foreach(Transform child in transform) 
        { 
            child.gameObject.SetActive(false);
        }
   }
}
