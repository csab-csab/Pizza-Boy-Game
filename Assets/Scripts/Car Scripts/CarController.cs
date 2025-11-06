using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;


public class CarController : MonoBehaviour
{
    #region Input Variables


    float InputHorizontal;
    [SerializeField] float InputVertical;

    [SerializeField] float LastInputVertical;

    private const string Horizontal = "Horizontal";
    private const string Vertical = "Vertical";

    //nessecary to flip car over
    private LayerMask GroundLayer;
    #endregion
    #region Car Properties
    [Header("Rigid Body, Collider Parent and Transform Parent")]
    //this is a reference to the player associated with this car
    private PlayerManager playerManager;
    
    public Rigidbody rb;
    public Transform ColliderParent;
    public Transform TransformParent;
    public Transform ParticleEffectsParent;
    //Assign this manually in inspector
    public Transform BodyParent;

    [Header("Destruction Variables")]
   


    [Header("Wheel Colliders")]
    [SerializeField] WheelCollider[] wheelColliders;


    [Space(10)]

    [Header("Wheel Transforms")]
    List<Transform> wheelTransforms = new List<Transform>();



    [Space(10)]

    [Header("Car Properties")]
    
    public bool isEnabled = true;
    Vector3 StartPosition;
    [SerializeField] CarPreset preset;
    [SerializeField] float Weight;
    [SerializeField] float DownForceValue = 50f;
    [Header("Engine")]
    [SerializeField] AnimationCurve totalEnginePower;
    [SerializeField] float enginePower;
    [SerializeField] float engineRpm;
    [Header("The higher this value, the less prevelant the engine braking. \n Use trial and error. I found 300 to be a good starting point.")]
    [SerializeField]float engineBrakeFactor = 300;
    [SerializeField]bool isEngineBraking = false;
    float idleRPM;
    float maxRPM;
    //used for audio
    bool wasEngineOn = false;
  
    [Header("Gearbox")]
    [SerializeField] float[] Gears;
    [SerializeField] int currentGear;
    [SerializeField] float switchUPRPM;
    [SerializeField] float switchDOWNRPM;
    [SerializeField] float lastRPM;
    [SerializeField] float gearChangeCoolDown;
    private float baseGearChangeCoolDown;
    [SerializeField] float FinalDrive;
    [Header("Speed")]
    [SerializeField] float actualSpeed;
    [SerializeField] float displaySpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float maxReverseSpeed;

    [Header("Fuel")]
    //this variable is used to disable the refuel area when delivering or fuel for example
    [SerializeField]bool isAllowedToRefuel = true;
    [SerializeField]AnimationCurve FuelDrainRate;
    public float FuelCapacity;
     public float currentFuel;

    [Header("Current Status")]
    bool isEngineOn;
    private bool isDrifting;
    [SerializeField]private float currentDriftValue;


    public bool isBraking { get; private set; }
    public bool isHandBraking { get; private set; }
    [SerializeField] bool isReversing;
    private float currentBrakeForce;
    private float maxBrakeForce;
    private float frontBias = 0.7f;
    private float rearBias = 0.3f;

    [SerializeField] float WheelsRPM;

    float currentSteeringAngle;
    [SerializeField] float maxSteeringAngle;

    [SerializeField] enum typeOfDrive { frontWheelDrive, rearWheelDrive, allWheelDrive };
    [SerializeField] typeOfDrive TypeOfDrive;

    [SerializeField] enum typeOfTransmission { manual, automatic };
    [SerializeField] typeOfTransmission TypeOfTransmission;

    [Header("Paricle Effects")]
    [SerializeField] List<TrailRenderer> TyreSkidMarks;
    [SerializeField] List<ParticleSystem> TyreSmoke;
    
    [SerializeField] ParticleSystem ExhaustSmoke;
    [SerializeField] ParticleSystem ExhaustSmoke1;

    [SerializeField] ParticleSystem ExhaustFlame;
    [SerializeField] ParticleSystem ExhaustFlame1;

    //remove serialize 
    //Parent object that holds the fire effects etc.
    [SerializeField] Transform DamageEffectsHolder;
    [SerializeField] List<ParticleSystem> DamageEffects;

    //this variable is used to check if the current rt value is more than last frames
    float lastFrameThrottleValue = 0;
    bool isFlameReady;

    [SerializeField] float SpeedLineTriggerSpeed = 100;
    bool speedLineReady;

    [Header("Lights")]
    //THESE NEED TO BE SERIALIZEFIELD OTHERWISE IT DOESNT WORK IDK WHY 
    [SerializeField]Transform LightTransformParent;
     [SerializeField]List<Light> Lights;
     [SerializeField]Material LightMaterial;
    private bool lightsEnabled = false;

    [Header("Sounds")]
    AudioSource carAudioSource;
    AudioSource tyreAudioSource;
    #endregion
    #region Events
    public delegate void OutOfFuel();
    public delegate void LowOnFuel();
    public delegate void HighOnFuel();

    public OutOfFuel OutOfFuelEvent;
    public static LowOnFuel LowOnFuelEvent;
    public static HighOnFuel HighOnFuelEvent;
    #endregion

    #region Refs
    [SerializeField] Transform CameraLookAt;
    #endregion

    private void Start()
    {
        IntialiseCar();
    }

