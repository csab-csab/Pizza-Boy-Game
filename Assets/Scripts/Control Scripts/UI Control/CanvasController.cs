using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.Profiling;
using System;
using UnityEngine.VFX;
using System.Diagnostics.Contracts;


public class CanvasController : MonoBehaviour
{
    public static CanvasController instance;

    #region Scriptable Obj Ref
    //public So it can be accessed through singleton
    public TextPresets textPresets;
    #endregion
    #region Car Selection UI
    [Header("Car Selection UI")]
    [SerializeField] GameObject CarSelectionUiPanel;
    [SerializeField] GameObject CarViewChoicePanel;
    [SerializeField] GameObject CarViewPanel;
    [Header("Car Selection UI Buttons")]
    [SerializeField] Button SelectLockedCarsButton;
    [SerializeField] Button SelectUnlockedCarsButton;
    [SerializeField] Button SpawnCarButton;
    [SerializeField] Button ChangeColourButton;
    [SerializeField] Button ReturnButton;

    #endregion
    #region UI
    [Header("UI")]
    [SerializeField] GameObject GameplayUi;
    [SerializeField] GameObject CarDestroyedUi;
    [SerializeField] GameObject CarOutOfFuelUi;
    [SerializeField] GameObject TimerUi;
    [SerializeField] TMP_Text TimeText;
    [SerializeField] TMP_Text NotificationText;
    [SerializeField] TMP_Text InteractText;
    [SerializeField] TMP_Text MoneyText;
    [SerializeField] GameObject SavingsText;
    [SerializeField] GameObject FreeLookUi;
    [SerializeField] GameObject SettingsUi;


    [Header("Car UI")]
    [SerializeField] TMP_Text CountDownTimer;
    [SerializeField] TMP_Text SpeedText;
    [SerializeField] TMP_Text GearsText;
    [SerializeField] GameObject RPMNeedle;
    [SerializeField] float NeedleStart = 117.62f, NeedleStop = -135.4f;

    [SerializeField] GameObject FuelNeedle;
    [SerializeField] float FuelNeedleStart = -65f, FuelNeedleStop = 59.7f;
    [SerializeField] Image fuelIcon;
    [SerializeField]bool isLowOnFuel;
    [SerializeField]float flashesPerSec = 2f;

    [Header("Radio UI")]
    [SerializeField] Image RadioGraphic;
    [Header("Ensure 0 is for radio off graphic")]
    [SerializeField] List<Sprite> RadioSprites;
    [SerializeField] TMP_Text RadioText;

    [Space(10)]
    [Header("Debug UI")]
    [SerializeField] GameObject StartGameUi;
    [SerializeField] GameObject StartGameUiBase;
    [SerializeField] GameObject SelectCarStartGameUi;
    [SerializeField] GameObject DebugPanel;
    [SerializeField] TMP_Text RpmDisplayText;
    [SerializeField] TMP_Text TorqueDisplayText;
    [SerializeField] TMP_Text WheelRpmDisplayText;
    [SerializeField] TMP_Text CurrentTimeScaleText;

    [Header("Track UI")]
    [SerializeField] GameObject TrackUI;
    [SerializeField] TMP_Text Timer;
    [Header("Delivery UI")]
    [SerializeField] GameObject DeliveryUI;
    [SerializeField] TMP_Text DeliveryTimer;
    [SerializeField] TMP_Text PizzasToDelivery;

    [Header("Fuel UI")]
    [SerializeField] GameObject RefuelUi;
    [SerializeField] Button RefuelButton;
    [SerializeField] Image RefuelBar;
    [SerializeField] TMP_Text RefuelMoneyText;
    [SerializeField] TMP_Text RefuelPriceText;
    [SerializeField] TMP_Text PressAnyKeyRescueFuel;

    [Header("Dialouge UI")]
    [SerializeField] GameObject DialougePanel;
    [SerializeField] TMP_Text SpeakerName;
    [SerializeField] TMP_Text DialogueText;

