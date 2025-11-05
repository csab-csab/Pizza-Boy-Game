using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    #region Singleton
    public static InputManager instance;
    #endregion

    public enum InputDevice { Keyboard, Controller};
    public InputDevice device;

    public enum ControllerType {Xbox, PS};
    public ControllerType controllerType;

    private void Start()
    {
        #region Singleton Setup
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != this) 
        { 
          Destroy(instance);
          instance = this;
        }
        #endregion
    }

    private void Update()
    {
        CheckCurrentInputDevice();
    }
    
    private void CheckCurrentInputDevice() 
    {
        Gamepad gamepad = Gamepad.current;
        Keyboard keyboard = Keyboard.current;

        if(gamepad != null && gamepad.wasUpdatedThisFrame) 
        {
            device = InputDevice.Controller;
            controllerType = gamepad.name.Contains("Xbox") ? controllerType = ControllerType.Xbox : controllerType = ControllerType.PS;
        }
        else if(keyboard != null && keyboard.wasUpdatedThisFrame)
        {
            device = InputDevice.Keyboard;
        }
    }
}
