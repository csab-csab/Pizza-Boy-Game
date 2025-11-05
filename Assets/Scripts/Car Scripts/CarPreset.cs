using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Car Preset", menuName = "CarPresets" )]
public class CarPreset : ScriptableObject 
{
    [Header("Car Properties")]
    public float weight;
    public AnimationCurve totalEnginePower;
    [Header("Engine & Transmission")]
    public float[] Gears;
    public float finalDrive;
    public float enginePower;
    [Header("The higher this value, \n the less prevelant the engine braking.\nI found 300 to be a good starting point.")]
    public float engineBrakeFactor;
    public float switchUPRPM;
    public float switchDOWNRPM;
    public float gearChangeCoolDown;
    public float idleRPM;
    public float maxRPM;
    public float maxSpeed;
    public float maxReverseSpeed;
   [Header("Brakes & Steering")]
    public float maxBrakeForce;
    public float maxSteeringAngle;
    [Header("Drivetrain")]
        
    public typeOfDrive TypeofDrive;
    public enum typeOfDrive { frontWheelDrive, rearWheelDrive, allWheelDrive };
    public LayerMask GroundLayer;
    
    [Header("Fuel")]
    public AnimationCurve FuelDrainRate;
    public float FuelCapacity;
 
    [Header("Car Visuals")]
    public Material LightsMaterial;
    [Header("Wheel Collider Values")]
    //finish off for all values   
    public float asympForwardFriction;
    public float asympSideWaysFriction;
    public float extremForwardFriction;
    public float extremSideWaysFriction;
    public float suspensionStiffness;
    [Header("Car Sounds")]
    public AnimationCurve carRpmToPitch;
    public AudioClip startSound;
    public AudioClip runningSound;
    public AudioClip shutDownSound;
    public AnimationCurve tyreScreechVolume;
    public AudioClip tyreScreechSound;

      
}
