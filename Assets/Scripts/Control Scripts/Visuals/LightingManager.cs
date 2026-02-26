using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;


public class LightingManager : MonoBehaviour
{

    #region References
    [Header("References")]

    public static LightingManager instance;

    private StreetLightManager streetLightManager;
    [SerializeField] SunManager sunManager;
    [SerializeField] LightingPreset LightingPreset;
    [SerializeField] Light DirectionalLight;
    [SerializeField] Material DayBox;
    [SerializeField] Material NightBox;


    #endregion

    #region Variables
    [Header("Variables")]

    [Header("Determines if time progresses or not")]
    [SerializeField] bool isTimeStatic;

    private float secondsSinceLastHour;

    [Range(0, 24)] private float TimeOfDay;
    [Header("Smaller this value the faster time goes")]
    [Range(1, 128)] public float TimeScaleFactor = 60;
    //Needed as this cant be a range
    //This value can be accessed by other scripts that need to check the time
    public float TimeOfDayRef { get; private set; }
    //Used to display time in hours and minutes
    [SerializeField] float Hours;
    [SerializeField] float Mins;
    //For sky box and general time keeping
    public enum DayStatus { Day, Night };
    public DayStatus dayStatus { get; private set; }

    //this value determines how quickly the two skyboxes blend
    [SerializeField] float transitionSpeed = 0.1f;
    #endregion

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }

        instance = this;
    }

    private void Start()
    {
        try
        {
            streetLightManager = GetComponent<StreetLightManager>();
        }
        catch (Exception e)
        {
            Debug.LogError("Couldn't find street light manager script! " +
            "Make sure it is on the same gameobject as this script!\n" + e);
        }

        SetTimeOfDay("start" ,12, false);
    }

    private void Update()
    {
        #region Calculate Time
        //put into own method
        if (Application.isPlaying && !isTimeStatic)
        {
            CalculateTime();
            CalculateDisplayTime();
            UpdateLighting(TimeOfDay / 24f);
        }
        else if (Application.isEditor && !isTimeStatic)
        {
            CalculateDisplayTime();
            UpdateLighting(TimeOfDay / 24f);
        }
        #endregion

        #region Blend Skyboxes
        float blendValue = Mathf.PingPong(Time.time * transitionSpeed, 1.0f);
        RenderSettings.skybox = BlendSkyboxes(DayBox, NightBox, blendValue);
        #endregion

    }


    private void CalculateTime()
    {
        // Store the previous time of day
        float prevTimeOfDay = TimeOfDay;

        //print(TimeOfDay);
        // Update the time of day
        TimeOfDay += Time.deltaTime / TimeScaleFactor;

        // Ensure TimeOfDay wraps around 24 hours
        TimeOfDay %= 24;

        // Update the counter for real-world seconds since the last in-game hour started
        secondsSinceLastHour += Time.deltaTime;
       

        //if the hour is not the same as last frame, reset the counter
        if (Mathf.Floor(prevTimeOfDay) != Mathf.Floor(TimeOfDay))
        {
            // New in-game hour has started
            secondsSinceLastHour = 0f; // Reset the counter
        }

        // Calculate and display the real-world duration of one in-game hour
        float realWorldSecondsPerGameHour = TimeScaleFactor;
        // Debug.Log($"Seconds since last hour: {secondsSinceLastHour}");
        //Debug.Log($"Real-world seconds per in-game hour: {realWorldSecondsPerGameHour}");

        sunManager.UpdateSunPosition(TimeOfDay, secondsSinceLastHour, realWorldSecondsPerGameHour);
    }

    
    private void UpdateLighting(float timePercent)
    {
        if (LightingPreset == null)
        {
            Debug.LogError("Lighting Preset is not set!!!");
            return;
        }

        sunManager.UpdateSunColour(timePercent, LightingPreset.SunColour);
        RenderSettings.ambientLight = LightingPreset.AmbientColour.Evaluate(timePercent);
        RenderSettings.fogColor = LightingPreset.FogColour.Evaluate(timePercent);

        if (DirectionalLight == null)
        {
            try
            {
                //This gets the sun from the render settings if its set
                if (RenderSettings.sun != null)
                {
                    DirectionalLight = RenderSettings.sun;
                }
                else
                {
                    //This code gets the first Directional Light found in the scene
                    Light[] lights = FindObjectsOfType<Light>();
                    foreach (Light light in lights)
                    {
                        if (light.type == LightType.Directional)
                        {
                            DirectionalLight = light;
                        }
                    }
                }

            }
            catch
            {
                Debug.LogError("Directional Light Couldn't be found");
                return;
            }

        }

        DirectionalLight.color = LightingPreset.DirectionalLightColour.Evaluate(timePercent);
        DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));

        #region Update Day Status
        if (TimeOfDay >= 6 && TimeOfDay <= 19)
        {
            //So it only executes once
            if (dayStatus == DayStatus.Night)
            {
                ToggleDayNight(DayStatus.Day);
            }
        }
        else
        {
            //So it only executes once
            if (dayStatus == DayStatus.Day)
            {
                ToggleDayNight(DayStatus.Night);
            }
        }
        #endregion
    }

    private void ToggleDayNight(DayStatus _dayStatus)
    {
        dayStatus = _dayStatus;
        if (dayStatus == DayStatus.Day)
        {
            UpdateSkyBox(true);

            if (streetLightManager == null) return;

            streetLightManager.EnableDisableStreetLights(false);
            SoundManager.instance.SwapTimeOfDayAmbienece(true);
        }
        else if (dayStatus == DayStatus.Night)
        {
            UpdateSkyBox(false);

            if (streetLightManager == null) return;

            streetLightManager.EnableDisableStreetLights(true);
            SoundManager.instance.SwapTimeOfDayAmbienece(false);
        }

    }

    #region Update Skybox

    Material BlendSkyboxes(Material daySkybox, Material nightSkybox, float blendValue)
    {
        Material blendedSkybox = new Material(daySkybox);
        blendedSkybox.Lerp(daySkybox, nightSkybox, blendValue);
        return blendedSkybox;
    }


    void UpdateSkyBox(bool isDay)
    {
        if (DayBox == null || NightBox == null)
        {
            Debug.LogError("No Skybox assigned!");
            return;
        }

        if (isDay)
        {
            RenderSettings.skybox = DayBox;
        }
        else
        {
            RenderSettings.skybox = NightBox;
        }
    }
    #endregion


    public void ModifyTimeScaleFactor(float valueToAdd)
    {
        if (TimeScaleFactor + valueToAdd < 1 || TimeScaleFactor + valueToAdd > 128) return;
        TimeScaleFactor += valueToAdd;

        try
        {
            CanvasController.instance.DisplayCurrentTimeScale(TimeScaleFactor);
        }
        catch (Exception e)
        {
            Debug.LogError("Couldn't display current time scale \n" + e);
        }
    }
    
    void CalculateDisplayTime() 
    {
        float minutes = TimeOfDay % 1f;

        
        Hours = Mathf.FloorToInt(TimeOfDay);
        Mins = (minutes * 60);

        CanvasController.instance.DisplayTime(Hours, Mins);
    }

    //Forces a certain time of day and prevents time from passing
    /// <summary>
    /// time of day is in a 24h format, freezetime is self explan.
    /// //method is used to identify what method and script called this
    /// </summary>
    /// <param name="forceTime"></param>
    /// <param name="timeofDay"></param>
    public void SetTimeOfDay(String callerMethod ,float timeofDay = 12, bool _freezeTime = true)
    {


        print("Method that set time of day: " + callerMethod);

        ToggleFreezeTime(_freezeTime);
        
        TimeOfDay = timeofDay;
        
        //skybox managment is done in this method so no need to change skybox
        UpdateLighting(TimeOfDay/24);
        sunManager.UpdateSunPosition(TimeOfDay, secondsSinceLastHour, TimeScaleFactor);
    }

    public void ToggleDirectionalLight(bool value) 
    {
        if(DirectionalLight != null) 
        { 
            DirectionalLight.gameObject.SetActive(value);
        }
    }


    public void ToggleFreezeTime(bool freezeTime)
    {
        if (freezeTime)
        {
            ModifyTimeScaleFactor(0);
            isTimeStatic = true;
        }
        else
        {
            ModifyTimeScaleFactor(60);
            isTimeStatic = false;
        } 
    }
}