    [Header("Cutscene UI")]
    [SerializeField] GameObject CutscenePanel;
    [SerializeField] GameObject Cutscene1UI;

    //These variables are used for the black screen fade in/out effect for the camera when 
    // cutscene/dialouge is over
    [SerializeField] Animator Cutscene_EndAnimator;
    [SerializeField] Image Cutscene_EndEffect;

    private const string CUTSCENE_End_FADE_IN = "Cutscene and Dialouge End Fade IN";

    [Header("Quest Ui")]
    public TMP_Text QuestTitle;
    private float timeUntilQuestTitleFade;
    [SerializeField] float def_timeUntilQuestTitleFade = 5f;
    
    [SerializeField] TMP_Text ObjectiveText;
    
    [SerializeField]private float ObjetiveTextLifeTime;
    [SerializeField] float def_ObjetiveTextLifeTime = 5f;
    public TMP_Text QuestOverText;
    public TMP_Text QuestOverSubText;
    private float timeUntilQuestOverTextFade;
    private float def_timeUntilQuestOverTextFade = 5f;

    [SerializeField] Image QuestOverTimerBar;

    [SerializeField] Color QuestCompleteColor;

    [SerializeField] Color QuestFailColor;


    [Header("Scale in values")]
    [SerializeField] float scaleInFactor = 20f;

    //Gameplay Ui
    [SerializeField] bool scaleInGameplayUi;

    //Car Select Ui
    [SerializeField] bool scaleInCarSelectMenu;

    [SerializeField] bool scaleInCarView;

    //Refuel Ui
    [SerializeField] bool scaleInRefuelUi;

    //Radio Ui
    [SerializeField] bool scaleInRadioUi;
    [SerializeField] bool scaleOutRadioUi;

    //Dialogue Ui
    [SerializeField] bool scaleInDialogue;
    [SerializeField] bool scaleOutDialogue;

    //Quest Ui
    [SerializeField] bool scaleInQuestTitle;
    [SerializeField] bool scaleOutQuestTitle;

    [SerializeField] bool scaleInQuestOver;
    [SerializeField] bool scaleOutQuestOver;
    //this is a work around so it can be passed into scale in method when a scale out bool isnt required
    private bool emptyBool;



    //default value for the amount of time after which the radio graphic gets hidden
    float def_timeUntilRadioFade = 5f;
    
    float timeUntilRadioFade = 0f;
    #endregion

    #region Variables
    //this is used as a base variable
    [SerializeField] private float NotificationShowTime = 5f;
    //this gets updated during execution 
    private float _notificationShowTime;
    
    [SerializeField] private float NotifcationShowScale = 10f;

    private bool freeLookHUDon;
    //this variable is used exculsively to check whether to reenable gameplay ui after dialouge
    private bool wasGamePlayUiOn;
    private bool startDeliverTextCleared;
    #endregion

    float GoTimer;
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
        NotificationText.text = "";
        SetupButtonListeners();
        ToggleCarDestroyedUi(false);

        //Subscribe to events
        QuestManager.OnQuestStarted += CallQuestTitleFade;
        QuestManager.OnQuestCompleted += CallQuestOverFade;
        QuestManager.OnQuestFailed += CallQuestOverFade;
        CarController.LowOnFuelEvent += TriggerLowFuelFlash;
        CarController.HighOnFuelEvent += StopLowFuelFlash;

