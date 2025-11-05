using System;
using System.Collections.Generic;
using UnityEngine;

public class StreetLightManager : MonoBehaviour
{
    #region References
    private LightingManager lightingManager;
    #endregion

    const string STREETLIGHT_TAG = "StreetLight";
    
    #region Variables
    [Header("Variables")]
   
    private Light[] lights;
    [SerializeField]private List<Light> streetLights = new List<Light>();
    [SerializeField]private List<Light> halos = new List<Light>();

    [SerializeField] float HaloIntensity;
    [SerializeField] float LightIntensity;

    #endregion

    private void Start()
    {
        try
        { //Why the reference no uses here? || Pending for removal
            lightingManager = GetComponent<LightingManager>();
        }
        catch (NullReferenceException)
        {
            Debug.LogError("Lighting Manager Cannot Be Found On the Game Manager Object!!!");
        }

       lights = FindObjectsOfType<Light>();

        //Loop through everylight in the scene and find the streetlights
        foreach (Light light in lights) 
        {
            if (light != null && light.CompareTag(STREETLIGHT_TAG) && light.name.StartsWith("L"))
            { 
                streetLights.Add(light);
            }
            else if (light.name.StartsWith("H"))
            { 
                halos.Add(light);
            }
        }
    }

    public void EnableDisableStreetLights(bool enabled)
    {
        float lightIntensity = enabled ? LightIntensity : 0f;
        float haloIntensity = enabled ? HaloIntensity : 0f;

        for (int i = 0; i < streetLights.Count - 1; i++)
        {
            //To avoid errors if the two lists aren't the exact same length
            try
            {
                streetLights[i].intensity = lightIntensity;
                halos[i].intensity = haloIntensity;
            }
            catch
            {
                if (streetLights[i] == null)
                {
                    streetLights.RemoveAt(i);
                }
                else //Remove the halo
                {
                    halos.RemoveAt(i);
                }
            }
        }
    }
}
