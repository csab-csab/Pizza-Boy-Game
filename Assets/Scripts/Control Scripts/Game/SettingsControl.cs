using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

public class SettingsControl : MonoBehaviour, IDataPersistance
{

    #region Settings Values
    [Header("Settings Values")]
    private Resolution[] availableResolutions;
    private int GraphicsQualityIndex;
    private int PresentationOption;
    private float MasterVolume;

    private int TransmissionTypeIndex = 0;

    private bool isFullScreen = true;
    #endregion

    #region  UI
    [Header("UI")]
    [SerializeField] TMP_Dropdown resDropdown;
    [SerializeField] TMP_Dropdown qualityDropdown;
    [SerializeField] TMP_Dropdown presDropdown;
    [SerializeField] TMP_Dropdown transmissisonDropDown;
    [SerializeField] Slider masterAudioSlider;
    #endregion

    private void Start()
    {
        GetAvailableResolutions();
    }

    #region Video and Graphics
    public void SetResolution(int resolutionIndex)
    {
        Resolution selectedRes = availableResolutions[resolutionIndex];
        Screen.SetResolution(selectedRes.width, selectedRes.height, isFullScreen);
    }

    public void SetGraphicsQuality(int QualityIndex)
    {
        QualitySettings.SetQualityLevel(QualityIndex);
        GraphicsQualityIndex = QualityIndex;
    }

    public void SetPresentationMode(int PresentationModeIndex)
    {
        switch (PresentationModeIndex)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                isFullScreen = true;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                isFullScreen = true;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                isFullScreen = false;
                break;
        }

        PresentationOption = PresentationModeIndex;
    }
    #endregion

    #region Gameplay
    public void SwitchTransmission(int index)
    {
        //Subscribe this method to be called when scene is actually loaded
        bool inMainMenu = SceneManager.instance.ReturnCurrentSceneIndex() == 0;

        if (inMainMenu)
        {
            TransmissionTypeIndex = index;
            return;
        }

        switch (index)
        {
            case 0:
                try
                {
                    GameManager.instance.AccessCarController().SwitchTransmissionMode(CarController.typeOfTransmission.automatic);
                }
                catch (Exception e)
                {
                    TransmissionTypeIndex = index;
                }
                break;
            case 1:
                try
                {
                    GameManager.instance.AccessCarController().SwitchTransmissionMode(CarController.typeOfTransmission.manual);
                }
                catch (Exception e)
                {
                    TransmissionTypeIndex = index;
                }
                break;
        }
    }
    #endregion

    #region  Audio
    public void SetMasterVolume(float volume)
    {
        MasterVolume = volume;
        AudioListener.volume = volume;
    }

    #endregion

    /// <summary>
    /// Fills dropdown with resolutions supported by current displa
    /// </summary>
    private void GetAvailableResolutions()
    {
        availableResolutions = Screen.resolutions;
        if (availableResolutions != null)
        {
            FillResolutionDropdown();
        }
    }

    /// <summary>
    /// Loops through all available screen resolutions, formats them into stirngs, and adds them to the dropdown ui element
    /// </summary>
    private void FillResolutionDropdown()
    {
        resDropdown.ClearOptions();

        List<string> formattedStrings = new List<string>();

        int i = 0;
        int currentResIndex = 0;

        foreach (Resolution res in availableResolutions)
        {
            i++;

            float refreshRate = (float)res.refreshRateRatio.value;
            refreshRate = Mathf.Round(refreshRate);

            string formattedString = $"{res.width}x{res.height}@{refreshRate:F0}hz";
            formattedStrings.Add(formattedString);

            if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resDropdown.AddOptions(formattedStrings);
        resDropdown.value = currentResIndex;
        resDropdown.RefreshShownValue();
    }



    private void RefreshSettingsUi(int qualIndex, int presIndex, float masterVol, int TransmissionType)
    {
        qualityDropdown.value = qualIndex;
        presDropdown.value = presIndex;
        masterAudioSlider.value = masterVol;
        transmissisonDropDown.value = TransmissionType;
        qualityDropdown.RefreshShownValue();
        presDropdown.RefreshShownValue();
        transmissisonDropDown.RefreshShownValue();
    }

    #region  Saving and Loading Settings
    public void SaveGameData(ref GameData gameData)
    {
        //not needed for this script
    }
    public void LoadGameData(GameData gameData)
    {
        //not needed for this script
    }
    public void SaveSettingsData(ref SettingsData settingsData)
    {
        settingsData.GraphicsQualityIndex = this.GraphicsQualityIndex;
        settingsData.PresentationOption = this.PresentationOption;
        settingsData.MasterVolume = this.MasterVolume;
        settingsData.TransmissionTypeIndex = this.TransmissionTypeIndex;
    }
    public void LoadSettingsData(SettingsData settingsData)
    {
        print("Loading settings data!");
        this.GraphicsQualityIndex = settingsData.GraphicsQualityIndex;
        SetGraphicsQuality(this.GraphicsQualityIndex);

        this.PresentationOption = settingsData.PresentationOption;
        SetPresentationMode(this.PresentationOption);

        this.MasterVolume = settingsData.MasterVolume;
        SetMasterVolume(this.MasterVolume);

        this.TransmissionTypeIndex = settingsData.TransmissionTypeIndex;

        RefreshSettingsUi(GraphicsQualityIndex, PresentationOption, MasterVolume, TransmissionTypeIndex);

        print("Loaded settings in script");
    }
    #endregion
}

