using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightManager : JunctionManagerBase
{

    [Header("Time it takes to switch")]
    [SerializeField] float SwitchTime = 15f;
    [Header("The amount of time all the lights are red")]
    [SerializeField] float AllRedTime = 3f;


    [Header("Supports only 4 lights")]
    [SerializeField] List<TrafficLightJunctionPoint> TrafficLightPoints = new List<TrafficLightJunctionPoint>(4);

    [SerializeField] Material goMat;
    [SerializeField] Material amberMat;
    [SerializeField] Material stopMat;
    
    void Start()
    {
        // Determine how many lights should be set to "Go"
        int goLights = Mathf.Min(2, TrafficLightPoints.Count);
        //This loop sets the first or first two lights to go and the other two to red
        for (int i = 0; i < TrafficLightPoints.Count; i++) 
        {
            TrafficLightJunctionPoint tfl = TrafficLightPoints[i];
            if (i < 1)
            {
                tfl.SetTrafficLightState(TrafficLightJunctionPoint.TrafficLightState.Stop);
            }
            else 
            {
                tfl.SetTrafficLightState(TrafficLightJunctionPoint.TrafficLightState.Go);
            }
        }

        InvokeRepeating("SwitchAllLightsToStop", 0.0f, SwitchTime - AllRedTime);
        InvokeRepeating("SwitchLigths", 0.0f, SwitchTime);

        // Subscribe to the event for each JunctionPoint
        foreach (var trafficLightJunctionPoint in TrafficLightPoints)
        {
           trafficLightJunctionPoint.OnChoosingPath += GetPath;
        }
    }

    //this method switches all the lights to stop a few seconds before its time to go
    //However it gets the next state ready by making it standby
    void SwitchAllLightsToStop() 
    {
        foreach (TrafficLightJunctionPoint tfl in TrafficLightPoints)
        {
            if (tfl.lightState == TrafficLightJunctionPoint.TrafficLightState.Go)
            {
                tfl.SetTrafficLightState(TrafficLightJunctionPoint.TrafficLightState.StandByStop);
                tfl.GetComponent<MeshRenderer>().material = amberMat;
            }
            else if(tfl.lightState == TrafficLightJunctionPoint.TrafficLightState.Stop)
            {
                tfl.SetTrafficLightState(TrafficLightJunctionPoint.TrafficLightState.StandByGo);
            }
        }
    }


    //this method actually switches the lights using the pre-determined states
    void SwitchLigths() 
    {
        foreach(TrafficLightJunctionPoint tfl in TrafficLightPoints) 
        {
            if(tfl.lightState == TrafficLightJunctionPoint.TrafficLightState.StandByGo) 
            {
                StartCoroutine(DelaySwitchToGreen(tfl));
            }
            else if(tfl.lightState == TrafficLightJunctionPoint.TrafficLightState.StandByStop)
            {
                tfl.SetTrafficLightState(TrafficLightJunctionPoint.TrafficLightState.Stop);
                tfl.GetComponent<MeshRenderer>().material = stopMat;
            }
        }
    }

    IEnumerator DelaySwitchToGreen(TrafficLightJunctionPoint tfl) 
    {
        yield return new WaitForSeconds(AllRedTime/2);
        
        tfl.GetComponent<MeshRenderer>().material = amberMat;
       
        yield return new WaitForSeconds(AllRedTime / 2);
        
        tfl.SetTrafficLightState(TrafficLightJunctionPoint.TrafficLightState.Go);
        tfl.GetComponent<MeshRenderer>().material = goMat;
    }

}
