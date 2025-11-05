using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Accessibility;

public class TrafficLightJunctionPoint : JunctionPoint
{

    public enum TrafficLightState { Stop, StandByStop,StandByGo, Go };
    public TrafficLightState lightState{ get; private set; }

    public void SetTrafficLightState(TrafficLightState state)
    {
        lightState = state;
        this.state = (lightState == TrafficLightState.Go) ? JunctionState.Go : JunctionState.Stop;
    }
}
