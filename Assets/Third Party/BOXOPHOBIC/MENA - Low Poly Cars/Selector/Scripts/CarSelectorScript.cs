using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class CarSelectorScript : MonoBehaviour
{
    public static CarSelectorScript instance;
    #region Variables
    
    //Events
   //this event is used to spawn the car by other scripts without needing a reference
    public delegate void TriggerSpawnCar(Transform transformToSpawnAt, GameObject car, int fuelDenomination, int TransmissionTypeIndex, string method, float exactFuel);
    public static TriggerSpawnCar triggerSpawnCar; 
    
    [Header("Properties")]
    //[SerializeField] GameObject carSelect;
    [SerializeField] int totalCars;
    [SerializeField] int currentCar = 0;
    [SerializeField] int actualColor = 0;


    [Header("Car Switch Variables")]
    [SerializeField] GameObject CarViewSpawn;
    GameObject TempSelCar;
    [SerializeField] enum CarSelectMenu { Unselected, Owned, Locked };
    [SerializeField] CarSelectMenu carSelectMenu;
    //this value is needed to see if the the enter key can be used to select car
    bool isReadyToConfirm = false;
    //Contains models with all required colliders
    public List<GameObject> carListUnlockedPlayable;
    public List<GameObject> carListLockedPlayable;
    //For display and selection purposes
    public List<GameObject> carListUnlocked;
    public List<GameObject> carListLocked;

    [SerializeField] Transform CarSpawn;

    //this used to spawn the correct car once its destroyed for example
    private GameObject lastSpawnedCar;

    [Space(20)]

    [Header("Visuals")]
    public Animator main_Camera_Animator, camera_Container_Animator, car_Container_Animator;
    public TMP_Text carNameText;
    public Material ColorVar_1, ColorVar_2, ColorVar_3, ColorVar_4, ColorVar_5;
    [SerializeField] GameObject GarageLights;

    [Space(20)]

    [Header("Audio")]
    private AudioSource carSoundSource;
    [SerializeField] List<AudioClip> carSounds;
    [SerializeField] List<AudioClip> unlockedCarSounds;

    [Header("Misc")] 
    //The spawn point for the raiden after quest 3 is complete
    [SerializeField] private Transform raidenSpawnPoint;
    #endregion

    void Start()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        instance = this;

        GarageLights.SetActive(false);
        triggerSpawnCar += SpawnSpecifiedCar;
    }

    private void Update()
    {
        #region Controls 
        #region Controller Car Selection

        #region Comment
        /* Gets the horizontal axis of the dpad and checks if the right or left button is pressed
         1 == right
         0 == neutral
        -1 == left
         and loops thriugh car list depending on the value
         Also disables joystick to prevent continous looping
         */
        #endregion

        if (Input.GetAxisRaw("Joystick Dpad X") == 1 && GameManager.instance.ReturnJoystickXStatus())
        {
            NextCarRight();

            #region Update Car Selection Button Call
            CanvasController.instance.UpdateCarSelectionButtons(InputManager.instance.device
                == InputManager.InputDevice.Controller,
                carSelectMenu == CarSelectMenu.Locked,
                InputManager.instance.controllerType == InputManager.ControllerType.Xbox);
            #endregion
        }
        else if (Input.GetAxisRaw("Joystick Dpad X") == -1 && GameManager.instance.ReturnJoystickXStatus())
        {
            NextCarLeft();

            #region Update Car Selection Button Call
            CanvasController.instance.UpdateCarSelectionButtons(InputManager.instance.device
                == InputManager.InputDevice.Controller,
                carSelectMenu == CarSelectMenu.Locked,
                InputManager.instance.controllerType == InputManager.ControllerType.Xbox);
            #endregion
        }

        //Return out of Car view ui


        #region Change colour
        if (Input.GetAxisRaw("Joystick Dpad Y") == 1 && GameManager.instance.ReturnJoystickYStatus())
        {
            if (actualColor == 4)
            {
                actualColor = 0;
            }
            else
            {
                actualColor++;
            }
            changeColor();

            #region Update Car Selection Button Call
            CanvasController.instance.UpdateCarSelectionButtons(InputManager.instance.device
                == InputManager.InputDevice.Controller,
                carSelectMenu == CarSelectMenu.Locked,
                InputManager.instance.controllerType == InputManager.ControllerType.Xbox);
            #endregion

        }
        else if (Input.GetAxisRaw("Joystick Dpad Y") == -1 && GameManager.instance.ReturnJoystickYStatus())
        {
            if (TempSelCar == null) return;

            if (actualColor <= 0)
            {
                actualColor = 4;
            }
            else
            {
                actualColor--;
            }
            changeColor();

            #region Update Car Selection Button Call
            CanvasController.instance.UpdateCarSelectionButtons(InputManager.instance.device
                == InputManager.InputDevice.Controller,
                carSelectMenu == CarSelectMenu.Locked,
                InputManager.instance.controllerType == InputManager.ControllerType.Xbox);
            #endregion
        }
        #endregion

        #endregion
        #region Mouse and Keyboard Car Selection
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            NextCarLeft();

           
            #region Update Car Selection Button Call
            CanvasController.instance.UpdateCarSelectionButtons(InputManager.instance.device 
                == InputManager.InputDevice.Controller, 
                carSelectMenu == CarSelectMenu.Locked, 
                InputManager.instance.controllerType == InputManager.ControllerType.Xbox);
            #endregion
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextCarRight();

            #region Update Car Selection Button Call
            CanvasController.instance.UpdateCarSelectionButtons(InputManager.instance.device
                == InputManager.InputDevice.Controller,
                carSelectMenu == CarSelectMenu.Locked,
                InputManager.instance.controllerType == InputManager.ControllerType.Xbox);
            #endregion
        }

        if (Input.GetKeyDown("c"))
        {
            ChangeColourButton();

            #region Update Car Selection Button Call
            CanvasController.instance.UpdateCarSelectionButtons(InputManager.instance.device
                == InputManager.InputDevice.Controller,
                carSelectMenu == CarSelectMenu.Locked,
                InputManager.instance.controllerType == InputManager.ControllerType.Xbox);
            #endregion
        }
        #endregion

        #region Universal

        if (Input.GetButtonDown("Submit") && carSelectMenu != CarSelectMenu.Unselected && isReadyToConfirm)
        { 
            SelectCar();
        }

        if (Input.GetButtonDown("Cancel") && carSelectMenu != CarSelectMenu.Unselected && isReadyToConfirm)
        {
            //Ensures view model of the car gets destroyed
            if(TempSelCar != null) 
            { 
                Destroy(TempSelCar);
            }
            
            carSelectMenu = CarSelectMenu.Unselected;
            
            CanvasController.instance.EnableDisableCarViewUi(false);
        }
        #endregion
        #endregion
    }

    #region Select Car
    public void NextCarLeft() {
        if (carSelectMenu == CarSelectMenu.Unselected) return;

        //Resets Car Before Switching away
        if (actualColor != 0)
        {
            actualColor = 0;
            changeColor();
        }

        //sets car
        if (currentCar == 0)
        {
            currentCar = totalCars;
        }
        else
        {
            currentCar--;
        }

        SwitchCar();
    }

    public void NextCarRight()
    {
        if (carSelectMenu == CarSelectMenu.Unselected) return;

        //Resets Car Before Switching away
        if (actualColor != 0)
        {
            actualColor = 0;
            changeColor();
        }

        //sets car
        if (currentCar == totalCars)
        {
            currentCar = 0;
        }
        else
        {
            currentCar++;
        }

        SwitchCar();
    }

    public void SwitchCar()
    {
        if(carSelectMenu == CarSelectMenu.Unselected) return;
        
        //Creates a temporary gameobject that can be destroyed    
        GameObject TempTempSelCar = TempSelCar;
        Destroy(TempTempSelCar);

        if (carSelectMenu == CarSelectMenu.Owned)
        {
            TempSelCar = Instantiate(carListUnlocked[currentCar], CarViewSpawn.transform);

            //Gets prefab name to avoid the name ending with clone
            carNameText.text = carListUnlocked[currentCar].name;
        }
        else if (carSelectMenu == CarSelectMenu.Locked)
        {
            TempSelCar = Instantiate(carListLocked[currentCar], CarViewSpawn.transform);

            //Gets prefab name to avoid the name ending with clone
            carNameText.text = carListLocked[currentCar].name;
        }

        TempSelCar.transform.position = CarViewSpawn.transform.position;
        TempSelCar.transform.rotation = Quaternion.Euler(0, 50, 0);

        car_Container_Animator.Play("Cars_Container_Car_Change_Anim", -1, 0f);
        main_Camera_Animator.Play("Main_Camera_Car_Change_Anim", -1, 0f);

        carSoundSource = TempSelCar.transform.GetComponent<AudioSource>();

        if (carSelectMenu == CarSelectMenu.Locked)
        {
            SoundManager.instance.SetAudioClip(carSoundSource, carSounds[currentCar], true);
            SoundManager.instance.PlayAudioSource(carSoundSource);
        }
        else if(carSelectMenu == CarSelectMenu.Owned)
        {
            SoundManager.instance.SetAudioClip(carSoundSource, unlockedCarSounds[currentCar], true);
            SoundManager.instance.PlayAudioSource(carSoundSource);
        }
    
    }
    #endregion

    #region Change Colour
    public void ChangeColourButton ()
    {
        if(carSelectMenu == CarSelectMenu.Unselected) return;
        
        if (actualColor < 4)
        {
            actualColor++;
        }
        else if (actualColor >= 4)
        {
            actualColor = 0;
        }
        changeColor();
    }

    public void changeColor(){

        if (carSelectMenu == CarSelectMenu.Unselected) return;
        
        GameObject ModelsParent = null;
       
        if (carSelectMenu == CarSelectMenu.Owned)
        {
            ModelsParent = carListUnlockedPlayable[currentCar].transform.Find("Models").gameObject;
        }
        
        switch (actualColor)
        {
           
        case 0:
                TempSelCar.transform.Find("Body").GetComponent<Renderer>().material = ColorVar_1;
                
                if (carSelectMenu != CarSelectMenu.Owned) break;
                ModelsParent.GetComponentInChildren<Renderer>().material = ColorVar_1;
                break;

        case 1:
                TempSelCar.transform.Find("Body").GetComponent<Renderer>().material = ColorVar_2;

                if (carSelectMenu != CarSelectMenu.Owned) break;
                ModelsParent.GetComponentInChildren<Renderer>().material = ColorVar_2;
                
                break;

        case 2:
                TempSelCar.transform.Find("Body").GetComponent<Renderer>().material = ColorVar_3;
                
                if (carSelectMenu != CarSelectMenu.Owned) break;
                ModelsParent.GetComponentInChildren<Renderer>().material = ColorVar_3;
                
                break;

        case 3:
                TempSelCar.transform.Find("Body").GetComponent<Renderer>().material = ColorVar_4;
                
                if (carSelectMenu != CarSelectMenu.Owned) break;
                ModelsParent.GetComponentInChildren<Renderer>().material = ColorVar_4;
                
                break;

        case 4:
                TempSelCar.transform.Find("Body").GetComponent<Renderer>().material = ColorVar_5;
                
                if (carSelectMenu != CarSelectMenu.Owned) break;
                ModelsParent.GetComponentInChildren<Renderer>().material = ColorVar_5;
                
                break;
        }

    }
    #endregion
    
    /// <summary>
    /// This method either buys a car or spawns it, depending on which car select menu the player is in
    /// </summary>
    public void SelectCar() 
    { 
        //this executes when buying a new car
        if(carSelectMenu == CarSelectMenu.Locked) 
        {
            if (TempSelCar == null) return;

            carListUnlocked.Add(carListLocked[currentCar]);
            carListLocked.Remove(carListLocked[currentCar]);

            carListUnlockedPlayable.Add(carListLockedPlayable[currentCar]);
            carListLockedPlayable.Remove(carListLockedPlayable[currentCar]);

            unlockedCarSounds.Add(carSounds[currentCar]);
            carSounds.RemoveAt(currentCar);
           
            UnSelectCarsList();
            currentCar = 0;
            CanvasController.instance.EnableDisableCarSelectionUI(true);
        }
        //this spawns the car
        else if(carSelectMenu == CarSelectMenu.Owned) 
        {
            if (TempSelCar != null)
            {
                Destroy(TempSelCar);
            } 
            
            //Change so fuel level is whatever it was at before
            SpawnSpecifiedCar(CarSpawn.transform, carListUnlockedPlayable[currentCar], 1, 
                GameManager.instance.ReturnTransmissionTypeLoaded(), "SelectCar()", GameManager.instance.ReturnFuel());
            
            GameManager.instance.ToggleCarSelect(false);
        }

        if (GarageLights != null)
        {
            GarageLights.SetActive(false);
        }    
    }

    //Spawns Any Car
    //WARNING, IF CAR DOESNT WORK WHEN SPAWNED MAKE SURE GAME MANAGER STATE IS PLAYING
    public void SpawnSpecifiedCar(Transform transformToSpawnAt, GameObject car, int fuelDenomination, int TransmissionTypeIndex, string method_name, float exactFuelLevel = -1)
    {
        string method_called_from_name = method_name;

        print("Method that called spawn car: " + method_name);

        GameObject Car = Instantiate(car, transformToSpawnAt.position, transformToSpawnAt.rotation );

        AssignLastSpawnedCar(car);

        GameManager.instance.AssignSpawnedCarVariables(Car);

        CameraManager.instance.AssignCameras(Car);

        CarController controller = Car.GetComponent<CarController>();
        controller.SwitchTransmissionMode((CarController.typeOfTransmission)TransmissionTypeIndex);

        if (controller != null) 
        {
            if (exactFuelLevel > -1)
            {
                controller.currentFuel =  exactFuelLevel;
            }
            else
            {
                controller.SetFuelByDenomination(fuelDenomination);
            }
        }

        GameManager.instance.SetPlayState();
    }

    
    
    public void SelectOwnedCarsList() 
    {
        if (TempSelCar != null)
        {
            Destroy(TempSelCar);
        }

        SwitchCarSelectMenu(CarSelectMenu.Owned);
        totalCars = carListUnlocked.Count - 1;
        SwitchCar();

        #region Update Car Selection Button Call
        CanvasController.instance.UpdateCarSelectionButtons(InputManager.instance.device
            == InputManager.InputDevice.Controller,
            carSelectMenu == CarSelectMenu.Locked,
            InputManager.instance.controllerType == InputManager.ControllerType.Xbox);
        #endregion

        CanvasController.instance.EnableDisableCarViewUi(true);
    }

    public void SelectLockedCarsList() 
    {
        if(TempSelCar != null) 
        {
            Destroy(TempSelCar);
        }

        if (carListLocked.Count <= 0) return;

        SwitchCarSelectMenu(CarSelectMenu.Locked);
        
        totalCars = carListLocked.Count - 1;
        currentCar = 0;
        
        SwitchCar();

        #region Update Car Selection Button Call
        CanvasController.instance.UpdateCarSelectionButtons(InputManager.instance.device
            == InputManager.InputDevice.Controller,
            carSelectMenu == CarSelectMenu.Locked,
            InputManager.instance.controllerType == InputManager.ControllerType.Xbox);
        #endregion

        CanvasController.instance.EnableDisableCarViewUi(true);
    }

    public void UnSelectCarsList()
    {
        Destroy(TempSelCar);
        SwitchCarSelectMenu(CarSelectMenu.Unselected);
        currentCar = 0;
        totalCars = 0;

    }

    /// <summary>
    /// Gives player the raiden
    /// </summary>
    public void GrantRaiden()
    {
        //Changed the index of raiden to be the first one
        carListUnlocked.Add(carListLocked[0]);
        carListLocked.Remove(carListLocked[0]);

        carListUnlockedPlayable.Add(carListLockedPlayable[0]);
        carListLockedPlayable.Remove(carListLockedPlayable[0]);

        unlockedCarSounds.Add(carSounds[0]);
        carSounds.RemoveAt(currentCar);
        
        GameManager.instance.ForceDestroyCurCar();
        SpawnSpecifiedCar(raidenSpawnPoint, carListUnlockedPlayable[1], 1, 
            GameManager.instance.ReturnTransmissionTypeLoaded(), "Grant Raiden");
    }

    //Delays execution so car doesnt get confirmed before menu is shown
    private IEnumerator DelayConfirmButton() 
    { 
        isReadyToConfirm = false;

        yield return new WaitForSeconds(0.1f);

        isReadyToConfirm = true;
    }

    private void SwitchCarSelectMenu(CarSelectMenu _carSelectMenu)
    {
        StartCoroutine(DelayConfirmButton());
       
        carSelectMenu = _carSelectMenu;

        if (carSelectMenu == CarSelectMenu.Unselected)
        {
            GarageLights.SetActive(false);
        }
        else 
        { 
            GarageLights.SetActive(true);   
        }
    }

    private void AssignLastSpawnedCar(GameObject _car)
    {
        lastSpawnedCar = _car;
    }
    
    /// <summary>
    /// this simply returns most recently spawned player car, so the correct car can be spawned again once the car is destroyed for example. Eg.: if raiden was last car, spawn raiden
    /// </summary>
    public GameObject ReturnLastSpawnedCar()
    {
        return lastSpawnedCar;
    }

    private void OnEnable()
    {
        GarageLights.SetActive(false);
        carSelectMenu = CarSelectMenu.Unselected;
        currentCar = 0;
        actualColor = 0;
        TempSelCar = null;
    }
}
