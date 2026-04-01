using System.ComponentModel;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class DataPersistanceManager : MonoBehaviour
{
    //Singleton Reference
    public static DataPersistanceManager instance{get; private set;}
    
    [Header("File Storage Config")]
    [SerializeField]string gameDataFileName;
    [SerializeField]string settingsDataFileName;

    //Reference to file data handler
    private FileDataHandler fileDataHandler;

    //Game Data
    private GameData gameData;
    
    //Settings Data
    private SettingsData settingsData;

    
    //List of all the scripts that implement data persistance interface
    [SerializeField]List<IDataPersistance> dataPersistanceObjects = new List<IDataPersistance>();
    
    //Singleton assignment
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        DataPersistanceManager.instance.FindAndAssignAllDataPersistanceObjects();
        this.fileDataHandler = new FileDataHandler(Application.persistentDataPath, gameDataFileName, settingsDataFileName);
        LoadSettingsData();
        LoadGame();
        SaveGame();
    }

    public void NewGame()
    {
        this.gameData = new GameData();
        SaveGame();
    }

    public void NewSettings()
    {
      //print("No settings data was found. Creating new...");
      this.settingsData = new SettingsData();
    }

    public void LoadGame()
    {
        this.gameData = fileDataHandler.LoadData<GameData>();
        if(this.gameData == null)
        {
            NewGame();
            return;
        }

        foreach(IDataPersistance data in dataPersistanceObjects)
        {
            data.LoadGameData(gameData);
            data.LoadSettingsData(settingsData);
        }
        
        print("Loaded game data.");
    }

    public void SaveGame()
    {
        foreach(IDataPersistance data in dataPersistanceObjects)
        {
            data.SaveGameData(ref gameData);
            data.SaveSettingsData(ref settingsData);
            print("Saved game data");
        }
        fileDataHandler.SaveData<GameData>(gameData);
    }
    
    public void LoadSettingsData()
    {
        this.settingsData = fileDataHandler.LoadData<SettingsData>();
        if(this.settingsData == null)
        {
            NewSettings();
            return;
        }
        
        foreach(IDataPersistance data in dataPersistanceObjects)
        {
            data.LoadSettingsData(settingsData);
            //print("Loaded settings data");
        }
    }

    public void SaveSettingsData()
    {
        foreach(IDataPersistance data in dataPersistanceObjects)
        {
            data.SaveSettingsData(ref settingsData);
            //print("Saved settings data");
        }
        fileDataHandler.SaveData<SettingsData>(settingsData);
    }

    public void FindAndAssignAllDataPersistanceObjects()
    {
        this.dataPersistanceObjects.Clear();
        IEnumerable<IDataPersistance> persistances = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None).OfType<IDataPersistance>();
        this.dataPersistanceObjects = new List<IDataPersistance>(persistances);
    }
}
