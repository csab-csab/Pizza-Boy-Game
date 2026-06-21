using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeLookCameraMovement : MonoBehaviour
{
   private bool isMovementEnabled = false;

    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float lookSens = 0.5f;

    float sensMultiplier = 10;

    bool mouseMovement = false;
    bool controllerMovement = false;

    private Vector2 velocity;
    Vector2 currentRotation;
    [SerializeField] Vector2 smoothing = new Vector2(2000, 2000);

    [SerializeField] bool invertY = true;  


    private void Update()
    {
       if(isMovementEnabled){
        float horizontalMove = Input.GetAxisRaw("Horizontal");
        float verticalMove = Input.GetAxisRaw("Vertical");

        float horizontal = Input.GetAxisRaw("Mouse X") * (lookSens * sensMultiplier);
        float vertical = invertY ? -Input.GetAxisRaw("Mouse Y") * (lookSens * sensMultiplier) 
            : -Input.GetAxisRaw("Mouse Y") * (lookSens * sensMultiplier);
        

        float controllerHorizontal = Input.GetAxis("Joystick Look X") * (lookSens * sensMultiplier);
        float controllerVertical = invertY ? -Input.GetAxis("Joystick Look Y") * (lookSens * sensMultiplier) :
                                    Input.GetAxis("Joystick Look Y") * (lookSens * sensMultiplier);

        

        //Calculate movement based on direction camera is facing
        Vector3 moveDirection = transform.forward * verticalMove + transform.right * horizontalMove;

        transform.Translate(moveDirection * moveSpeed * Time.unscaledDeltaTime, Space.World);

        if (Input.GetButton("Camera Move Up"))
        {
         transform.Translate(Vector3.up * moveSpeed * Time.unscaledDeltaTime, Space.World);
        }

        #region Controller Camera Move up/down
        if (Input.GetAxis("RT") > 0) 
        {
            float rt = Input.GetAxis("RT");
            transform.Translate(new Vector3( 0, rt) * moveSpeed * Time.unscaledDeltaTime, Space.World);
        }

        if (Input.GetAxis("LT") > 0)
        {
            float lt = Input.GetAxis("LT");
            transform.Translate(new Vector3(0, -lt) * moveSpeed * Time.unscaledDeltaTime, Space.World);
        }
        #endregion
       
        if (Input.GetMouseButtonDown(1))
        {
            mouseMovement = !mouseMovement;
            CanvasController.instance.ToggleCursor(!mouseMovement);
        }

        if (Input.GetMouseButtonDown(2))
        {
            controllerMovement = !controllerMovement;
            CanvasController.instance.ToggleCursor(!controllerMovement);
        }

        if (mouseMovement)
        {

             velocity = new Vector2(
                 Mathf.MoveTowards(velocity.x, -vertical, smoothing.x *Time.unscaledDeltaTime),
                 Mathf.MoveTowards(velocity.y, horizontal, smoothing.y * Time.unscaledDeltaTime));
        

          currentRotation += velocity;

          transform.localEulerAngles =  new Vector3(Mathf.Clamp(currentRotation.x, -100, 100), currentRotation.y);
        }

        if (controllerMovement)
        {

            velocity = new Vector2(
             Mathf.MoveTowards(velocity.x, -controllerVertical, smoothing.x * Time.deltaTime),
             Mathf.MoveTowards(velocity.y, controllerHorizontal, smoothing.y * Time.deltaTime));


            currentRotation += velocity;

            transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, 0);

            // //Invert the Y axis for the controller as well
            // transform.Rotate(controllerVertical, controllerHorizontal, 0);
            //
            // //Keep the camera's roll at zero:
            // transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
        }
        
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            TeleportToPlayer();
        }
       }
    }

    public void ToggleMovement(bool MovementEnabled)
    {
        isMovementEnabled = MovementEnabled;
    }
    
    private void ClampRotation(float angle) 
    { 
        Mathf.Clamp(angle, -90, 90);
    }
    
    private void TeleportToPlayer()
    {
        CarController carController = GameManager.instance.AccessCarController();
        
        print("Trying to teleport to player");
        
        if (carController != null)
        {
            print("Teleported to player executed correctly");
            transform.position = carController.transform.position;    
        }
    }
}