        ToggleStartGameUi(true);
    }
   
    private void Update()
    {
      #region Go Timer
      //Shows the GO text after countdown is over for two seconds
      if (GoTimer > 0) 
      { 
          GoTimer -= Time.deltaTime;
      }
      else if(GoTimer <= 0) 
      {
          CountDownTimer.text = " ";
      }
        #endregion

        #region Low Fuel Flash
        if (isLowOnFuel) 
        {
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * flashesPerSec));
            Color c = fuelIcon.color;
            c.a = alpha;
            fuelIcon.color = c;
        }
        #endregion

        #region Fade Notification
        //this is used to fade the notifcation text once the timer is 0
        if (_notificationShowTime > 0) 
        { 
         _notificationShowTime -= Time.unscaledDeltaTime;
        }
        else if (NotificationText.fontSize > 0) 
        {
          NotificationText.fontSize -= Time.unscaledDeltaTime * NotifcationShowScale;

            //this is to make sure the text fully dissapears and doesnt flip
            if(NotificationText.fontSize < 0) 
            { 
             NotificationText.fontSize = 0;
            }
        }
        #endregion

      #region Call Scale in effect
        CheckScaleInBools();
       
        #endregion
    
      #region Scale out effect
        if(timeUntilRadioFade > 0) 
        { 
            timeUntilRadioFade -= Time.deltaTime;
        }

        //scales out radio graphic after time is up
        //used instead of a scale out method because it ran into coflicts with the scale in
        //method and caused weird glitches
        //this works 
        if(timeUntilRadioFade <= 0 && RadioGraphic.transform.localScale.y > 0) 
        {
           RadioGraphic.transform.localScale = new Vector3(1, Mathf.Lerp(RadioGraphic.transform.localScale.y, 0f, scaleInFactor * Time.deltaTime), 1);
        }

        if (scaleOutDialogue && DialougePanel.transform.localScale.y > 0)
        {
            //Move towards is so much better than lerp for ui cases like this
            float newY = Mathf.MoveTowards(DialougePanel.transform.localScale.y, 0f, scaleInFactor * Time.deltaTime);

            DialougePanel.transform.localScale = new Vector3(1, newY, 1);
        }
        else if(scaleOutDialogue && DialougePanel.transform.localScale.y <= 0.01f) 
        {
            scaleOutDialogue = false;
            DialougePanel.transform.localScale = new Vector3(1, 0, 1);
            DialougePanel.SetActive(false);
        }
        #region scale in/out effect for quest title
        //scale in/out quest title
        if(scaleOutQuestTitle == true && timeUntilQuestTitleFade > 0)
        {
            timeUntilQuestTitleFade -= Time.deltaTime;
        }
        else if(scaleOutQuestTitle == true && timeUntilQuestTitleFade <= 0 &&  
        QuestTitle.transform.localScale.y >= 0.01f)
        {
           QuestTitle.transform.localScale = new Vector3(1, Mathf.Lerp(QuestTitle.transform.localScale.y, 0f, scaleInFactor/2 * Time.deltaTime), 1);
          
        }
        else if(scaleOutQuestTitle == true && timeUntilQuestTitleFade <= 0 && QuestTitle.transform.localScale.y <= 0.01f )
        {
            scaleOutQuestTitle = false;
            QuestTitle.transform.localScale = new Vector3(1,0,1);
            ShowObjectiveText(true);
        }
        #endregion
        
        //scale in/out effect for quest over text
        if(scaleOutQuestOver == true && timeUntilQuestOverTextFade > 0)
        {
            timeUntilQuestOverTextFade -= Time.deltaTime;
        }
        else if(scaleOutQuestOver == true && timeUntilQuestOverTextFade <= 0 &&  
        QuestOverText.transform.localScale.y >= 0.01f)
        {
           QuestOverText.transform.localScale = new Vector3(1, Mathf.Lerp(QuestTitle.transform.localScale.y, 0f, scaleInFactor/2 * Time.deltaTime), 1);
           QuestOverSubText.transform.localScale = new Vector3(1, Mathf.Lerp(QuestTitle.transform.localScale.y, 0f, scaleInFactor/2 * Time.deltaTime), 1);
        }
        else if(scaleOutQuestOver == true && timeUntilQuestOverTextFade <= 0 
        && QuestOverText.transform.localScale.y <= 0.01f )
        {
            scaleOutQuestOver = false;
            QuestOverText.transform.localScale = new Vector3(1,0,1);
            QuestOverSubText.transform.localScale = new Vector3(1,0,1);
        }


        if(ObjetiveTextLifeTime > 0)
        {
            ObjetiveTextLifeTime -= Time.deltaTime;
        }
        else if(ObjetiveTextLifeTime <= 0 && ObjectiveText.gameObject.activeSelf)
        {
            ShowObjectiveText(false);
        }
        #endregion
    }
   


    //Adds required methods to the OnClick event 
    private void SetupButtonListeners()
    {
        if(SpawnCarButton == null) Debug.LogError("Spawn car button is null, assign it in the inspector.");
        
        SpawnCarButton.GetComponent<Button>().onClick.RemoveAllListeners();
        SpawnCarButton.GetComponent<Button>().onClick.AddListener(CarSelectorScript.instance.SelectCar);
    }
    
    #region Toggle UI Elements
    
    public void ToggleStartGameUi(bool enabled) 
    {
        StartGameUi.SetActive(enabled);
    }

    public void ToggleCarSelectStartGame(bool enabled) 
    { 
        SelectCarStartGameUi.SetActive(enabled);
        StartGameUiBase.SetActive(!enabled);
    }
    
    public void EnableUi(bool trackUi, bool deliveryUi) 
    {
       TrackUI.SetActive(trackUi);
       DeliveryUI.SetActive(deliveryUi);
    }

    public void EnableDisableDebugUi(bool active) 
    { 
      DebugPanel.SetActive(active);
    }
    
    public void EnableDisableGameplayUi(bool active) 
    {
        if (active == GameplayUi.activeInHierarchy) return;

        GameplayUi.SetActive(active);

        SelectButtonToStartNavigation(null);
        
        if (active)
        {
            //so scale in can you know scale in
            GameplayUi.transform.localScale = new Vector3(1, 0, 1);
            scaleInGameplayUi = true;
        }

    }
    
    public void EnableDisableCarSelectionUI(bool active)
    {

        CarSelectionUiPanel.SetActive(active);
        CarViewChoicePanel.SetActive(active);
        CarViewPanel.SetActive(!active);
        SelectButtonToStartNavigation(SelectLockedCarsButton.gameObject);
        
        if (active)
        {
            //so scale in can you know scale in
            CarSelectionUiPanel.transform.localScale = new Vector3(1, 0, 1);
            scaleInCarSelectMenu = true;
        }
    }

    public void EnableDisableCarViewUi(bool active) 
    {
        CarViewPanel.SetActive(active);
        CarViewChoicePanel.SetActive(!active);
        

        //Selects UI Button so it can be navigated
        if (active) 
        {
            
            SelectButtonToStartNavigation(null);
            //so scale in can you know scale in
            CarViewPanel.transform.localScale = new Vector3(1, 0, 1);
            scaleInCarView = true;
        }
    }

    public void EnableDisableTimerUI(bool isEnabled)
    {
        TimerUi.SetActive(isEnabled);
    }

    //THIS FOR USE WITH THE BUTTON ONLY
    public void DisableRefuelUi() 
    {
        ToggleRefuelUi(false);
        
        GameManager.instance.SetRefuelingStatus(GameManager.FuelMode.Normal);
        GameManager.instance.TurnOffRefuelInCarController();

        CameraManager.instance.SwitchCameraMode(CameraManager.CameraView.Normal);
      
    }
    
    public void ToggleRefuelUi(bool enabled, CarController carController = null) 
    {
        if (enabled == RefuelUi.activeSelf) return;
        
      RefuelUi.SetActive(enabled);
      EnableDisableGameplayUi(!enabled);

        if (carController != null)
        {
            RefuelButton.onClick.AddListener(() => carController.GiveFuel(GameManager.instance.fuelUnitMultiplier, true));
        }

        if (enabled)
        {
            //so scale in can you know scale in
            RefuelUi.transform.localScale = new Vector3(1, 0, 1);
            scaleInRefuelUi = true;
        }
        
    }


    public void ToggleCarDestroyedUi(bool enabled) 
    { 
        CarDestroyedUi.SetActive(enabled);
    }

    public void ToggleDialougeUi(bool enabled, bool gamePaused = false) 
    {
        if (enabled)
        {
            DialougePanel.SetActive(true);
            
            if (ReturnGamePlayUiOn()) 
            {
                wasGamePlayUiOn = true;
                EnableDisableGameplayUi(false);
            }
            
           
            scaleInDialogue = true;
        }
        else 
        {
            //game paused variable is needed because when the game is paused
            //time.delta is 0 so ui cant be scaled out using that
            if (gamePaused) 
            { 
                DialougePanel.SetActive(false);
                DialougePanel.transform.localScale = new Vector3(1, 0, 1);
                return;
            }
            
            scaleOutDialogue = true;
            
            if (wasGamePlayUiOn)
            {
                wasGamePlayUiOn = false;
                EnableDisableGameplayUi(true);
            }
        }
    }

    public void ToggleSavingsText(bool on)
    {
        if (on)
        {
            SavingsText.SetActive(true);
        } 
        else
        {
            SavingsText.SetActive(false);
        }
    }

    private void CallQuestTitleFade()
    {
        scaleInQuestTitle = true;
        scaleOutQuestTitle = false;
        timeUntilQuestTitleFade = def_timeUntilQuestTitleFade;
        ShowObjectiveText(false);
    }

    //public cuz it needs to be used by the GameManager Delivery Too
     public void CallQuestOverFade()
    {
        scaleInQuestOver = true;
        scaleOutQuestOver = false;
        timeUntilQuestOverTextFade = def_timeUntilQuestOverTextFade;
    }

    /// <summary>
    /// Disables it self
    /// </summary>
    /// <param name="show"></param>
    public void ShowObjectiveText(bool show)
    {
        ObjectiveText.gameObject.SetActive(show);
        
        if(show)
        {
            ObjetiveTextLifeTime = def_ObjetiveTextLifeTime;
        }
    }

    public void ToggleFreeLookCamUi(bool enabled) 
    { 
        FreeLookUi.SetActive(enabled);
        freeLookHUDon = enabled;
    }

    private void ToggleCutscenePanel(bool enabled)
    {
        CutscenePanel.SetActive(enabled);
    }

    public void ToggleCutscene1UI(bool enabled)
    {
        ToggleCutscenePanel(enabled);
        Cutscene1UI.SetActive(enabled);
    }

    public void ToggleCarOutOfFuelUi(bool enabled, float rescueCost = 0)
    {
        CarOutOfFuelUi.SetActive(enabled);
        PressAnyKeyRescueFuel.text = $"Press any key to pay {rescueCost} for recovery costs.";
        
        //Sets it y scale to 0 so the anim can fade it in smoothly
        if (enabled)
        {
            CarOutOfFuelUi.transform.localScale = new Vector3(1, 0, 1);
        }
    }

    public void ToggleSettingsUi(bool enabled)
    {
        SettingsUi.SetActive(enabled);
    }
    #endregion

    #region Update UI Elements 

    #region Update Timers UI
    public void UpdateTimerUI(int millisecs, int secs, int mins) 
    {
       Timer.text = mins + "." + secs + "." + millisecs;
    }

    public void UpdatePizzaTemperatureUI(float temperature) 
    {
       DeliveryTimer.text = temperature.ToString("F2") + "°C";
    }
   
    public void UpdateCountDownUi(int time)
    {

        CountDownTimer.text = time.ToString();
        if (time <= 0)
        {
            GoTimer = 2f;
            CountDownTimer.text = "GO!";
        }
    }

    public void IntialiseQuestOverTimerBar(bool failed) 
    {
        if (failed) 
        {
            QuestOverTimerBar.color = QuestFailColor;
        }
        else 
        {
            QuestOverTimerBar.color = QuestCompleteColor;
        }
    }

    //The timer bar at the end of quest indicating how long until restart/ui dismissal
    public void UpdateQuestOverTimerBar(float currentTime, float maxTime) 
    { 
       QuestOverTimerBar.fillAmount = currentTime/maxTime; 
    }
    #endregion

    #region Update Gameplay Information UI
   
    public void UpdateQuestTitleText(string text)
    {
        QuestTitle.text = text;
    }
    
    public void UpdateQuestObjectiveText(string text)
    {
        ObjectiveText.text = text;
    }

    public void UpdateQuestOverText(string text, bool failed)
    {
        if (failed)
        {
            QuestOverText.color = QuestFailColor;
        }
        else
        {
            QuestOverText.color = QuestCompleteColor;    
        }

        QuestOverText.text = text;
    }

    public void UpdateQuestOverSubText(string text)
    {
        QuestOverSubText.text = text;
    }

    /// <summary>
    /// Overwrites current fade progress, making the text dissapear now
    /// Used for when the player uses a button to restart
    /// </summary>
    public void FadeQuestOverTextNow() 
    {
        UpdateQuestOverTimerBar(0,0);
        timeUntilQuestOverTextFade = 0;
        scaleOutQuestOver = true; 
    }

    /// <summary>
    /// Make sure to not call this method in update as it will not fade
    /// </summary>
    /// <param name="text"></param>
    public void UpdateNotificationText(string text) 
    { 
      NotificationText.text = text;
      NotificationText.fontSize = 50f;
      _notificationShowTime = NotificationShowTime;
    }

    public void UpdateInteractUiText(string text) 
    {
        InteractText.text = "\n" + text;
    }

    public void ClearInteractUiText()
    {
        InteractText.text = " "; 
    }

    public void UpdateNumberOfPizzas(int pizza) 
    { 
       PizzasToDelivery.text = "Pizza to Deliver: " + pizza.ToString();
    }

    public void DisplayTime(float hours, float minutes) 
    {
       int mins = Convert.ToInt32(minutes);

        string formattedHours = hours < 10 ? $"0{hours}" : hours.ToString();
        string formattedMinutes = minutes < 10 ? $"0{mins}" : mins.ToString();

        TimeText.text = $"{formattedHours}:{formattedMinutes}";
    }

    public void UpdateRefuelBar(float currentFuel, float maxFuel) 
    {
        RefuelBar.fillAmount =  currentFuel / maxFuel;
    }

    public void UpdateRadioStationUi(int currentRadio , string trackName) 
    {
        //convert radio value to list value
        //eg: when off (-1) becomes 0 so correct sprite can be shown
        currentRadio++;

        //for visual effect so it looks cool
        RadioGraphic.transform.localScale = new Vector3(1, 0, 0);

        RadioGraphic.sprite = RadioSprites[currentRadio];
        RadioText.text = trackName;
        
        scaleInRadioUi = true;
        timeUntilRadioFade = def_timeUntilRadioFade;
    }

    public void UpdateCarRefuelUi(float money, float price) 
    { 
        RefuelMoneyText.text = "Your money:$" +  money.ToString("F2");
        RefuelPriceText.text = "Fuel price per unit:$ " + price.ToString();
    }

    public void UpdateMoneyText(float money) 
    {
        MoneyText.text = "$" + money.ToString("N2");
    }
    #endregion

    #region Update Vehicle Information UI
    public void UpdateRpmNeedle(float RPM) 
    {
        float desiredPos = NeedleStart - NeedleStop;
    
        float NeedlePos = RPM / 8000;

       RPMNeedle.transform.eulerAngles = new Vector3(0, 0, NeedleStart - NeedlePos * desiredPos);
    }

    public void UpdateFuelNeedle(float currentFuel, float MaxFuel) 
    {
        float desiredPos = FuelNeedleStart - FuelNeedleStop;

        float NeedlePos =  currentFuel/ MaxFuel;

        FuelNeedle.transform.eulerAngles = new Vector3(0, 0, FuelNeedleStart - NeedlePos * desiredPos);
    }

    private void TriggerLowFuelFlash() 
    { 
        isLowOnFuel = true;
    }

    private void StopLowFuelFlash() 
    { 
        isLowOnFuel = false;
        
        //hide low fuel flash
        Color c = fuelIcon.color;
        float alpha = 0;
        c.a = alpha;

        fuelIcon.color = c;
    }

    public void UpdateSpeed(float Speed) 
    { 
      SpeedText.text = Speed.ToString("F0");
    }

 
    public void UpdateGears(int GearNum) 
    {
        GearsText.text = GearNum.ToString();

        if (GearNum  == 0) 
        {
            GearsText.text = "N";
        }
        else if( GearNum == -1) 
        {
            GearsText.text = "R";
        }
    }
    #endregion

    #region Update Car Selection Panel Buttons
    /// <summary>
    /// Updates the buttons to reflect current input device as well as which menu the player is currently in
    /// eg; if controller and in buy menu, the confirm button will say "Buy Car A"
    /// if keyboard it will say enter
    /// </summary>
    /// <param name="isController"></param>
    public void UpdateCarSelectionButtons(bool isController, bool buyMenu, bool isXboxC)
    {
        #region Confirm Button
          string ConfirmButtonText = isController
          /*if controller*/ ? (buyMenu ? textPresets.BuyCarButtonText   : "Select Car ")
          /*else*/ : (buyMenu ? "Buy Car Enter" : "Select Car Enter");
       
          //Adds the correct button based on the type of controller
          ConfirmButtonText += isController ? (isXboxC ? "A" : "X") : "";
        #endregion

        #region Change Colour Button
        string ChangeColourButtonText = isController ? "Change Colour ^" : "Change Colour C";
        #endregion

        #region Return Button
        string ReturnButtonText = isController ? (isXboxC ? "< B" : "< o") : "< esc";
        #endregion
       
        SpawnCarButton.GetComponentInChildren<TMP_Text>().text = ConfirmButtonText;
        ChangeColourButton.GetComponentInChildren<TMP_Text>().text = ChangeColourButtonText;
        ReturnButton.GetComponentInChildren<TMP_Text>().text = ReturnButtonText;
    }

    #endregion

    #region Update Dialogue
    public void DialougeClearSentence() 
    {
        DialogueText.text = "";
    }

    //uses a char for the type out effect for the dialouge
    public void UpdateDialogue(string speakerName, char letter = ' ', string fullSentence = null) 
    { 
        SpeakerName.text = speakerName;

        if (fullSentence != null)
        {
            DialogueText.text = "";
            DialogueText.text = fullSentence;
        }
        else 
        {
            DialogueText.text += letter;
        }
    }
    #endregion

    

    #region Smooth Ui Transition 

    /// <summary>
    /// Checks if any scale in effects need to be called (dont judge i was tired when i wrote this)
    /// </summary>

    //Please actually let me know if there is a better way to do this
    private void CheckScaleInBools() 
    {
        if(scaleInGameplayUi) 
        {
            ScaleInEffect(GameplayUi.transform, ref scaleInGameplayUi, scaleInFactor, ref emptyBool);
        }

        if (scaleInRefuelUi)
        {
            ScaleInEffect(RefuelUi.transform, ref scaleInRefuelUi, scaleInFactor, ref emptyBool);
        }

        if (scaleInCarSelectMenu)
        {
            ScaleInEffect(CarSelectionUiPanel.transform, ref scaleInCarSelectMenu, scaleInFactor, ref emptyBool);
        }

        if (scaleInCarView)
        {
            ScaleInEffect(CarViewPanel.transform, ref scaleInCarView, scaleInFactor, ref emptyBool);
        }

        if (scaleInRadioUi) 
        { 
            ScaleInEffect(RadioGraphic.transform, ref scaleInRadioUi, scaleInFactor, ref emptyBool);
        }

        if (scaleInDialogue) 
        { 
            ScaleInEffect(DialougePanel.transform, ref scaleInDialogue, scaleInFactor, ref emptyBool);
        }
    
        if(scaleInQuestTitle)
        {
            ScaleInEffect(QuestTitle.transform, ref scaleInQuestTitle, scaleInFactor/2, ref scaleOutQuestTitle) ;
        }

        if(scaleInQuestOver)
        {
            ScaleInEffect(QuestOverText.transform, ref scaleInQuestOver, scaleInFactor/2, ref scaleOutQuestOver);
            
        }
    }


    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="valueToReset"> 
    /// This should be passed as a ref so the bool can be reset so it doesnt get called in update</param>    
    private void ScaleInEffect(Transform transform , ref bool valueToReset, float ScaleInFactor, ref bool valueToSet) 
    {
        transform.localScale =  new Vector3(1,Mathf.Lerp(transform.localScale.y, 1f, ScaleInFactor * Time.deltaTime), 1);

        if (transform.localScale.y >= 0.99)
        {
            valueToReset = false;
            valueToSet = true;
        }
    }
    #endregion

    #endregion

    #region Cutscene Effects
   // The fade-in animation transitions automatically to fade-out(transition in anim tree),
    // so I only need to trigger the fade-in animator here.
    public void Play_Cutscene_End_Fade()
    {
       Cutscene_EndAnimator.Play(CUTSCENE_End_FADE_IN);

    }

    public void Call_Dialogue_End_Fade(DialogueManager.DialougeFinished eventOnFinish, Action executeOnFinish = null)
    {
        StartCoroutine(Play_Dialogue_End_Fade(eventOnFinish, executeOnFinish));
    }

    IEnumerator Play_Dialogue_End_Fade(DialogueManager.DialougeFinished eventOnFinish, Action executeOnFinish = null)
    {
        float cutscene_length = Return_Cutscene_End_Fade_Length();
        
        Cutscene_EndAnimator.Play(CUTSCENE_End_FADE_IN);
        yield return new WaitForSecondsRealtime(cutscene_length);
        eventOnFinish?.Invoke();
        
        if (executeOnFinish != null)
        {
            executeOnFinish();
        }
    }

   
   //Return the length of the animations
   //Used to wait for fade before starting new object/Quest etc.
    public float Return_Cutscene_End_Fade_Length()
    {
       AnimationClip[] clips = Cutscene_EndAnimator.runtimeAnimatorController.animationClips;
    
      if(clips.Length <= 0)
      {
        Debug.LogError("No animation clips found");
        return 0;
      }
      else
      {
        return clips[0].length + clips[1].length;
      }
    
    }
    #endregion 

    #region Debug
    public void DisplayCurrentEngineStatistics(float rpm, float maxRpm, float wheelRPM, float engineTorque)
    {
        RpmDisplayText.text = rpm.ToString("f0") + "/" + maxRpm;
        WheelRpmDisplayText.text = "Wheel RPM: " + wheelRPM.ToString("f0");
        TorqueDisplayText.text = "Engine Torque: "+ engineTorque.ToString("f0");
    }
    
    public void DisplayCurrentTimeScale(float timeScale) 
    {
        CurrentTimeScaleText.text = timeScale.ToString();
    }
    #endregion


    #region Cursor
    public void ToggleCursor(bool visible) 
    { 
        Cursor.visible = visible;
        switch(visible)
        {
            case false:
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case true:
                #if UNITY_EDITOR
                    Cursor.lockState = CursorLockMode.None;
                #else
                    Cursor.lockState = CursorLockMode.Confined;
                #endif
                break;
        }
    }
        

    #endregion

    #region Return Values
    public bool ReturnfreeLookHudStat() 
    {
        return freeLookHUDon;
    }

    public bool ReturnGamePlayUiOn ()
    { 
        return GameplayUi.activeInHierarchy;
    }
    
    public bool ReturnRefuelUiOn() 
    { 
        return RefuelUi.activeInHierarchy;
    }

    public bool ReturnDialougeUiOn() 
    { 
        return DialougePanel.transform.localScale.y > 0;
    }
    
    public bool ReturnDeliveryTextCleared()
    {
        return startDeliverTextCleared;
    }

    public void AssignDeliverTextCleared(bool cleared)
    {
        startDeliverTextCleared = cleared;
    }
    #endregion
   
    //Pre selects button to allow navigation of buttons with arrow keys and controller
    private void SelectButtonToStartNavigation(GameObject button) 
    {
      EventSystem.current.SetSelectedGameObject(button);
    }
}