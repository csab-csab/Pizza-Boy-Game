using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerExplosion : MonoBehaviour
{
     private Destructible destructible;

     [SerializeField] float ExplosionRadius = 10f;
     [SerializeField] float ExplosionForce = 15f;
     [SerializeField] float DestroyedLifeTime = 10f;


    private void Start()
    {
        TryGetComponent<Destructible>(out destructible);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //print(collision.transform.name + " with " + gameObject.name);
        if (collision.transform.CompareTag(GameManager.instance.DESTRUCTIBLE_TAG) || collision.transform.CompareTag(GameManager.instance.PLAYER_CAR_TAG))
        {
            ParticleEffectsControl.instance.PlayExplosionAt(this.transform.position, ExplosionRadius, ExplosionForce);

            if (destructible != null) 
            {
                destructible.HandleExplosion();
            }
        }
    }
}