    private void Update()
    {
   
        if (GameManager.instance.gameState == GameManager.GameState.Playing && 
            isEnabled)
        {
            GetInput();
        }

        #region  Reset Car
        //Reset the car position if its below the map
        if (transform.position.y <= -10)
        {
            ResetCarPostion();  
        }

        //Check if car is upside down 
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), transform.up, 5f, GroundLayer))
        {
          ResetCarRotation();
        }

        #endregion

        if (gearChangeCoolDown > 0)
        {
            gearChangeCoolDown -= Time.deltaTime;
        }

        GroundCheck();
        
        CheckSpeedLines();
    }

    private void FixedUpdate()
    {
        if (isEnabled)
        {
            if (isEngineOn)
            {
                CalculateEnginePower();
                HandleSteering();
                UpdateWheelMeshes();
            }
        }



        AddDownForce();

        CheckDrift();

        actualSpeed = rb.linearVelocity.magnitude * 4f;

        displaySpeed = actualSpeed <= 1 ? displaySpeed = 0 : displaySpeed = actualSpeed;
        
        CanvasController.instance.UpdateRpmNeedle(engineRpm);
        CanvasController.instance.UpdateSpeed(displaySpeed);
        CanvasController.instance.UpdateFuelNeedle(currentFuel, FuelCapacity);
    }

    private void GetInput()
    {
        
       
        InputHorizontal = Input.GetAxisRaw(Horizontal);
        InputVertical = Input.GetAxisRaw(Vertical);

        currentSteeringAngle = maxSteeringAngle * InputHorizontal;

        if (Input.GetKeyDown("p")) 
        {
           
            ToggleEngine(!isEngineOn);
        }

        #region Braking 
        isBraking = Input.GetKey(KeyCode.Space) || Input.GetAxisRaw("LT") > 0 && !isReversing;
        isHandBraking = Input.GetButton("Hand Brake");

        if (isBraking)
        {
            switch (GameManager.instance.inputDevice) {

                case GameManager.InputDevice.KeyboardAndMouse:
                    currentBrakeForce = maxBrakeForce;
                    HandleBraking();
                    break;

                case GameManager.InputDevice.Controller:
                    currentBrakeForce = Input.GetAxisRaw("LT") * maxBrakeForce;
                    HandleBraking();
                    break;
            }
        }
        else if (!isBraking && !isEngineBraking)
        {
            currentBrakeForce = 0;
            HandleBraking();
        }

        HandleHandBrake(isHandBraking);

        #endregion

        #region Change Gear
        if (TypeOfTransmission == typeOfTransmission.manual)
        {
            if (Input.GetButtonDown("Gear Up") && currentGear < Gears.Length - 1)
            {
                currentGear++;
                CanvasController.instance.UpdateGears(currentGear);
            }
            else if (Input.GetButtonDown("Gear Down") && currentGear > 0)
            {

                int potGear = currentGear - 1;
                float velocity = 0.0f;

                if (Mathf.SmoothDamp(engineRpm, 1000 + (Mathf.Abs(WheelsRPM) * 3.6f * (Gears[potGear])), ref velocity, 0.001f) >= maxRPM)
                {
                    print("Rpm Too High");
                    return;
                }

                currentGear--;

                CanvasController.instance.UpdateGears(currentGear);
            }
        }
        else if (TypeOfTransmission == typeOfTransmission.automatic)
        {
            if (!isReversing && displaySpeed <= 25 && currentGear > 1)
            {
                ChangeGear(1);
            }

            if (currentGear == 0 && InputVertical > 0 ^ Input.GetAxis("RT") > 0)
            {
                ChangeGear(1);

                gearChangeCoolDown = baseGearChangeCoolDown;
            }
            //switch up 
            if (!isReversing && gearChangeCoolDown <= 0 && currentGear < (Gears.Length - 1) && engineRpm >= switchUPRPM && engineRpm > lastRPM)
            {
                ChangeGear(currentGear + 1);
                gearChangeCoolDown = baseGearChangeCoolDown;

            }
            //switch down
            else if (!isReversing && gearChangeCoolDown <= 0 && currentGear > 1 && engineRpm <= switchDOWNRPM && engineRpm < lastRPM)
            {
                ChangeGear(currentGear - 1);
                gearChangeCoolDown = baseGearChangeCoolDown;
            }
        }
        #endregion

        #region Handle Reversing
        if (GameManager.instance.inputDevice == GameManager.InputDevice.KeyboardAndMouse)
        {
            if (InputVertical < 0 && !isReversing && rb.linearVelocity.magnitude <= 1f  || InputVertical < 0 && isReversing && displaySpeed < maxReverseSpeed)
            {
                SetIsReversing(true);

                currentGear = 1;

                CanvasController.instance.UpdateGears(-1);
            }
            else if (InputVertical > 0 && isReversing)
            {

                SetIsReversing(false);

                CanvasController.instance.UpdateGears(currentGear);
            }
        }
        else if (GameManager.instance.inputDevice == GameManager.InputDevice.Controller)
        {
            //if not reversing or if already reversing
            if (!isReversing && rb.linearVelocity.magnitude <= 0.01f && Input.GetAxisRaw("LT") > 0f || isReversing && Input.GetAxisRaw("LT") > 0f && displaySpeed < maxReverseSpeed)
            {
                SetIsReversing(true);

                currentGear = 1;

                CanvasController.instance.UpdateGears(-1);
            }
            else if (Input.GetAxisRaw("LT") == 0f)
            {
                SetIsReversing(false);

                CanvasController.instance.UpdateGears(currentGear);
            }
        }
        #endregion

        #region Particle Effects
        if (GameManager.instance.inputDevice == GameManager.InputDevice.KeyboardAndMouse && InputVertical < lastFrameThrottleValue
            || GameManager.instance.inputDevice == GameManager.InputDevice.Controller && Input.GetAxis("RT") < lastFrameThrottleValue)
        {
            isFlameReady = true;
        }
        
        if (ParticleEffectsParent != null && isEngineOn && isFlameReady)
        {
            ParticleEffectsControl.instance.PlayParticleEffect(ExhaustFlame);
            ParticleEffectsControl.instance.PlayParticleEffect(ExhaustFlame1);
            isFlameReady = false;
        }

        if (isDrifting && RoadCheck() || currentBrakeForce == maxBrakeForce && displaySpeed > 10 || isHandBraking && displaySpeed > 10) 
        {
            if (isHandBraking) 
            {
                ParticleEffectsControl.instance.RenderTyreMarks(true, true);
            }
            else 
            {
                ParticleEffectsControl.instance.RenderTyreMarks(true);
            }
            
            ParticleEffectsControl.instance.PlayTyreSmoke(true);
            SoundManager.instance.PlayAudioSource(tyreAudioSource);
          
        }
        else 
        {
            ParticleEffectsControl.instance.RenderTyreMarks(false);
            ParticleEffectsControl.instance.PlayTyreSmoke(false);
        }


        if (currentBrakeForce == maxBrakeForce && displaySpeed > 10 && RoadCheck())
        {
            PlayTyreScreechOnBrake(true);
        }
        else if (!isDrifting && displaySpeed < 10)
        {
            PlayTyreScreechOnBrake(false);
        }

        #endregion

        #region Lights

        if (Input.GetAxis("Joystick Dpad Y") > 0f && GameManager.instance.ReturnJoystickYStatus() || Input.GetKeyDown("h"))
        {
            ToggleLights();
        }

        ToggleBrakeLights();

        #endregion

        #region Values to be used next frame
        if(GameManager.instance.inputDevice == GameManager.InputDevice.KeyboardAndMouse) 
        {
            lastFrameThrottleValue = InputVertical;
        }
        else if (GameManager.instance.inputDevice == GameManager.InputDevice.Controller)
        {
            lastFrameThrottleValue = Input.GetAxis("RT");
        }
        #endregion
    }

    #region Calculate Values
    private void CalculateEnginePower()
    {
        if (currentFuel > 0 || !isEngineOn)
        {
            GetWheelRPM();

            if (GameManager.instance.inputDevice == GameManager.InputDevice.KeyboardAndMouse)
            {
                enginePower = totalEnginePower.Evaluate(engineRpm) * (Gears[currentGear]) * InputVertical * FinalDrive;
            }
            else if (GameManager.instance.inputDevice == GameManager.InputDevice.Controller)
            {
                if (!isReversing)
                {
                    enginePower = totalEnginePower.Evaluate(engineRpm) * (Gears[currentGear]) * Input.GetAxisRaw("RT") * FinalDrive;
                }
                else if (isReversing)
                {
                    enginePower = totalEnginePower.Evaluate(engineRpm) * (Gears[currentGear]) * -Input.GetAxisRaw("LT") * FinalDrive;
                }
            }
           
            lastRPM = engineRpm;

            float velocity = 0.0f;

            // Apply rev limiter
            float targetRpm = 1000 + (Mathf.Abs(WheelsRPM) * 3.6f * (Gears[currentGear]));
            engineRpm = Mathf.Clamp(Mathf.SmoothDamp(engineRpm, targetRpm, ref velocity, 0.001f), idleRPM, maxRPM);

            if (engineRpm >= maxRPM - 500)
            {
                // Bounce off the limit
                float overshoot = engineRpm - maxRPM;
                engineRpm = maxRPM - overshoot;
            }

            TakeFuel(FuelDrainRate.Evaluate(engineRpm)/1000 * Time.deltaTime); 
        }
        
        if(currentFuel <= 0)
        {
            ToggleEngine(false);
        }


        HandleEngine();

        CanvasController.instance.DisplayCurrentEngineStatistics(engineRpm, maxRPM, WheelsRPM, enginePower);
        SoundManager.instance.ModifyPitch(carAudioSource, preset.carRpmToPitch.Evaluate(engineRpm));
    }


    private void GetWheelRPM()
    {
        float sum = 0;
        //Number of wheels processed 
        int R = 0;

        for (int i = 0; i < 4; i++)
        {
            sum += wheelColliders[i].rpm;
            R++;
        }

        //if r!= 0 then return sum/R else return 0
        WheelsRPM = (R != 0) ? sum / R : 1;

    }

    private float WorkOutWhereObjectIs(Transform otherTransform) 
    {
        // Calculate vector from Object A to Object B
        Vector3 vectorAB = transform.position - otherTransform.position;

        // Choose a reference vector (e.g., right vector)
        Vector3 referenceVector = Vector3.right;

        // Normalize vectors
        vectorAB.Normalize();
        referenceVector.Normalize();

        // Calculate dot product
        float dotProduct = Vector3.Dot(vectorAB, referenceVector);
        
        return dotProduct;
    }
    #endregion

    #region Handle Vehicle Behaviours
   
    private void ToggleEngine(bool on) 
    {
        isEngineOn = on;
        if (isEngineOn)
        {
            ParticleEffectsControl.instance.PlayParticleEffect(ExhaustSmoke);
            ParticleEffectsControl.instance.PlayParticleEffect(ExhaustSmoke1);

            SoundManager.instance.SetAudioClip(carAudioSource, preset.startSound, false);
            StartCoroutine(SoundManager.instance.PlayCarStartSound(carAudioSource, preset.runningSound));
            wasEngineOn = true;

        }
        else 
        { 
            enginePower = 0;

            lastRPM = engineRpm;

            engineRpm = 0;

            ParticleEffectsControl.instance.StopParticleEffect(ExhaustSmoke);
            ParticleEffectsControl.instance.StopParticleEffect(ExhaustSmoke1);
            if (wasEngineOn == true)
            {
                SoundManager.instance.SetAudioClip(carAudioSource, preset.shutDownSound, false);
                StartCoroutine(SoundManager.instance.PlayCarStopSound(carAudioSource, preset.runningSound));
                wasEngineOn = false;
            }
        }
    }
    
    private void HandleEngine()
    {
        switch (TypeOfDrive)
        {
            case typeOfDrive.frontWheelDrive:

                //Dividing by two to split the power between the driven wheels
                wheelColliders[0].motorTorque = enginePower / 2;
                wheelColliders[1].motorTorque = enginePower / 2;

                //Engine Braking
                if (Input.GetAxisRaw("RT") == 0 && InputVertical == 0 && !isBraking && displaySpeed > 5)
                {
                    isEngineBraking = true;

                    wheelColliders[0].brakeTorque = maxBrakeForce/engineBrakeFactor;
                    wheelColliders[1].brakeTorque = maxBrakeForce/engineBrakeFactor;
                    wheelColliders[2].brakeTorque = maxBrakeForce/engineBrakeFactor;
                    wheelColliders[3].brakeTorque = maxBrakeForce/engineBrakeFactor;
                }
                else
                {
                    isEngineBraking = false;
                }

                break;

            case typeOfDrive.rearWheelDrive:

                //Dividing by two to split the power between the driven wheels
                wheelColliders[2].motorTorque = enginePower / 2;
                wheelColliders[3].motorTorque = enginePower / 2;

                //Engine Braking
                if (Input.GetAxisRaw("RT") == 0 && InputVertical == 0 && !isBraking && displaySpeed > 5)
                {
                    isEngineBraking = true;
                    
                    wheelColliders[2].brakeTorque = maxBrakeForce / engineBrakeFactor;
                    wheelColliders[3].brakeTorque = maxBrakeForce/engineBrakeFactor;
                 
                }
                else
                {
                    isEngineBraking = false;
                }
                break;

            case typeOfDrive.allWheelDrive:

                //Dividing by four to split the power between all of the driven wheels
                wheelColliders[0].motorTorque = enginePower / 4;
                wheelColliders[1].motorTorque = enginePower / 4;
                wheelColliders[2].motorTorque = enginePower / 4;
                wheelColliders[3].motorTorque = enginePower / 4;

                //Engine Braking
                if (Input.GetAxisRaw("RT") == 0 && InputVertical == 0 && !isBraking && displaySpeed > 5)
                {
                    wheelColliders[0].brakeTorque = enginePower / 10;
                    wheelColliders[1].brakeTorque = enginePower / 10;
                    wheelColliders[2].brakeTorque = enginePower / 10;
                    wheelColliders[3].brakeTorque = enginePower / 10;
                }

                break;
        }

    }

    private void HandleSteering()
    {
        wheelColliders[0].steerAngle = currentSteeringAngle;
        wheelColliders[1].steerAngle = currentSteeringAngle;
    }

    private void HandleHandBrake(bool enabled)
    {
       
            float velocity = rb.linearVelocity.z;
            for (int i = 2; i < wheelColliders.Length; i++)
            {
                //brake effect
                wheelColliders[i].brakeTorque = enabled ? Mathf.Infinity : 0;
            }
    }
    
    private void HandleBraking()
    {
        float torque = currentBrakeForce;

        for(int i = 0; i < wheelColliders.Length - 1; i++) 
        {
            WheelCollider wheel = wheelColliders[i];
            wheel.brakeTorque = i <= 1 ? torque * frontBias : torque * rearBias; 
        }
    }

    //Completely Stops the car
    private void ToggleLockAllBrakes(bool locked) 
    {
        float torque = locked ? Mathf.Infinity : 0;

        foreach (var wheel in wheelColliders) 
        {
            wheel.brakeTorque = currentBrakeForce;
        }
    }

    private void ChangeGear(int gear)
    {
        currentGear = gear;
       
        if (ParticleEffectsParent != null && isEngineOn)
        {
            ParticleEffectsControl.instance.PlayParticleEffect(ExhaustFlame);
            ParticleEffectsControl.instance.PlayParticleEffect(ExhaustFlame1);
        }

        CanvasController.instance.UpdateGears(currentGear);
    }

    private void AddDownForce()
    {
        rb.AddForce(-transform.up * DownForceValue * rb.linearVelocity.magnitude);
    }
    #endregion

    #region Visuals
    private void UpdateWheelMeshes()
    {
        UpdateWheelMesh(wheelColliders[0], wheelTransforms[0]);
        UpdateWheelMesh(wheelColliders[1], wheelTransforms[1]);
        UpdateWheelMesh(wheelColliders[2], wheelTransforms[2]);
        UpdateWheelMesh(wheelColliders[3], wheelTransforms[3]);
    }

    private void UpdateWheelMesh(WheelCollider wheelCollider, Transform wheelMesh)
    {
        Vector3 pos;
        Quaternion rot;

        wheelCollider.GetWorldPose(out pos, out rot);
        wheelMesh.position = pos;
        wheelMesh.rotation = rot;
    }
    
    //checks speed and if speed lines should be played
    private void CheckSpeedLines() 
    {
        if (displaySpeed >= SpeedLineTriggerSpeed && speedLineReady)
        {
            speedLineReady = false;
            ParticleEffectsControl.instance.PlaySpeedLineFX(true);
        }
        else if (displaySpeed < SpeedLineTriggerSpeed && !speedLineReady)
        {
            ParticleEffectsControl.instance.PlaySpeedLineFX(false);
            speedLineReady = true;
        }
    }
    #endregion

    #region Assign Variables
    private void IntialiseCar() 
    {
        rb = GetComponent<Rigidbody>();
        wheelColliders = new WheelCollider[4];

        #region Assign Preset Values
        Gears = preset.Gears;
        totalEnginePower = preset.totalEnginePower;
        engineBrakeFactor = preset.engineBrakeFactor;
        idleRPM = preset.idleRPM;
        maxRPM = preset.maxRPM;
        switchUPRPM = preset.switchUPRPM;
        switchDOWNRPM = preset.switchDOWNRPM;
        maxSpeed = preset.maxSpeed;
        maxReverseSpeed = preset.maxReverseSpeed;
        maxBrakeForce = preset.maxBrakeForce;
        maxSteeringAngle = preset.maxSteeringAngle;
        Weight = preset.weight;
        baseGearChangeCoolDown = preset.gearChangeCoolDown;
        FinalDrive = preset.finalDrive;
        TypeOfDrive = (typeOfDrive)preset.TypeofDrive;

        FuelDrainRate = preset.FuelDrainRate;
        FuelCapacity =  preset.FuelCapacity;

       

        #endregion

        GroundLayer = preset.GroundLayer;

        LightMaterial = preset.LightsMaterial;

        rb.centerOfMass = new Vector3(0, 0, 0);

        gameObject.tag = GameManager.instance.PLAYER_CAR_TAG;
        
        //this is to ensure collisions work with objective points
        //ensure body parent child 0 is the active (not destroyed) body of the car
        BodyParent.GetChild(0).gameObject.tag = GameManager.instance.PLAYER_CAR_TAG;

        GetWheelColliders();
        GetWheelTransforms();
        GetParticleEffects();
        GetLights();
        GenerateAudioSource();
        //TuneCar();

        StartPosition = transform.position;

        rb.mass = Weight;
        CanvasController.instance.UpdateGears(currentGear);

        isFlameReady = true;

        isEnabled = true;

        ToggleEngine(false);

        
    }
    
    private void GetWheelColliders()
    {
        wheelColliders[0] = ColliderParent.GetChild(0).GetComponent<WheelCollider>();
        wheelColliders[1] = ColliderParent.GetChild(1).GetComponent<WheelCollider>();
        wheelColliders[2] = ColliderParent.GetChild(2).GetComponent<WheelCollider>();
        wheelColliders[3] = ColliderParent.GetChild(3).GetComponent<WheelCollider>();
    }

    private void GetWheelTransforms()
    {
        wheelTransforms.Clear();
        wheelTransforms.Add(TransformParent.GetChild(0).transform);
        wheelTransforms.Add(TransformParent.GetChild(1).transform);
        wheelTransforms.Add(TransformParent.GetChild(2).transform);
        wheelTransforms.Add(TransformParent.GetChild(3).transform);
    }

    private void GetParticleEffects()
    {
        if (ParticleEffectsParent == null) return;

     
        //Make sure the objects on the prefab are in the right order
        ExhaustSmoke = ParticleEffectsParent.GetChild(0).GetComponent<ParticleSystem>();
        ExhaustSmoke1 = ParticleEffectsParent.GetChild(1).GetComponent<ParticleSystem>();

        ExhaustFlame = ParticleEffectsParent.GetChild(2).GetComponent<ParticleSystem>();
        ExhaustFlame1 = ParticleEffectsParent.GetChild(3).GetComponent<ParticleSystem>();

        TyreSkidMarks.Add(ParticleEffectsParent.GetChild(4).GetComponent<TrailRenderer>()); 
        TyreSkidMarks.Add(ParticleEffectsParent.GetChild(5).GetComponent<TrailRenderer>());
        TyreSkidMarks.Add(ParticleEffectsParent.GetChild(6).GetComponent<TrailRenderer>());
        TyreSkidMarks.Add(ParticleEffectsParent.GetChild(7).GetComponent<TrailRenderer>());
        
        TyreSmoke.Add(ParticleEffectsParent.GetChild(8).GetComponent<ParticleSystem>());
        TyreSmoke.Add(ParticleEffectsParent.GetChild(9).GetComponent<ParticleSystem>());

        DamageEffectsHolder = ParticleEffectsParent.GetChild(10);

        if(DamageEffectsHolder != null) 
        {
            for (int i = 0; i < DamageEffectsHolder.childCount; i++) 
            {
               DamageEffects.Add(DamageEffectsHolder.GetChild(i).GetComponent<ParticleSystem>());
            }
        }


        ParticleEffectsControl.instance.GetTyreMarkRenders(TyreSkidMarks);
        ParticleEffectsControl.instance.GetTyreSmokes(TyreSmoke);
        ParticleEffectsControl.instance.GetDamageParticles(DamageEffects);
        ParticleEffectsControl.instance.GetCarBodies(BodyParent.GetChild(0).GetComponent<MeshRenderer>(), BodyParent.GetChild(1).gameObject);
    }

    private void GenerateAudioSource()
    {
        carAudioSource = this.gameObject.AddComponent<AudioSource>();
        carAudioSource.playOnAwake= false;
        tyreAudioSource = this.gameObject.AddComponent<AudioSource>();
        
        SoundManager.instance.SetAudioClip(tyreAudioSource, preset.tyreScreechSound, true);

        SoundManager.instance.GetCarAudioSources(carAudioSource, true);
        SoundManager.instance.GetCarAudioSources(tyreAudioSource);
    }


    private void GetLights()
    {
        LightTransformParent = this.transform.Find("Headlights").transform;

        if (LightTransformParent == null) return;
        for (int i = 0; i < 4; i++)
        {
            Lights.Add(LightTransformParent.GetChild(i).GetComponent<Light>());
        }
    }

    private void TuneCar() 
    {
        if (wheelColliders.Length > 0)
        {
            foreach (WheelCollider wheelCollider in wheelColliders)
            {
                JointSpring spring = wheelCollider.suspensionSpring;
                spring.spring = 30000f;
                spring.damper = 4000f;
                spring.targetPosition = 0.5f;
                wheelCollider.suspensionSpring = spring;

                wheelCollider.suspensionDistance = 0.3f;

                wheelCollider.wheelDampingRate = 1f;

                WheelFrictionCurve forwardFriction = wheelCollider.forwardFriction;
                forwardFriction.stiffness = 2.0f;
                wheelCollider.forwardFriction = forwardFriction;

                WheelFrictionCurve sidewaysFriction = wheelCollider.sidewaysFriction;
                sidewaysFriction.stiffness = 3f;
                wheelCollider.sidewaysFriction = sidewaysFriction;
            }
        }

        
    }
    #endregion

    #region Modify or Check Status
   
    public void CheckDrift() 
    {
        Vector3 driftValue = transform.InverseTransformVector(rb.linearVelocity); 
        float driftAngle = (Mathf.Atan2(driftValue.x, driftValue.z) * Mathf.Rad2Deg);

        currentDriftValue = driftValue.x;

        //Make sure tyre screech doesnt get overwritten when braking
        if (currentBrakeForce != maxBrakeForce)
        {
            SoundManager.instance.ModifyVolume(tyreAudioSource, preset.tyreScreechVolume.Evaluate(Mathf.Abs(currentDriftValue)));
        }

        isDrifting = Mathf.Abs(driftValue.x) > 4 && GroundCheck();

       // print("is Drifting " + isDrifting);
    }
    
    public bool GroundCheck() 
    {
        return Physics.Raycast(transform.position, Vector3.down, 1f, GroundLayer);
    }

    private bool RoadCheck() 
    {
        RaycastHit hit; 
        if(Physics.Raycast(transform.position, Vector3.down, out hit, 1f,  GroundLayer)) 
        { 
            return hit.transform.CompareTag(GameManager.instance.ROAD_TAG);
        }
        
        return false;
    }

    //Enables Car Movement
    //Toggle
    public void EnableCarMovement()
    {
        isEnabled = true;
        ToggleLockAllBrakes(false);

    
        
    }
    //Disables Car Movement
    public void DisableCarMovement(bool engineOff = false)
    {
        currentBrakeForce = maxBrakeForce;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        ToggleLockAllBrakes(true);
        isEnabled = false;
        //Fixes annoying sound bug when objective is triggered while moving
        SoundManager.instance.ModifyVolume(tyreAudioSource, 0);
        SoundManager.instance.ModifyPitch(carAudioSource, preset.carRpmToPitch.Evaluate(idleRPM));



        if (engineOff)
        {
            Debug.LogError("Engine off executed");
            ToggleEngine(false);
        }
        
    }

    public void ResetCarPostion() 
    {
        transform.position = StartPosition;
    }

    public void ResetCarRotation() 
    {
        transform.position = new Vector3(transform.position.x, transform.position.y + 5, transform.position.z);
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public bool ReturnCarStatus()
    {
        return isEnabled;
    }
    #endregion

    #region CoolDowns


    #endregion

    #region Lights

    private void ToggleLights()
    {
        lightsEnabled = !lightsEnabled;

        for (int i = 0; i < Lights.Count; i++)
        {
            //Ensures brake lights remain on on when braking
            if (i == 2 && !lightsEnabled && isBraking) return;
            Lights[i].gameObject.SetActive(lightsEnabled);
        }
        LightMaterial.EnableKeyword("_EMISSION");

    }

    private void ToggleBrakeLights()
    {
        for (int i = 2; i < Lights.Count; i++)
        {
            Lights[i].gameObject.SetActive(isBraking || lightsEnabled);
            Lights[i].intensity = isBraking ? 10f : 5f;
        }

        if (isBraking || lightsEnabled)
        {
            LightMaterial.EnableKeyword("_EMISSION");
        }
        else
        {
            LightMaterial.DisableKeyword("_EMISSION");
        }

    }

    #endregion

    #region Collison Events
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Delivery Point"))
        {
            //scriptable obj for notification text
            CanvasController.instance.UpdateInteractUiText(CanvasController.instance.textPresets.DeliverPizzaTextPc);
        }

        if (other.gameObject.CompareTag(GameManager.instance.INSTANT_DESTROY_TAG)) 
        {
            playerManager.InstantDestroy();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Delivery Point"))
        {
            if (Input.GetAxis("Joystick Dpad X") > 0f && GameManager.instance.ReturnJoystickXStatus() || Input.GetKeyDown("f"))
            {
                GameManager.instance.EndCurrentDelivery();
                CanvasController.instance.ClearInteractUiText();
            }
        }
        
  
        if (other.gameObject.transform.CompareTag("Start Delivery") && displaySpeed <= 0 && GameManager.instance.Gamemode != GameManager.gamemode.FinishedDelivery)
        {
            //this needs to be called here not entry so it can be showed when the speed is 0
            CanvasController.instance.UpdateInteractUiText(CanvasController.instance.textPresets.StartDeliveryTextPc);
            
            if (Input.GetAxis("Joystick Dpad X") > 0f && GameManager.instance.ReturnJoystickXStatus() || Input.GetKeyDown("f"))
            {
                GameManager.instance.TryStartDelivery();
                
                CanvasController.instance.ClearInteractUiText();
            }
        }
        else if (other.gameObject.transform.CompareTag("Start Delivery") && displaySpeed > 0)
        {
            CanvasController.instance.ClearInteractUiText();
        }

        if (other.gameObject.transform.CompareTag("Start Delivery") && displaySpeed <= 0 && GameManager.instance.Gamemode == GameManager.gamemode.FinishedDelivery)
        {
            //this needs to be called here not entry so it can be showed when the speed is 0
            CanvasController.instance.UpdateInteractUiText(CanvasController.instance.textPresets.EndDeliveryTextPc);

            if (Input.GetAxis("Joystick Dpad X") > 0f && GameManager.instance.ReturnJoystickXStatus() || Input.GetKeyDown("f"))
            {
                GameManager.instance.EndDelivery();

                CanvasController.instance.ClearInteractUiText();
            }
        }


        if (other.gameObject.transform.CompareTag("Garage") && displaySpeed <= 0)
        {
            //this needs to be called here not entry so it can be showed when the speed is 0
            CanvasController.instance.UpdateInteractUiText(CanvasController.instance.textPresets.EnterGarageTextPc);

            if (Input.GetAxis("Joystick Dpad X") > 0f && GameManager.instance.ReturnJoystickXStatus() || Input.GetKeyDown("f"))
            {
                GameManager.instance.ToggleCarSelect(true);
                CanvasController.instance.ClearInteractUiText();
            }
        }
       
        if(other.gameObject.transform.CompareTag("Fuel Station")  && displaySpeed <= 0)
        {
            if (isAllowedToRefuel)
            {
                CanvasController.instance.UpdateInteractUiText(CanvasController.instance.textPresets.RefuelCarTextPc);


                if (Input.GetAxis("Joystick Dpad X") > 0f && GameManager.instance.ReturnJoystickXStatus() || Input.GetKeyDown("f"))
                {
                    if (GameManager.instance.ReturnFuelMode() == GameManager.FuelMode.Normal)
                    {
                        ToggleRefuel(true, other.transform);
                        other.gameObject.GetComponent<MeshRenderer>().enabled = false;
                    }
                    else
                    {
                        ToggleRefuel(false);
                    }
                }
            }
           
            //Cancel is so the back button is usable for quitting refuelling
            if (Input.GetButtonDown("Cancel") && GameManager.instance.ReturnFuelMode() == GameManager.FuelMode.Refueling || currentFuel >= FuelCapacity)
            {
                ToggleRefuel(false);
            }
        }
        else if (other.gameObject.transform.CompareTag("Fuel Station") && displaySpeed > 0 || currentFuel >= FuelCapacity)
        {
            CanvasController.instance.ClearInteractUiText();
            
            //stops unnesecary calls
            if (GameManager.instance.ReturnFuelMode() == GameManager.FuelMode.Refueling)
            {
                ToggleRefuel(false);
            }
        }

        if (other.gameObject.TryGetComponent(out DialogueTrigger dialogueTrigger) && displaySpeed <= 0)
        {
            CanvasController.instance.UpdateInteractUiText(CanvasController.instance.textPresets.StartDialougeTextPc + dialogueTrigger.ReturnSpeakerName());

            if (Input.GetAxis("Joystick Dpad X") > 0f
                && GameManager.instance.ReturnJoystickXStatus()
                || Input.GetKeyDown("f"))
            {
                if (dialogueTrigger == null) return;
                dialogueTrigger.StartDialouge();
                CanvasController.instance.ClearInteractUiText();
            }
        }
        else if(dialogueTrigger != null && displaySpeed > 0) 
        {
            CanvasController.instance.ClearInteractUiText();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CanvasController.instance.ClearInteractUiText();

        if (other.gameObject.transform.CompareTag("Fuel Station") && !GameManager.instance.ReturnIsDelivery())
        {
            isAllowedToRefuel = true;
            SetIsAllowedToRefuel(true);
            other.gameObject.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (displaySpeed > 10 && collision.gameObject.TryGetComponent(out Destructible destructible))
        {
            destructible.HandleDestruction();
            ParticleEffectsControl.instance.PlayBaseDestructionEffectAT(collision.GetContact(0).point);
            playerManager.DamageCar(0f);

            if(collision.gameObject.TryGetComponent(out Streetlight streetlight)) 
            {
               //switches street light off on collision
                streetlight.HandleDestruction();
            }
        }

        if (collision.gameObject.CompareTag(GameManager.instance.INSTANT_DESTROY_TAG))
        { 
           playerManager.InstantDestroy();
        }
    }



    #endregion

    #region Special
    void PlayTyreScreechOnBrake(bool play) 
    { 
        float volume = play ? 1f : 0f;
        SoundManager.instance.ModifyVolume(tyreAudioSource, volume);
    }
    #endregion

    #region Return Variables
    public Transform ReturnCameraLookAtTrans() 
    {
        return CameraLookAt;
    }
    #endregion

    #region Set Values
    public void SetPlayer(PlayerManager player) 
    {
        playerManager = player;
        playerManager.ResetHealth();
    }

    private void SetIsReversing(bool reversing) 
    { 
        if(isReversing == reversing) return;
        
        isReversing = reversing;

        if(isReversing) 
        {
            CameraManager.instance.SwitchCameraMode(CameraManager.CameraView.Reverse);
        }
        else 
        {
            CameraManager.instance.SwitchCameraMode(CameraManager.CameraView.Normal);
        }
    }


    private void TakeFuel(float fuelToTake) 
    { 
        currentFuel -= fuelToTake;

        if (currentFuel <= FuelCapacity / 4) 
        {
            LowOnFuelEvent?.Invoke();
        }

        if(currentFuel <= 0) 
        {
            OutOfFuelEvent?.Invoke();
        }

    }

    public void GiveFuel(float fuelToGive, bool chargeForFuel)
    {
        int price = GameManager.instance.fuelPrice;

        float unit = GameManager.instance.fuelUnitMultiplier;

        float playerMoney = playerManager.ReturnMoney();

        QuestManager cachedQuest = GameManager.instance.currentQuest;

        CanvasController.instance.UpdateCarRefuelUi(playerMoney, price);

        //if first mission and objective is reach poin
        //instead of this make an event for quest 1 refuel objective
        if (cachedQuest != null && cachedQuest.quest.id == 0 &&
            cachedQuest.quest.ReturnCurrentObjective().type == Objective.ObjectiveType.ReachValue)
        {
            cachedQuest.FeedReachValue(currentFuel);
            chargeForFuel = false;
        }

        if (chargeForFuel && playerMoney < price )
        {
            CanvasController.instance.UpdateNotificationText("You don't have enough money for fuel.\n " +
                "Go get some money by doing delivery!");
            return;
        }

        if (currentFuel + fuelToGive > FuelCapacity)
        {
            currentFuel = FuelCapacity;
        }
        else
        {
            currentFuel += fuelToGive;
           
            if (chargeForFuel) 
            {
                playerManager.TakeMoney((fuelToGive / unit) * price);
            }
        }
        
        CanvasController.instance.UpdateRefuelBar(currentFuel, FuelCapacity);
        CanvasController.instance.UpdateCarRefuelUi(playerMoney, price);
        if (currentFuel > FuelCapacity / 4)
        {
            HighOnFuelEvent?.Invoke();
        }

    }
    
    //Less precise way to set fuel
    /// <summary>
    /// fill ratio denominator(frd) determines what fraction of a fuel tank gets filled
    /// eg.: if frd = 1, full tank, if 4, a quarter
    /// </summary>
    /// <param name="fillRatioDenominator"></param>
    public void SetFuelByDenomination(int fillRatioDenominator) 
    { 
        if(fillRatioDenominator < 1) 
        {
            return;        
        }

        //to assign capacity as this gets called before assign for some
        //reason
        if(FuelCapacity <= 0) 
        { 
         FuelCapacity = preset.FuelCapacity;
        }

        currentFuel = FuelCapacity/fillRatioDenominator;
        print("works" + fillRatioDenominator + "/" + FuelCapacity + "/"+ currentFuel);

        if (currentFuel > FuelCapacity / 4) 
        {
            HighOnFuelEvent?.Invoke();
        }
    
    }

    #endregion

    #region Refuelling

   //this is used to set all the variables for refuelling to keep the collision detection cleaner
   //and so it can be called from other places such as the return button
    public void ToggleRefuel(bool enabled, Transform other = null) 
    {
        if (enabled) 
        {
           SetIsAllowedToRefuel(false);
            
            DisableCarMovement();

            CanvasController.instance.UpdateRefuelBar(currentFuel, FuelCapacity);

            GameManager.instance.SetRefuelingStatus(GameManager.FuelMode.Refueling);


            CanvasController.instance.ClearInteractUiText();

            if (other != null)
            {

                //Works out where the fuel pump is in relation to the car
                float dotProduct = WorkOutWhereObjectIs(other.GetComponentInParent<Transform>());

                if (dotProduct > 0)
                {
                    CameraManager.instance.pumpSide = CameraManager.PumpSide.Right;
                    CameraManager.instance.SwitchCameraMode(CameraManager.CameraView.Refuel);
                }
                else if (dotProduct < 0)
                {
                    CameraManager.instance.pumpSide = CameraManager.PumpSide.Left;
                    CameraManager.instance.SwitchCameraMode(CameraManager.CameraView.Refuel);
                }
            }
        }
        else 
        {
            StartCoroutine(DelayRefufelStatusSet());
            CameraManager.instance.SwitchCameraMode(CameraManager.CameraView.Normal);
            EnableCarMovement();
        }
    }
    
    public void SetIsAllowedToRefuel(bool enabled) 
    {
        isAllowedToRefuel = enabled; 
    }

    //Delays the refuel status set when pressing escape, so it doesnt instantly pause the game when backing out
    private IEnumerator DelayRefufelStatusSet()
    {
        yield return new WaitForSeconds(0.1f);

        GameManager.instance.SetRefuelingStatus(GameManager.FuelMode.Normal);
    }
   
    public float ReturnFuelCapacity() 
    { 
     return FuelCapacity;
    }
    #endregion

        private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, Vector3.down, Color.yellow);
    }
}
