using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class GameManager : MonoBehaviour, IDataPersistance
{
   public static GameManager instance;

    public enum gamemode {Freemode,Delivery, FinishedDelivery, Track};
    public gamemode Gamemode { get; private set; }


    public enum DebugMode { Off, On };
    public DebugMode debugMode { get; private set; }

    #region TAGS
    public string DESTRUCTIBLE_TAG { get; private set; } = "Destructible" ;

    public string INDESTRUCTABLE_TAG { get; private set; } = "Indestructable";

    public string INSTANT_DESTROY_TAG { get; private set; } = "Instant Destroy";

    public string ROAD_TAG { get; private set; } = "Road";

    public string PLAYER_CAR_TAG { get; private set;} = "Player Car";
    #endregion

    #region Player References
    [Header("Player References")]
    //The parent of all of the player objects
    [SerializeField] Transform PlayerParent;
    private PlayerManager player;
    #endregion

    #region Script References
    [Header("Script References")]

    [SerializeField]private CarController carController;
    private LightingManager lightingManager;


    public QuestManager currentQuest { get; private set; }
    #endregion

    #region Pause Variables
    [Header("Pause Variables")]
    [SerializeField] GameObject PauseMenu;
    #endregion

    #region Car Select
    [Header("Car Select Variables")]
    [SerializeField] GameObject car_select;
    [SerializeField]GameObject car_select_cam;

    //Reference To Actual Rendering Camera
    [SerializeField] Camera car_cam;

    [SerializeField] Transform DefaultCarSpawn;
    [SerializeField] GameObject DefaultCar;
    [SerializeField] GameObject DreamCar;

    #endregion

    #region FreeLook and Cutscene Camera
    [Header("FreeLook and Cutscene Camera")]
    [SerializeField] Camera FreeLookCam;
    bool freeLookCamOn;
    bool freeLookHudOn;
    #endregion

    #region Car Destroyed and Out of fuel Variables
    [Header("Car Destroyed and out of fuel Variables")]
    [SerializeField] Camera CarDestroyedCam;
    [SerializeField] float RecoveryCost = 100;
    [SerializeField] Camera CarOutOfFuelCam;
    [SerializeField] PlayableDirector CarOutOfFuelCutscene;
    //Used so we can set appropiate fuel level in car select
    [SerializeField] private float currentFuelLevel;
    #endregion


    #region Game State Variables
    public enum GameState { CarSelect, Refueling, CarDestroyed, CarOutOfFuel, QuestFailed, Paused, Playing, Cutscene};
    public GameState gameState { get; private set;}


    //used as a bool to check if refueling or not
    public enum FuelMode { Normal, Refueling };

    public FuelMode fuelMode { get; private set; }

    public int fuelPrice = 1;

    //this decides how much real fuel is one unit of fuel
    //used for pricing
    //so for example 1 unit is = to fuelUnitMultiplier and that costs the fuel price
    public float fuelUnitMultiplier = 50f;
    #endregion

    #region CachedPauseVariables
     //These cached bools will be used to determine which ui elements to re enable
     //after pause based on which were enabled at the time of pausing
     bool wasGamePlayUi = false;
     bool wasDialougeUi = false;
     bool wasRefuelUi = false;
    #endregion

    #region Track References
    [Header("Time Trial Variables")]
    [SerializeField] double countDownStartTime;
    [SerializeField] double countDownDuration;
    [SerializeField] bool isCountingDown;
    [SerializeField] float lapTimerMilli;
    [SerializeField] float lapTimerSecs;
    [SerializeField] float lapTimerMins;
    float LastLapTimer;

    public bool isLapTimerActive;

    #endregion

    #region Pizza Delivery Stuff Variables
    [Header("Pizza Delivery Related Variables")]

    //this value is used to calculate the payout and xp at the end of the delivery
    float timeItTookToDeliver = 0;

    [SerializeField] float basePayout = 250f;
    [SerializeField] float maxBonus = 200f;
    //time after which there is no bonus
   
    [Header("Set Variable Here")]
    [SerializeField] float base_time_to_deliver;
    [Header("Serialized for debug")]
    [SerializeField] float timeToDeliver;
    [Header("Set Variable Here")]
    [SerializeField] float timeToAdd;


    //using my amazing research skills, I found out pizza is around
    //250 �C so this number will be used to calculate the pizza temp timer
    private static float HOT_PIZZA_TEMP = 250f;

    private static float ROOM_TEMP = 25f;

    //this so the timer can be displayed as temperature instead of timer
    private float currentPizzaTemp = HOT_PIZZA_TEMP;

    //private
    [SerializeField] int pizzasToDeliver;
    //private, serialize used for debug
    [SerializeField] int currentNoPizzas;

    public delegate void DeliveryCompleted();

    public static DeliveryCompleted OnDeliveryCompleted;

    [Space(10)]

    [Header("GameObject References")]
    [SerializeField] GameObject delivery_point;
    [SerializeField] GameObject objective_point;
    
    public enum ArrowType{Delivery, Objective};
    public ArrowType arrowType;
    [SerializeField] GameObject delivery_trigger_point;
    [SerializeField] GameObject garage_trigger_point;
    [SerializeField] GameObject refuel_trigger_point_parent;

    [SerializeField]List<GameObject> fuel_triggers = new List<GameObject>();


  

    [SerializeField]GameObject possibleDeliveryPointsParent;
    //Actual delivery points
    [SerializeField]List<Transform> Houses;
    [SerializeField]string pizzaThrowEffectTag = "PizzaThrowEffectAnchor";
    private int lastHouse;

    [Header("WayPoint")]
    bool doesDeliveryArrowExist = false;
    [SerializeField]GameObject delivery_arrow_prefab;
    GameObject deliveryArrowIns;
    #endregion

    #region Input Related Variables

    #region  Saving/Loading Refs
    int TransmissionTypeIndex;
    #endregion

    [Header("Input Variables")]

    public bool isJoystickDpadXEnabled;
    public bool isJoystickDpadYEnabled;
    
    public enum InputDevice { KeyboardAndMouse, Controller }
    public InputDevice inputDevice;

    #endregion
   
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        //Change this when fully implemented
        player = PlayerParent.GetChild(0).GetComponent<PlayerManager>();

        #region Intialise Delivery Point
        
        Houses.Clear();
        //Do this so I can remove the child objects of delivery points; the pizza throw at objects
        Transform[] tempArray = possibleDeliveryPointsParent.GetComponentsInChildren<Transform>();
        
        foreach(Transform t in tempArray)
        {
            if(t.CompareTag("PizzaThrowEffectAnchor"))
            {
                continue;   
            }
            
            Houses.Add(t);
        }
        #endregion
        
        #region Get Self Contained References
        try
        {
            lightingManager = GetComponent<LightingManager>();
        }
        catch(NullReferenceException) 
        {
           Debug.LogError("Lighting Manager Cannot Be Found On the Game Manager Object!!!");
        }
        #endregion
        #region Ui Intialisation
        
        CanvasController.instance.EnableDisableDebugUi(false);
        CanvasController.instance.ToggleDeliveryUi(false);
        CanvasController.instance.ClearInteractUiText();
        //ONLY COMMENTED OUT FOR TEST BUILD
       /*
        #if !UNITY_EDITOR
        CanvasController.instance.ToggleCursor(false);
        #endif*/
        #endregion

        #region Get external Refs
        if (refuel_trigger_point_parent != null)
        {
            for (int i = 0; i < refuel_trigger_point_parent.transform.childCount; i++)
            {
                fuel_triggers.Add(refuel_trigger_point_parent.transform.GetChild(i).gameObject);
            }
        }
        #endregion

        #region Events
        QuestManager.OnSetValue += TriggerSetValueEvent;
        #endregion

        Application.targetFrameRate = 60;

      
    }

        

    private void Update()
    {
        #region Input Handling

        //Unpause in pause menu control
        if (Input.GetButtonDown("Pause") && gameState != GameState.Paused)
        {
            PauseGame();
        }

        if (Input.GetAxisRaw("Mouse X") != 0 ^ Input.GetAxisRaw("Mouse Y") != 0)
        {
            inputDevice = InputDevice.KeyboardAndMouse;
        }
        else if (Input.GetAxisRaw("RT") > 0 ^ Input.GetAxisRaw("LT") > 0 ^ Input.GetAxisRaw("Joystick Look X") > 0 ^ Input.GetAxisRaw("Joystick Look Y") > 0 ^ Input.GetAxisRaw("Joystick Dpad X") != 0
        ^ Input.GetAxisRaw("Joystick Dpad Y") != 0)
        {
            inputDevice = InputDevice.Controller;
        }

        #region Comment
        /* 
         Resets value of Joystick so its usable again 
         */
        #endregion
        if (Input.GetAxisRaw("Joystick Dpad X") > 0 || Input.GetAxisRaw("Joystick Dpad X") < 0) 
        { 
         isJoystickDpadXEnabled = false;
        }

        if (Input.GetAxisRaw("Joystick Dpad Y") > 0 || Input.GetAxisRaw("Joystick Dpad Y") < 0)
        {
            isJoystickDpadYEnabled = false;
        }

        if (!isJoystickDpadXEnabled && Input.GetAxisRaw("Joystick Dpad X") == 0)
        {
            isJoystickDpadXEnabled = true;
        }
        if (!isJoystickDpadYEnabled && Input.GetAxisRaw("Joystick Dpad Y") == 0)
        {
            isJoystickDpadYEnabled = true;
        }

        if(Input.GetButtonDown("Freelook Camera")) 
        {
            ToggleFreeLookCamera(!freeLookCamOn);
        }

        if(Input.GetButtonDown("Freelook HUD") && freeLookCamOn) 
        {
            freeLookHudOn = CanvasController.instance.ReturnfreeLookHudStat();
            CanvasController.instance.ToggleFreeLookCamUi(!freeLookHudOn);
        }
        #region Debug Inputs
            
            if (Input.GetKeyDown(KeyCode.Backslash))
            {
                if (debugMode == DebugMode.Off)
                {

                    debugMode = DebugMode.On;
                    CanvasController.instance.EnableDisableDebugUi(true);

                }
                else
                {
                    debugMode = DebugMode.Off;
                    CanvasController.instance.EnableDisableDebugUi(false);
                }
            }

            if (debugMode == DebugMode.On)
            {
                if (Input.GetKey(KeyCode.Equals))
                {
                    lightingManager.ModifyTimeScaleFactor(1);
                }
            
                if (Input.GetKey(KeyCode.Minus))
                {
                    lightingManager.ModifyTimeScaleFactor(-1);
                }
            
                if (Input.GetKeyDown(KeyCode.N))
                {
                    CanvasController.instance.UpdateNotificationText("This is a notification");
                }
            
                if (Input.GetKey(KeyCode.R))
                {
                    carController.ResetCarPostion();
                    carController.ResetCarRotation();
                }
            
                if (Input.GetKeyDown(KeyCode.T))
                {
                    ParticleEffectsControl.instance.ThrowPizzaOutCar(carController.gameObject);
                }

                if(Input.GetKeyDown(KeyCode.Comma))
                {
                     CarSelectorScript.triggerSpawnCar?.Invoke(DefaultCarSpawn,DefaultCar, 1, TransmissionTypeIndex, nameof(GameManager.instance.Start), -1);
                     SetPlayState();
                }

            }

        #endregion
        #endregion

        #region Timers
        // Time elapsed according to the audio clock
        if (isCountingDown) {
            double elapsed = AudioSettings.dspTime - countDownStartTime;
            double remaining = countDownDuration - elapsed;

            CanvasController.instance.UpdateCountDownUi((int)remaining);

            if (remaining <= 0)
            {
                carController.EnableCarMovement();
                isCountingDown = false;

            } 
        }

        

        if (Gamemode == gamemode.Delivery && timeToDeliver > 0)
        {
            timeToDeliver -= Time.deltaTime;

            //converting so we can get delivery time as a value between 0 and 1 for the lerp below
            float normalizedTimeTodeliver = Mathf.Clamp01(timeToDeliver/base_time_to_deliver);

            currentPizzaTemp = Mathf.Lerp(ROOM_TEMP, HOT_PIZZA_TEMP, normalizedTimeTodeliver);

            CanvasController.instance.UpdatePizzaTemperatureUI(currentPizzaTemp);

            timeItTookToDeliver += Time.deltaTime;
        }
        else if(Gamemode == gamemode.Delivery && timeToDeliver <= 0 && currentNoPizzas > 0)
        {
          Defeat();
        }
        
        
        if (isLapTimerActive) 
        { 
            lapTimerMilli += Time.deltaTime * 1000;

            if (lapTimerMilli >= 1000) 
            {
                lapTimerMilli = 0;
                lapTimerSecs++;
            }
            
            if(lapTimerSecs >= 60) 
            {
                lapTimerSecs = 0;
                lapTimerMins++;
            }
        }

        #endregion


        #region Handle Inputs during different gameState scenarios
        if (fuelMode == FuelMode.Refueling) 
        {
            
            carController.GiveFuel(Input.GetAxis("RT") * fuelUnitMultiplier * Time.deltaTime, true);
            carController.GiveFuel(Input.GetAxis("Jump") * fuelUnitMultiplier * Time.deltaTime, true);
            CanvasController.instance.UpdateRefuelBar(carController.currentFuel, carController.FuelCapacity);
        }
        //if car is destroyed, respawn car
        if (gameState == GameState.CarDestroyed && Input.GetButtonDown("Jump") 
            ^ Input.anyKeyDown)
        {
            CarSelectorScript.instance.SpawnSpecifiedCar(DefaultCarSpawn,  CarSelectorScript.instance.ReturnLastSpawnedCar(), 1, TransmissionTypeIndex, nameof(GameManager.instance.Start));
            CanvasController.instance.EnableDisableGameplayUi(true);
        }

        if(gameState == GameState.CarOutOfFuel && Input.GetButtonDown("Jump") ^ Input.anyKeyDown)  
        {
            ResetCarOutOfFuel();
        }
        #endregion

        if (doesDeliveryArrowExist)
        {
            PointToDeliveryPoint();
        }
    }

    #region Game States
   //used by button to start game by simply spawning car
   //used for testing
    public void StartGameFreemode(StartGameDebug.CarToSpawn car) 
    {
        debugMode = DebugMode.Off;
        Gamemode = gamemode.Freemode;

        
        switch (car) 
        { 
            case StartGameDebug.CarToSpawn.Emma:
                CarSelectorScript.triggerSpawnCar?.Invoke(DefaultCarSpawn, DefaultCar, 1, TransmissionTypeIndex,nameof(StartGameFreemode), -1);
                break;
            case StartGameDebug.CarToSpawn.Raiden:
                CarSelectorScript.triggerSpawnCar?.Invoke(DefaultCarSpawn, DreamCar, 1, TransmissionTypeIndex,nameof(StartGameFreemode), -1);
                break;
        }
        
        CanvasController.instance.EnableDisableGameplayUi(true);
    }
    
    private void ModifyGameState(GameState _gameState) 
    {
        GameState previousGameState = gameState;
        
        gameState = _gameState;
        
        //Reset Car Destroyed Variables
        //Avoids Unnesecary call by checking if previous state was car destroyed.
        if(previousGameState == GameState.CarDestroyed
         && gameState != GameState.CarDestroyed) 
        { 
            CarDestroyedCam.gameObject.SetActive(false);
            CanvasController.instance.ToggleCarDestroyedUi(false);
            ParticleEffectsControl.instance.PizzaFallOnCarDestroy(false);
        }
    }


    public void PauseGame() 
    {
        if (gameState == GameState.CarSelect || gameState == GameState.Refueling || gameState == GameState.Cutscene || gameState == GameState.CarDestroyed || 
            gameState == GameState.CarOutOfFuel) return;
        
        if (gameState == GameState.Playing) 
        {
            Time.timeScale = 0f;
            ModifyGameState(GameState.Paused);

            //Disable Gameplay Cam
            CameraManager.instance.ToggleMainCamera(false);
            //Disable Directional light for pause menu to be lit correctly
            lightingManager.ToggleDirectionalLight(false);
            PauseMenu.SetActive(true);

            SoundManager.instance.ToggleMuteAudioForPause(true);
            SoundManager.instance.ToggleAmbientSounds(false);

            if (CanvasController.instance.ReturnGamePlayUiOn())
            {
                wasGamePlayUi = true;
                CanvasController.instance.EnableDisableGameplayUi(false);
            }

            if (CanvasController.instance.ReturnDialougeUiOn()) 
            { 
                wasDialougeUi = true;
                CanvasController.instance.ToggleDialougeUi(false, true);
            }

            if (CanvasController.instance.ReturnRefuelUiOn()) 
            { 
                wasRefuelUi = true;
                CanvasController.instance.ToggleRefuelUi(false);
            }

            CanvasController.instance.EnableDisableDebugUi(false);
            CanvasController.instance.ToggleCursor(true);
        }
        else if(gameState == GameState.Paused)
        {
            Time.timeScale = 1f;
            ModifyGameState(GameState.Playing);
            
            //Disable Gameplay Cam
            CameraManager.instance.ToggleMainCamera(true);
            //Disable Directional light for pause menu to be lit correctly
            lightingManager.ToggleDirectionalLight(true);
            PauseMenu.SetActive(false);

            SoundManager.instance.ToggleMuteAudioForPause(false);
             SoundManager.instance.ToggleAmbientSounds(true);

            if (wasGamePlayUi)
            {
                wasGamePlayUi = false;
                CanvasController.instance.EnableDisableGameplayUi(true);
            }

            if (wasDialougeUi) 
            { 
                wasDialougeUi = false;
                CanvasController.instance.ToggleDialougeUi(true);
            }

            if (wasRefuelUi) 
            { 
                wasRefuelUi = false;
                CanvasController.instance.ToggleRefuelUi(true);
            }
            
            if (debugMode == DebugMode.On)
            {
                CanvasController.instance.EnableDisableDebugUi(true);
            }

            CanvasController.instance.ToggleCursor(false);
        }
    }

    public void SetRefuelingStatus(FuelMode mode)
    {
        fuelMode = mode;

        switch (fuelMode)
        {
            case FuelMode.Refueling:
                ModifyGameState(GameState.Refueling);
                CanvasController.instance.ToggleRefuelUi(true, carController);
                CanvasController.instance.ToggleCursor(true);
                break;

            case FuelMode.Normal:
                ModifyGameState(GameState.Playing);
                CanvasController.instance.ToggleRefuelUi(false);
                CanvasController.instance.ToggleCursor(false);
                break;
        }
    }

    //Use only with return button 
    public void TurnOffRefuelInCarController()
    {
        carController.ToggleRefuel(false);
    }

  
    public void EnableCar()
    {
        if (carController != null)
        {
            carController.EnableCarMovement();
        }
    }
   
   
   
    //Disables Car Control
    //Use cautiously 
    public void DisableCar(bool enabled)
    {
        if (carController != null)
        {
            carController.DisableCarMovement(enabled);
        }
    }

    //subcribe to car controller out of fuel event
    private void CarOutOfFuel()
    {

        if (QuestManager.isQuestActive)
        {
            QuestManager.OnObjectiveFailed?.Invoke("Car ran out of fuel.");
        }
        else
        {
            ModifyGameState(gameState = GameState.CarOutOfFuel);

            CanvasController.instance.EnableDisableGameplayUi(false);
            CanvasController.instance.ToggleCarOutOfFuelUi(true, RecoveryCost);
            
            CarOutOfFuelCam.gameObject.SetActive(true);

            CarOutOfFuelCutscene.Play();
        }

        if (doesDeliveryArrowExist)
        {
            DestroyPointerArrow();
        }

    }

    private void ResetCarOutOfFuel() 
    {
        //chagne this to just outside of fuel station pos
        carController.transform.position = DefaultCarSpawn.position;

        player.TakeMoney(RecoveryCost);
        carController.SetFuelByDenomination(4);


        ModifyGameState(gameState = GameState.Playing);

        CanvasController.instance.EnableDisableGameplayUi(true);
        CanvasController.instance.ToggleCarOutOfFuelUi(false);

        CarOutOfFuelCam.gameObject.SetActive(false);

        CarOutOfFuelCutscene.Stop();
    }


    public void DestroyCar() 
    {
        if (QuestManager.isQuestActive)
        {
            QuestManager.OnObjectiveFailed?.Invoke("Car was destroyed");
        }
        else
        {
            ModifyGameState(GameState.CarDestroyed);
            //Destroys current car and disables camera
            if (carController != null)
            {
                car_cam.gameObject.SetActive(false);
                Destroy(carController.gameObject);
            }

            if (doesDeliveryArrowExist)
            {
                DestroyPointerArrow();
            }

            CarDestroyedCam.gameObject.SetActive(true);
            ParticleEffectsControl.instance.PizzaFallOnCarDestroy(true);
            ParticleEffectsControl.instance.ResetCarFX();

            CanvasController.instance.EnableDisableGameplayUi(false);
            CanvasController.instance.ToggleCarDestroyedUi(true);
        }


        if (doesDeliveryArrowExist)
        {
            DestroyPointerArrow();
        }
    }

    /// <summary>
    /// Destroys current player car without triggering car destroy screen and etc.
    /// Created for use by quests that require car to respawn for example
    /// </summary>
    public void ForceDestroyCurCar() 
    {
        if (carController != null)
        {
            car_cam.gameObject.SetActive(false);
            Destroy(carController.gameObject);
        }

        if (doesDeliveryArrowExist)
        {
            DestroyPointerArrow();
        }
        
        carController = null;
    }

    //this method exists so status can be accessed from the cutscene
    //man. script (its a priv method)
    public void SetCutsceneState() 
    {
        ModifyGameState(GameState.Cutscene);
    }

      //this method exists so status can be accessed from the cutscene
    //man. script (its a priv method)
    public void SetPlayState() 
    {
        ModifyGameState(GameState.Playing);
    }

    #endregion

    #region Trigger Timers
    //Countdown is synced up perfectly to audio
    public void StartCountDown() 
    {
        carController.DisableCarMovement();
        countDownDuration = 4;
        SoundManager.instance.PlayCountdownAudio();
        countDownStartTime = AudioSettings.dspTime;
        isCountingDown = true;
    }

    public void StartLapTimer()
    {
       CanvasController.instance.EnableDisableTimerUI(true);
       isLapTimerActive = true;
    }

    public void EndLapTimer()
    {
        isLapTimerActive = false;
        LastLapTimer = lapTimerMins + lapTimerSecs + lapTimerMilli;
        lapTimerMilli = 0;
        lapTimerSecs = 0;
        lapTimerMins = 0;
    }
    #endregion

    #region Delivery Related Functions
   public void TryStartDelivery() 
   {
        if (Gamemode == gamemode.Delivery ||
          gameState == GameState.Paused || gameState == GameState.Cutscene)
        {
            Debug.LogError("Attempted to start delivery while " + Gamemode + "and " + gameState+ 
                           "\n This is not allowed.");
            return; 
        }

        EnableDisableMapTriggers(false);

        if (DialogueHolder.instance != null)
        { 
            Dialouge PreDeliveryDialouge = 
                DialogueHolder.instance.ReturnDeliveryDialouge(player.ReturnDelisCompleted());

            if (PreDeliveryDialouge != null)
            {

                DialogueManager.instance.StartDialouge(PreDeliveryDialouge);
                //in case player calls this multiple times
                DialogueManager.OnDialogueFinished -= StartDelivery;
                DialogueManager.OnDialogueFinished += StartDelivery;
            }
            else
            {
                Debug.LogError("PreDelivery dialouge is null.");
                StartDelivery();
            }
        
        }
        else 
        { 
            StartDelivery();
        }
   }

    

    public void StartDelivery() 
    {
       Debug.LogWarning("starting delivery");

        //unsubscribe in case called after dialouge
       DialogueManager.OnDialogueFinished -= StartDelivery;

       if (Gamemode == gamemode.Delivery ||
            gameState == GameState.Paused || gameState == GameState.Cutscene){ 
                Debug.LogError("delivery cannot be started because gameState is: " + Gamemode + " and " + gameState);
                return;}
        
        Gamemode = gamemode.Delivery;
        StartCountDown();
       
        timeToDeliver = base_time_to_deliver;
        currentNoPizzas = pizzasToDeliver;
        
        SpawnDeliveryPoint();
        SpawnPointerArrow(ArrowType.Delivery);
        EnableDisableMapTriggers(false);
      

        CanvasController.instance.ToggleDeliveryUi(true);
        CanvasController.instance.UpdateNumberOfPizzas(currentNoPizzas);
        CanvasController.instance.UpdateNotificationText("Deliver the pizza before the it's temperature reaches 25�C!");
    }
    
    private void SpawnDeliveryPoint() 
    {
        int houseNum = UnityEngine.Random.Range(0, Houses.Count);

        //This is for the pizza throw particle effect
        Transform PointToThrowPizzaTo = Houses[houseNum].GetChild(0);

        if (houseNum != lastHouse)
        {
            delivery_point.transform.position = Houses[houseNum].position;
            lastHouse = houseNum;

            if (PointToThrowPizzaTo != null)
            {
                ParticleEffectsControl.instance.GivePizzaToThrowToPos(PointToThrowPizzaTo);
            }
            else
            {
                Debug.LogError("PTTT IS NULL");
            }

            delivery_point.SetActive(true);
            //isDeliveryPointActive = true;

            timeToDeliver += timeToAdd;
        
        }
        else 
        {
            SpawnDeliveryPoint();
        }
    }
    public void EndCurrentDelivery() 
    {
        //makes sure to only trigger when gamemode is delivery
        if (Gamemode != gamemode.Delivery) return;

        delivery_point.SetActive(false);

        if (currentNoPizzas > 0)
        {
            currentNoPizzas--;
        }

        ParticleEffectsControl.instance.ThrowPizzaOutCar(carController.gameObject);
        CanvasController.instance.UpdateNumberOfPizzas(currentNoPizzas);

        if (currentNoPizzas > 0 && timeToDeliver > 0) 
        { 
         SpawnDeliveryPoint();
        }
        else if(currentNoPizzas <= 0)
        {
            if (QuestManager.isQuestActive)
            {
                QuestManager.OnObjectiveCompleted?.Invoke(Objective.ObjectiveType.Delivery);
                EndDelivery();
            }
            else
            {
                FinishedDelivery();
            }

        }
    }

    private void FinishedDelivery() 
    {
        Gamemode = gamemode.FinishedDelivery;
        
        DestroyPointerArrow();

        delivery_trigger_point.SetActive(true);

        SpawnPointerArrow(ArrowType.Objective, delivery_trigger_point);

       
        CanvasController.instance.UpdateQuestObjectiveText("Drive back to the pizzeria.");

        CanvasController.instance.ShowObjectiveText(true);
    }

    public void EndDelivery(bool failed = false) 
    {
        Gamemode = gamemode.Freemode;

        DestroyPointerArrow();
        
        CanvasController.instance.ToggleDeliveryUi(false);

        EnableDisableMapTriggers(true);

        if (!failed)
        {
            player.AddToDelisComplete();
            OnDeliveryCompleted?.Invoke();
        }
        
        if (QuestManager.isQuestActive == false)
        {
            GrantRewardsForDelivery();
        }
    }

    private void GrantRewardsForDelivery() 
    {
        float timeFactor = Mathf.Clamp01(1f - (timeItTookToDeliver / base_time_to_deliver));
        float bonus = maxBonus * timeFactor;

        float payout = basePayout + bonus;

        player.GiveMoney(payout);

        CanvasController.instance.UpdateQuestOverText("Delivery Complete!", false);
        CanvasController.instance.UpdateQuestOverSubText($"+$ {payout:F0} Pay + ${bonus:F2} Tips");
        CanvasController.instance.UpdateQuestOverTimerBar(0,1);
        CanvasController.instance.CallQuestOverFade();


        timeItTookToDeliver = 0;
        //increase num of deliveries and base time
        //tweak these values in the future after teaching
        if (player.ReturnDelisCompleted() < 10) return;
        AssignPizzasToDeliver(pizzasToDeliver + 2);
        base_time_to_deliver += 10f;


        //enable quest trigger
        if(player.ReturnMoney() >= QuestPrerequisite.DreamCarCost) 
        {
          QuestPrerequisite.EnableQuest2();    
        }
        
    }

    private void Defeat() 
    {
        if (QuestManager.isQuestActive)
        {
            QuestManager.OnObjectiveFailed?.Invoke("You failed to deliver pizza in time.");
        }
        else 
        {
            EnableDisableMapTriggers(true);
            CanvasController.instance.UpdateQuestOverText("Delivery Failed!", true);
            CanvasController.instance.UpdateQuestOverSubText("You failed to deliver the pizza in time!!\n" +
                                                             " No tips for you!");
            Gamemode = gamemode.Freemode;
        }

        if(delivery_point != null) 
        { 
            delivery_point.SetActive(false);
        }
        else 
        {
            Debug.LogError("Delivery point object is null.");
        }

        DestroyPointerArrow();
      
        CanvasController.instance.ToggleDeliveryUi(false);
    }
    
    public void EnableDisableMapTriggers(bool enableAll  ,bool enableDelivery = false, bool enableGarage = false, 
        bool enableFuel = false) 
    {
        if (enableAll) 
        {
            delivery_trigger_point.SetActive(enableAll);
            garage_trigger_point.SetActive(enableAll);

            if (fuel_triggers.Count <= 0) return;
            foreach (GameObject g in fuel_triggers)
            {
                g.SetActive(enableAll);
            }

            return;
        }
        
        delivery_trigger_point.SetActive(enableDelivery);
        garage_trigger_point.SetActive(enableGarage);

        if (fuel_triggers.Count <= 0) return; 
        foreach (GameObject g in fuel_triggers)
        {
            g.SetActive(enableFuel);
        }
    }

    public void EnableFuelTriggerOnLowFuel() 
    {
        if (QuestManager.isQuestActive) 
        { 
         EnableDisableMapTriggers(false, enableFuel: true);
        }
    }

    /// <summary>
    /// Use this to check if the current gamemode is delivery
    /// </summary>
    /// <returns>Returns true if the gamemode is delivery</returns>
    public bool ReturnIsDelivery() 
    {
        return Gamemode == gamemode.Delivery;
    }

    #region Delivery/Objective Arrow
    public void SpawnPointerArrow(ArrowType type, GameObject ObjectivePoint = null) 
    {
        if (doesDeliveryArrowExist || delivery_arrow_prefab == null) 
        {
            Debug.LogError("Delivery arrow already exists or prefab is not referenced!");
            return;
        }

        if(carController == null)
        {
            Debug.LogError("Car controller does not exist or is not assigned.");
        }
       
        deliveryArrowIns = Instantiate(delivery_arrow_prefab, carController.transform);
        deliveryArrowIns.transform.position = new Vector3(carController.transform.position.x, carController.transform.position.y + 3, carController.transform.position.z);
        
        doesDeliveryArrowExist = true;
       
        arrowType = type;

        objective_point = ObjectivePoint;
    }

    //break exits the current case, return exits entire method
    void PointToDeliveryPoint() 
    {
     switch(arrowType)
     {
        case ArrowType.Delivery:
        
         deliveryArrowIns.transform.LookAt(delivery_point.transform);
         break;
       
        case ArrowType.Objective:
             if(objective_point == null)
             {
                Debug.LogError("objective point is null!");
                return;
             }
              deliveryArrowIns.transform.LookAt(objective_point.transform);
              break;
     }
      
    }
    
    //Destroy so it can be spawned again
    public void DestroyPointerArrow() 
    {
        if(deliveryArrowIns == null)
        {
          doesDeliveryArrowExist = false;
          return;
        }
        doesDeliveryArrowExist = false;
        Destroy(deliveryArrowIns);
    }
    #endregion

    #endregion

   

    #region Car Select
    //Assigns necessary variables to a car just spawned
    public void AssignSpawnedCarVariables(GameObject car) 
    {
        try
        {
            carController = car.GetComponent<CarController>();
        
            carController.SetPlayer(player); 
        }
        catch (Exception e)
        {
           Debug.Log($"An error occured {e.Message}");
        }

        CameraManager.instance.PassRefsToCinemachine(carController.ReturnCameraLookAtTrans());

        CarController.LowOnFuelEvent += EnableFuelTriggerOnLowFuel;
        carController.OutOfFuelEvent += CarOutOfFuel;
    }

    //Activates Car Selection UI
    public void ToggleCarSelect(bool active) 
    {
        if(gameState == GameState.Cutscene) return;

        ModifyGameState( active? GameState.CarSelect : GameState.Playing);

        if ( active && carController != null)
        {
            currentFuelLevel = carController.currentFuel;
            ForceDestroyCurCar();
        }

        lightingManager.ToggleDirectionalLight(!active);
        
        CameraManager.instance.ToggleMainCamera(!active);
        
        ToggleFreeLookCamera(false);

        CanvasController.instance.ToggleCursor(active);
        
        CanvasController.instance.EnableDisableDebugUi(false);

        
        
        //only call on active as we destroy car, it tries to unmute audio
        //source that doesnt exist anymore on unpause
        if (active)
        {
            SoundManager.instance.ToggleMuteAudioForPause(true);
        }

        SoundManager.instance.ToggleAmbientSounds(!active);
        
        CanvasController.instance.EnableDisableCarSelectionUI(active);
        CanvasController.instance.EnableDisableGameplayUi(!active);
        
        car_select.SetActive(active);
        
        //to get rid of annoying 2 audio listener message
        car_select_cam.SetActive(active);
        car_select_cam.GetComponent<AudioListener>().enabled = active;
      
    }

    public void ToggleFreeLookCamera(bool active, bool cutSceneMode = false, Vector3 pos = default, 
        Quaternion rot = default, float fieldOfView = 60)
    {
        FreeLookCameraMovement freeLookCamScript =  FreeLookCam.GetComponent<FreeLookCameraMovement>();
        
        //Set field of view
        FreeLookCam.fieldOfView = fieldOfView;
        
        if (pos != default)
        {
            freeLookCamScript.transform.position = pos;
        }

        if (rot != default) 
        {
            freeLookCamScript.transform.rotation = rot;
        }


        if (!cutSceneMode)
        {
            if (carController == null) return;
            

            if (active)
            {
                car_cam.gameObject.SetActive(false);
                carController.isEnabled = false;
                FreeLookCam.gameObject.SetActive(true);
                freeLookCamOn = true;

                //enable camera movement
                if(freeLookCamScript != null)
                {
                    freeLookCamScript.ToggleMovement(true);
                }
            }
            else
            {
                car_cam.gameObject.SetActive(true);
                carController.isEnabled = true;
                FreeLookCam.gameObject.SetActive(false);
                freeLookCamOn = false;
            }

            CanvasController.instance.ToggleFreeLookCamUi(active);
            CanvasController.instance.EnableDisableGameplayUi(!active);
        }
        else 
        {
            if (active)
            {
                if(carController != null) 
                {
                    car_cam.gameObject.SetActive(false);
                    carController.isEnabled = false;
                    CanvasController.instance.EnableDisableGameplayUi(!active);
                }

                FreeLookCam.gameObject.SetActive(true);
                freeLookCamOn = true;
                
                //disable freelook cam movement for cutscene
                if(freeLookCamScript != null)
                {
                    freeLookCamScript.ToggleMovement(false);
                }
            }
            else
            {
                if(carController != null) 
                {
                    car_cam.gameObject.SetActive(true);
                    carController.isEnabled = true;
                    CanvasController.instance.EnableDisableGameplayUi(!active);
                }

                FreeLookCam.gameObject.SetActive(false);
                freeLookCamOn = false;
            }

           
            CanvasController.instance.ToggleFreeLookCamUi(false);
          
        }
    
    }
     
    #endregion

    #region Assign Values
    //Idea is that quests can use this method to assign num of pizzas to be delivered
    //if they desire
    public void AssignPizzasToDeliver(int pizzas)
    {
        pizzasToDeliver = pizzas;
    }

    //assigns current quest so certain values such as reach point
    //can access the script
    public void AssignQuestManager(QuestManager questManager) 
    {
        currentQuest = questManager;
    }
    #endregion

    #region Trigger Events
    private void TriggerSetValueEvent(Objective.VariableToSet toSet, float value) 
    {
        switch (toSet) 
        { 
            case Objective.VariableToSet.Fuel:
                if (carController != null) 
                {
                    carController.SetFuelByDenomination((int)value);
                }
                break;
        }
    }
    #endregion

    #region Return Value Methods
    public bool ReturnJoystickXStatus()
    {
        return isJoystickDpadXEnabled;
    }
    public bool ReturnJoystickYStatus()
    {
        return isJoystickDpadYEnabled;
    }
    public FuelMode ReturnFuelMode() 
    {
        return fuelMode;
    }

    public Vector3 ReturnCarPosition() 
    { 
        if(carController == null) 
        {
            return Vector3.zero;
        } 
        
        return carController.transform.position;
    }

    public CarController AccessCarController()
    {
        if(carController != null)
        {
            return carController;
        }
        else
        {
            return null;
        }
    }
    
    public PlayerManager ReturnPlayerManager() 
    {
        //if multiple player managers add arg to choose correct
        return player;
    }

    public float ReturnFuel()
    {
        return currentFuelLevel;
    }
    
    public int ReturnTransmissionTypeLoaded()
    {
       return this.TransmissionTypeIndex;
    }
    #endregion

     public void SaveGameData(ref GameData gameData)
    {
       
    }

    public void LoadGameData(GameData gameData)
    {
      
    }

    public void SaveSettingsData(ref SettingsData settingsData)
    {
       
    }

    public void LoadSettingsData(SettingsData settingsData)
    {
        this.TransmissionTypeIndex = settingsData.TransmissionTypeIndex;
        print($"in game, manager, loading{TransmissionTypeIndex}");
    }
}

