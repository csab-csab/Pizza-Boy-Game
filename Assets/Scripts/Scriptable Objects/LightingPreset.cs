using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu (fileName = "LightingPreset", menuName = "LightingPrestets/LightingPreset", order = 1)]
public class LightingPreset : ScriptableObject
{
    public Gradient SunColour;
    public Gradient AmbientColour;
    public Gradient DirectionalLightColour;
    public Gradient FogColour;
    public AnimationCurve FogIntensity;
   }
