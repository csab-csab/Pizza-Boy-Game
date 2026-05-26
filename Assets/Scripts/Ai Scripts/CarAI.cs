using System.Collections.Generic;
using UnityEngine;

public class CarAI : MonoBehaviour
{
    #region Parent Objs and Lists
    //Assign In Inspector
    [SerializeField] Transform WheelColliderParent;
    private List<WheelCollider> wheelColliders = new List<WheelCollider>();

    [Header("Visuals")]
    //Assign In Inspector
    [SerializeField] Transform WheelTransformParent;
    [SerializeField] private List<GameObject> characters = new List<GameObject>();

    private Transform LightsTransformParent;
    private Transform CharactersTransform;
    private List<Transform> wheelTransforms = new List<Transform>();
    private List<Light> lights = new List<Light>();
    #endregion

    #region Info
    [Header("Information")]
    private float distanceToTarget;
    private float distanceToObstacle = 1000f;
    private float distanceToJunctionPoint = Mathf.Infinity;
    private float speed;

    private Rigidbody rb;
    private Transform obstacleTransform;

    private bool isBraking;
    private bool lightsEnabled = true;

    private bool isOutOfPoints;
    private bool isNextPointJunction;
    private bool shouldStopForJunction;
    private bool isThereAnObstacle;

    //cached reference for the junction point
    private JunctionPoint jp;
    #endregion

    #region Inputs

    [SerializeField] float EnginePower = 100f;
    [SerializeField] float maxSidewaysInput = 35f;
    [SerializeField] float maxBrakeInput = 10000f;
    [SerializeField] float collisionAvoidanceDistance = 3;
    [SerializeField] private float distanceToLookForJunction = 7;
    [SerializeField] private float distanceToFindNextPoint = 3;
    [SerializeField] AnimationCurve targetSpeedAtAngle;

    private float sidewaysInput = 0f;
    private float forwardInput = 0f;
    private float brakeInput = 0f;
    private float stoppingDistance = 0f;

    [Header("Path Finding")]
    [SerializeField] private Path path;
    [Tooltip("Set to the index of the first path-point targeted by AI - 1. Indexes start with 0")]
    [SerializeField] private int current_point = -1;

    [SerializeField]private Vector3 targetPosition;
    [SerializeField]private Vector3 nextTargetPosition;

#endregion

    void Start()
    {
        Initialise();
    }

    // Update is called once per frame
    void Update()
    {
        CalculateSpeed();
        CheckForObstacles();
        WorkOutInputs();
        HandleCar();
        FindNextPoint();
    }


    #region Checks and Calculations
    private void CalculateSpeed()
    {
        speed = rb.linearVelocity.magnitude * 4;
    }

    private void WorkOutInputs()
    {
        distanceToJunctionPoint = (jp == null) ? Mathf.Infinity : (jp.transform.position - transform.position).magnitude;

        Vector3 dirToTarget = targetPosition - transform.position;
        distanceToTarget = dirToTarget.magnitude;
        float targetSteerAngle = Vector3.SignedAngle(transform.forward, dirToTarget, Vector3.up);
        sidewaysInput = Mathf.Clamp(targetSteerAngle, -maxSidewaysInput, maxSidewaysInput);

        Vector3 dirBetweenPoints = nextTargetPosition - targetPosition;
        float predictedCurveSharpness = Vector3.SignedAngle(transform.forward, dirBetweenPoints, Vector3.up);

        CheckIfNeedToBrake(Mathf.Max(Mathf.Abs(predictedCurveSharpness), Mathf.Abs(targetSteerAngle)));
        if(isBraking)
        {
            forwardInput = 0f;
        }
        else if (distanceToTarget > stoppingDistance)
        {
            forwardInput = 1f;
        }      
    }

    private void CheckForObstacles() 
    {
        // Raycast to check for obstacles in the car's path
        RaycastHit hit;
        isThereAnObstacle = Physics.Raycast(transform.position + transform.up * 0.6f, transform.forward, out hit, stoppingDistance + collisionAvoidanceDistance)
                           && (hit.transform.CompareTag("Obstacle") || hit.transform.CompareTag("Ai Car") || hit.transform.CompareTag("Player Car"));
        if (isThereAnObstacle)
        {
            obstacleTransform = hit.transform;
            distanceToObstacle = Vector3.Distance(transform.position, hit.transform.position);
        }
    }

    //Checks if the car needs to stop for the next junction
    private void CheckToBrakeAtNextJunction()
    {
        if(isNextPointJunction)
        {
            if (jp.state == JunctionPoint.JunctionState.Stop)
            {
                shouldStopForJunction = true;
            }
            else if(jp.state == JunctionPoint.JunctionState.Go)
            {
                shouldStopForJunction = false;

            }
        }
    }

