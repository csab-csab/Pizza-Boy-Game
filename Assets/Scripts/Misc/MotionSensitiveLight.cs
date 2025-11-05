using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSensitiveLight : MonoBehaviour
{
    private GameObject DetectionArea;
    private GameObject Light;
    [SerializeField] private LayerMask ignoreLayers;

    [SerializeField] float LightTimer = 3f;
    private float currentLightTime;
    
    void Start()
    {
        DetectionArea = this.transform.GetChild(0).gameObject;
        Light = this.transform.GetChild(1).gameObject;
    }

    private void Update()
    {
        if (currentLightTime > 0) 
        { 
            currentLightTime -= Time.deltaTime;
        }
        else if(Light.gameObject.activeSelf && currentLightTime <= 0) 
        { 
            Light.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider trigger)
    {
        if (CheckIgnoreLayer(trigger.gameObject.layer))
        {
            if (Light.TryGetComponent<Light>(out Light light))
            {
                    light.gameObject.SetActive(true);
                    currentLightTime = LightTimer;
            }
        }
    }

    private void OnTriggerStay(Collider trigger)
    {
        if (CheckIgnoreLayer(trigger.gameObject.layer))
        {
            if (trigger.gameObject.layer != ignoreLayers)
            {
                if (Light.TryGetComponent<Light>(out Light light))
                {
                    light.gameObject.SetActive(true);
                    currentLightTime = LightTimer;
                }
            }
        }
    }

    private bool CheckIgnoreLayer(int rawLayer) 
    { 
        return ((1 << rawLayer) & ignoreLayers) == 0 ;
    }
}
