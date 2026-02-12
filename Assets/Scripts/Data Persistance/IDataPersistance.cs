using UnityEngine;

public interface IDataPersistance
{
  void SaveGameData(ref GameData gameData);
  void LoadGameData(GameData gameData);
  void SaveSettingsData(ref SettingsData settingsData);
  void LoadSettingsData(SettingsData settingsData);
}
