using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]private Transform target;

    [Header("Position Settings")]
    [SerializeField] Vector3 CameraDefaultPosition = new Vector3(7.80f, -148.12f, -4.78f);
    //y meters above and -z meters behind
    [SerializeField] Vector3 CameraOffset = new Vector3(0, 2f, 0);

    [Header("Follow Settings")]
    //higher smoothness = snappier lower = lag behind
    [SerializeField] float FollowSmoothness = 6f;

    [Header("Rotation Settings")]
    [SerializeField] Vector3 DefaultRotation;
    //higher smoothness = snappier lower = lag behind
    [SerializeField] float RotationSmoothness = 6f;
    [SerializeField] float LookHeightOffset = 1.5f;

    [Header("Collison Settings")]
    [Space(2)]
    [Header("This is the layer the camera will collide with")]
    [SerializeField] LayerMask CollisionMask;
    [SerializeField] float CameraRadius = 0.35f;
    [SerializeField] float CollisionOffset = 0.3f;
    [SerializeField] float MinDistance = 1.5f;

    private Vector3 currentVelocity;

    private void Start()
    {
        transform.position = CameraDefaultPosition;
        transform.rotation = Quaternion.Euler(DefaultRotation);
    }

    private void FixedUpdate()
    {
       if(!target) return;

        //move each part into own method *
        //add collision logic making cam move closer to car so it doesnt clip through

        CheckCollisionAndMoveCamera();
        RotateCamera();

    }


    private void CheckCollisionAndMoveCamera() 
    {
        
        Vector3 desiredPos = target.TransformPoint(CameraOffset);

        if (Physics.SphereCast(target.position, CameraRadius, (desiredPos - target.position).normalized, out RaycastHit hit,
           CameraOffset.magnitude, CollisionMask))
        {
            // Instead of snapping to hit.point, reduce distance along the view direction
            float adjustedDistance = hit.distance - CollisionOffset;

            // Keep a minimum safe distance so it never goes *into* the car
            adjustedDistance = Mathf.Clamp(adjustedDistance, MinDistance, CameraOffset.magnitude);

            // Move along the same direction, but stop before the collision
            desiredPos = target.position + (desiredPos - target.position).normalized * adjustedDistance;
        }

        //smooth damp moves from a to b with intertia
        //used here to create the cool lag behind when speeding and slow catch up when braking effect
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref currentVelocity, 1f / FollowSmoothness);
    }

    private void RotateCamera() 
    {
        //height offset to look above the center of the car rather than at the centre
        Vector3 lookTarget = target.position + Vector3.up * LookHeightOffset;
        Quaternion targetRot = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * RotationSmoothness);
    }

    public void GiveTarget(Transform _target) 
    { 
     target = _target;
    }
}
    

