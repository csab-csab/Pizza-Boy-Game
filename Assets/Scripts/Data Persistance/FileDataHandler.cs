using UnityEngine;
using IO_Path = System.IO.Path;
using System;
using System.IO;

public class FileDataHandler
{
    private string dataDirPath = "";
    private string gameDatafileName = "";
    private string settingsDataFileName = "";

    public FileDataHandler(string _dataDirPath, string _gameDataFileName, string _settingsDataFileName)
    {
        this.dataDirPath = _dataDirPath;
        this.gameDatafileName = _gameDataFileName;
        this.settingsDataFileName = _settingsDataFileName;
    }

    /// <summary>
    /// Loads settings or game data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>Returns the type you specify; either game data or settings data.</returns>
    public T LoadData<T>()
    {
        //determines which file to load
        string fileName = typeof(T) switch
        {
            Type t when t == typeof(GameData) => gameDatafileName,
            Type t when t == typeof(SettingsData) => settingsDataFileName,
            _ => throw new InvalidOperationException($"Unsupported type: {typeof(T)}")
        };

        //Path combine ensures code works for different os's with different file
        //Seperators
        string fullPath = IO_Path.Combine(dataDirPath, fileName);
        string dataToLoad = "";

        if (File.Exists(fullPath))
        {
            try
            {
                using (FileStream fileStream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                       dataToLoad = reader.ReadToEnd();
                    }
                
                    fileStream.Close();
                }

                //deserialise data back into c# object and return it
                return JsonUtility.FromJson<T>(dataToLoad);
            }
             catch (Exception e)
            {
                Debug.LogError($"An error occured when loading the file at path: {fullPath} \n Error: {e}");
            }
        }

        //returns whatever the default value of the type you specified
        //in this case null
        return default(T);
    }


    /// <summary>
    /// Saves either save or settings data to a file using file name and path specified
    /// in this objects constructor     
    /// </summary>
    /// <typeparam name="T"> Either GameData or SettingsData type</typeparam>
    /// <param name="value"></param>
    public void SaveData<T>(T data)
    {
        //determines which file name to save with
        string fileName = data switch
        {
            GameData => gameDatafileName,
            SettingsData => settingsDataFileName,
            _ => throw new InvalidOperationException($"Unsupported type: {typeof(T)}")
        };
        //Path combine ensures code works for different os's with different file
        //Seperators
        string fullPath = IO_Path.Combine(dataDirPath, fileName);

        try
        {
            //Create directory and file if it doesn't already exist
            Directory.CreateDirectory(IO_Path.GetDirectoryName(fullPath));

            //Convert C# script data to JSOnm
            string dataToWrite = JsonUtility.ToJson(data, true);

            //Write Data To File
            using (FileStream fileStream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    writer.Write(dataToWrite);
                }
                fileStream.Close();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occured when saving the file at path: {fullPath} \n Error: {e}");
        }
    }

}
