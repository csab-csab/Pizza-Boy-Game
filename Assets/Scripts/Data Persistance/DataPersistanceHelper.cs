using UnityEngine;

public class DataPersistanceHelper : MonoBehaviour
{
    //this script is used call functions in data persistance manager when the main scene is loaded
    //it is also used where we need static references, such as the buttons in settings, to be able to save on exit

    private void Start()
    {
        DataPersistanceManager.instance.FindAndAssignAllDataPersistanceObjects();
        DataPersistanceManager.instance.LoadSettingsData();
        DataPersistanceManager.instance.LoadGame();
    }

    public void SaveSettingsData()
    {
        DataPersistanceManager.instance.SaveSettingsData();
        
        if (CanvasController.instance != null)
        {
            CanvasController.instance.ShowSavedGameText();    
        }
    }

    public void SaveGameData()
    {
        DataPersistanceManager.instance.SaveGame();
        
        if (CanvasController.instance != null)
        {
            CanvasController.instance.ShowSavedGameText();    
        }
    }

}
