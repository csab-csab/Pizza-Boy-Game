using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SettingsControl:MonoBehaviour
{

    #region Settings Values
    [Header("Settings Values")]
    private Resolution[] availableResolutions;
    private int GraphicsQualityIndex;
    private int PresentationOption;
    private float MasterVolume;

    private bool isFullScreen = true;
    #endregion

    #region  UI
    [Header("UI")]
    [SerializeField] TMP_Dropdown resDropdown;
    #endregion

    private void Start()
    {
        GetAvailableResolutions();
    }

    #region Ui Interaction
    public void SetResolution(int resolutionIndex)
    {
        Resolution selectedRes = availableResolutions[resolutionIndex];
        Screen.SetResolution(selectedRes.width, selectedRes.height, isFullScreen);
    }

    public void SetGraphicsQuality(int QualityIndex)
    {
        QualitySettings.SetQualityLevel(QualityIndex);
    }

    public void SetPresentationMode(int PresentationModeIndex)
    {
        switch(PresentationModeIndex)
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
    }

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        print($"Volume: {volume}");
    }
    #endregion

    private void GetAvailableResolutions()
    {
        availableResolutions = Screen.resolutions;
        if(availableResolutions != null)
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

        foreach(Resolution res in availableResolutions)
        {
            i++;
            
            float refreshRate = (float) res.refreshRateRatio.value;
            refreshRate = Mathf.Round(refreshRate);
            
            string formattedString = $"{res.width}x{res.height}@{refreshRate:F0}hz";
            formattedStrings.Add(formattedString);

            if(res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resDropdown.AddOptions(formattedStrings);
        resDropdown.value = currentResIndex;
        resDropdown.RefreshShownValue();
    }
}