    void CheckIfNeedToBrake(float targetSteerAngle)
    {

        // Calculate the braking distance based on the current speed
        // * 100 for correct magnitude, +1 to adjust to distance calculations
        stoppingDistance = (speed * speed) * 100 / (2 * maxBrakeInput) + 1;

        // Calculate the remaining distance to the target point
        float remainingDistance = Mathf.Max(0, distanceToJunctionPoint - stoppingDistance);

        //Calculates the sharpness of a corner based on how much steering is reuqired
        float cornerSharpness = Mathf.Abs(targetSteerAngle) / maxSidewaysInput;

        CheckToBrakeAtNextJunction();
        if (isThereAnObstacle)
        {
            brakeInput = maxBrakeInput;
        }
        else if (shouldStopForJunction && remainingDistance <= stoppingDistance)
        {
            // Calculate the braking input based on the remaining distance
            brakeInput = Mathf.Clamp01((stoppingDistance - remainingDistance) / stoppingDistance) * maxBrakeInput;
        }
        else if (targetSpeedAtAngle.Evaluate(cornerSharpness) < speed) 
        {
            brakeInput = Mathf.Clamp01(cornerSharpness) * maxBrakeInput;
        }
        else
        {
            // No need to brake
            brakeInput = 0;
        }

        BrakeCar(brakeInput);
    }


    #endregion


    #region Handle Car Stuff
    void HandleCar()
    {
        for (int i = 0; i < 4; i++)
        {
            //saves typing out wheelColliders
            WheelCollider w = wheelColliders[i];
            w.motorTorque = (forwardInput * EnginePower) / 4;
            w.brakeTorque = brakeInput;
            if (i < 2)
            {
                w.steerAngle = sidewaysInput;
            }

            RotateWheelVisual(w, wheelTransforms[i]);
        }
    }

    void BrakeCar(float brakeInput)
    {
        this.brakeInput = brakeInput;
        
        isBraking = brakeInput > 0;
        ToggleBrakeLights();
    }
    #endregion

    #region PathFinding

    void FindNextPoint()
    {
        //Note: This implicitly assumes distanceToLookForJunction > distanceToFindNextPoint
        if (shouldStopForJunction
            || distanceToTarget > distanceToLookForJunction
            || (distanceToTarget > distanceToFindNextPoint && !path.IsLast(current_point + 1)))
            return;

        current_point++;
        isOutOfPoints = current_point >= path.Length-1;

        if (isOutOfPoints)
        {
            forwardInput = 0;
            BrakeCar(maxBrakeInput);
            current_point = 0;
            return;
        }

        targetPosition = path.GetPoint(current_point);
        nextTargetPosition = path.GetPoint(current_point +1);
    }

    void HandleJunction(JunctionPoint junction)
    {
        #region Logic for junction with giveway/stop sign
        jp = junction;
        if (jp != null)
        {
            isNextPointJunction = true;
            jp.PointReached();

            //random path
            path = jp.GetNextPath(path);
            //Reset path finding for next path
            current_point = -1;
            nextTargetPosition = path.GetPoint(current_point + 1);
        }
        #endregion
    }

    //resets all values related to junctions when the next point is not a junction
    void ResetJunctionValues() 
    {
        jp = null;
        isNextPointJunction = false;
    }
    #endregion

    #region Visuals
    void RotateWheelVisual(WheelCollider wheelCollider, Transform wheelTransform)
    {

        Vector3 pos;
        Quaternion rot;

        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }


    #endregion

    #region Lights
    private void ToggleBrakeLights()
    {
        int startIdx = 2;

        for (int i = startIdx; i < lights.Count; i++)
        {
            lights[i].gameObject.SetActive(isBraking || lightsEnabled);
            lights[i].intensity = isBraking ? 10f : 5f;
        }
    }

    private void ToggleLights(bool enabled)
    {
        lightsEnabled = enabled;

        foreach(Light light in lights) 
        { 
            light.gameObject.SetActive(enabled);
        }
    }
    #endregion

    #region Collision Detection

    private void OnCollisionEnter(Collision collision)
    {
        if (speed > 10 && collision.gameObject.TryGetComponent(out Destructible destructible))
        {
            destructible.HandleDestruction();
            ParticleEffectsControl.instance.PlayBaseDestructionEffectAT(collision.GetContact(0).point);
        }
    }

    #endregion

    #region Setup
    private void Initialise()
    {
        rb = GetComponent<Rigidbody>();

        GetWheelColliders();
        GetLights();
        GetCharacters();
        FindNextPoint();
    }
    
    private void GetWheelColliders() 
    {
        //gets wheel colliders for each wheel
        for (int i = 0; i < 4; i++)
        {
            wheelColliders.Add(WheelColliderParent.GetChild(i).GetComponent<WheelCollider>());
            wheelTransforms.Add(WheelTransformParent.GetChild(i).GetComponent<Transform>());
        }
    }

    private void GetLights() 
    {
        LightsTransformParent = this.gameObject.transform.Find("Lights").transform;

        if (LightsTransformParent == null) return;

        lights.Clear();

        for (int i = 0; i < 4; i++) 
        {
            lights.Add(LightsTransformParent.GetChild(i).GetComponent<Light>());
        }
    }
    
    private void GetCharacters()
    {
        CharactersTransform = this.gameObject.transform.Find("Characters").transform;

        if (CharactersTransform == null) return;

         characters.Clear();

        for (int i = 0; i < CharactersTransform.childCount; i++)
        {
            if (!CharactersTransform.GetChild(i).name.Contains("root"))
            {
                characters.Add(CharactersTransform.GetChild(i).gameObject);
            }
        }

        ChooseRandomCharacter();
    }

    private void ChooseRandomCharacter() 
    { 
       if(characters.Count == 0) return;
        
        foreach (GameObject g in characters)
        {
            g.SetActive(false); 
           
        }

        int randInt = Random.Range(0, characters.Count);

        characters[randInt].SetActive(true);
    }
    #endregion

}
