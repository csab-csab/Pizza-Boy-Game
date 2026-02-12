using System.Collections;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Destructible : MonoBehaviour
{
    [Header("Time before object gets disabled after collision")]
    [SerializeField]static float DestroyedLifeTime = 10f;
    
    private Vector3 intialPosition;
    private Quaternion intialRotation;
    
    private Rigidbody rb;

    //ensures that the destruction of object is handled by trigger explosion
    //rather than this script if it exists on this object
    bool isExplosive = false;
    private void Start()
    {
        if (TryGetComponent<Rigidbody>(out rb))
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        intialPosition = transform.position;
        intialRotation = transform.rotation;

        if (GetComponent<TriggerExplosion>()) 
        { 
            isExplosive = true;
        }
    }

    public void HandleDestruction()
    {
        if (!isExplosive)
        {
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            StartCoroutine(DisableObject(DestroyedLifeTime));
        }
    }

    public void HandleExplosion() 
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        StartCoroutine(DisableObject());
    }

    public void ResetAfterDestruction()
    {
        transform.position = intialPosition;
        transform.rotation = intialRotation;

        if (TryGetComponent<Rigidbody>(out rb))
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        gameObject.SetActive(true);
    }

    IEnumerator DisableObject(float lifeTime = 0) 
    {
        yield return new WaitForSecondsRealtime(lifeTime);

        this.gameObject.SetActive(false);
        DestructibleManager.instance.RegisterDestruction(this);
    }
}
