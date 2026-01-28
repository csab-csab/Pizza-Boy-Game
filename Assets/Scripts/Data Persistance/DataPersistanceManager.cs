using System.ComponentModel;
using UnityEngine;

public class DataPersistanceManager : MonoBehaviour
{
    //Singleton Reference
    public static DataPersistanceManager instance;

    //Singletont assignment
    void Awake()
    {
        if(instance != null)
        {
            Destroy(this.gameObject);
        }

        instance = this;     
    }
    
    void NewGame()
    {
        
    }
}
