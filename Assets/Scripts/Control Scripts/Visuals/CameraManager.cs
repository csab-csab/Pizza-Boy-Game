using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    #region Consts
    private const string SECONDARY_CAM_NAME = "Camera";

    private const string CAM_POS_PARENT_NAME = "Camera Positions";

    #endregion

    #region Enums
    public enum CameraView {Normal, Reverse, Refuel };
    public CameraView cameraView;

    public enum PumpSide { Left, Right};
    public PumpSide pumpSide;
    #endregion

    #region Camera Properties
    float revrseCameraFov = 85f;
    float reverseFarClip = 5000f;

    //bool to see if secondary camera should be lerped
    //triggered by camera mode switch
    [SerializeField]bool lerpCamera = false;
    public float lerpRate = 0.25f;
    #endregion

    #region refs
    [SerializeField]Transform MainCamera;
    [SerializeField] CameraController cameraController;  

    //Camera used for reversing and refueling
    Transform SecondaryCamera;

    List<Transform> CameraPositionsList;

    
    #endregion
    private void Awake()
    {
       instance = this;
    }

    private void Start()
    {
        cameraController = MainCamera.GetComponent<CameraController>();
    }


    private void Update()
    {
        if (lerpCamera) 
        {
            if (cameraView == CameraView.Normal) 
            { 
                LerpCamera(MainCamera.transform.position, MainCamera.transform.rotation, lerpRate);
            }

            if (cameraView == CameraView.Reverse) {
                LerpCamera(CameraPositionsList[0].position, CameraPositionsList[0].rotation, lerpRate); 
            }

            if (cameraView == CameraView.Refuel)
            {
                //opposing side to pump
                if (pumpSide == PumpSide.Left)
                {
                    LerpCamera(CameraPositionsList[2].position, CameraPositionsList[2].rotation, lerpRate);

                }
                else
                {
                    LerpCamera(CameraPositionsList[1].position, CameraPositionsList[1].rotation, lerpRate);
                }
            }
        }
    }

    
    public void AssignCameras(GameObject car) 
    {
       
        
        SecondaryCamera = car.transform.Find(SECONDARY_CAM_NAME);

        SecondaryCamera.GetComponent<Camera>().fieldOfView = revrseCameraFov;

        SecondaryCamera.GetComponent<Camera>().farClipPlane = reverseFarClip;

        //stops annoying 2 audio listeners message
        if (SecondaryCamera.TryGetComponent<AudioListener>(out AudioListener listener)) 
        {
            listener.enabled = false;
        }

        MainCamera.gameObject.SetActive(true);
        SecondaryCamera.gameObject.SetActive(false);

        CameraPositionsList = new List<Transform>();

        Transform parent = car.transform.Find(CAM_POS_PARENT_NAME); 
    
        for(int i = 0; i < parent.childCount; i++) 
        { 
         CameraPositionsList.Add(parent.GetChild(i));
        }
    }

    public void SwitchCameraMode(CameraView view) 
    { 
        cameraView = view;

        switch(view) 
        { 
            case CameraView.Normal:
                lerpCamera = true;
                break;
            
            case CameraView.Reverse:
                MainCamera.GetComponent<Camera>().enabled = false;

                SecondaryCamera.gameObject.SetActive(true);
                
                lerpCamera = true;
                break;

            case CameraView.Refuel:
                MainCamera.GetComponent<Camera>().enabled = false;



                SecondaryCamera.gameObject.SetActive(true);
                lerpCamera = true;
                break;
        }
    }

    private void LerpCamera(Vector3 targetPos, Quaternion targetRot, float rate) 
    {
        //cache cams
        Camera Main = MainCamera.GetComponent<Camera>();
        Camera Secondary = SecondaryCamera.GetComponent<Camera>();
        
        //Lerp the FOV to match main cam
        if (cameraView == CameraView.Normal)
        {
          Secondary.fieldOfView = Mathf.Lerp(Secondary.fieldOfView, Main.fieldOfView, rate);
        }
        else if (cameraView == CameraView.Reverse) 
        {
            Secondary.fieldOfView = Mathf.Lerp(Secondary.fieldOfView, revrseCameraFov, rate);
        }

        //cache cam transform
        Transform camTrans = SecondaryCamera.transform;
        


        Vector3 lerpedPos = new Vector3 (Mathf.Lerp(camTrans.position.x, targetPos.x, rate), 
                                        Mathf.Lerp(camTrans.position.y, targetPos.y, rate),
                                        Mathf.Lerp(camTrans.position.z, targetPos.z, rate));

        Quaternion lerpedRotation = new Quaternion(Mathf.LerpAngle(camTrans.rotation.x, targetRot.x, rate),
                                        Mathf.LerpAngle(camTrans.rotation.y, targetRot.y, rate),
                                        Mathf.LerpAngle(camTrans.rotation.z, targetRot.z, rate),
                                        Mathf.LerpAngle(camTrans.rotation.w, targetRot.w, rate));

        SecondaryCamera.transform.position = lerpedPos;
        SecondaryCamera.rotation = lerpedRotation;

        float positionTolerance = 0.01f;
        float rotationTolerance = 0.01f;

        if (Vector3.Distance(camTrans.position, targetPos) < positionTolerance && Quaternion.Angle(camTrans.rotation, targetRot) < rotationTolerance) 
        { 
            lerpCamera = false;
            
            if (cameraView == CameraView.Normal) 
            {

                MainCamera.GetComponent<Camera>().enabled = true;

                SecondaryCamera.gameObject.SetActive(false);
            }
        }


    }

    public void ToggleMainCamera(bool enabled) 
    {
        if (MainCamera != null)
        {
            MainCamera.GetComponent<Camera>().enabled = enabled;
        }
    }

    //ensures car transform reference is passed to cinemachine cam
    public void PassRefsToCinemachine(Transform car_transform) 
    {
        cameraController.GiveTarget(car_transform);
    }
}
