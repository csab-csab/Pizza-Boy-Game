using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UI.Image;

public class ParticleEffectsControl : MonoBehaviour
{
    public static ParticleEffectsControl instance;

    private enum ParticleQuality { Low, Medium, High };
    private ParticleQuality quality;

    #region Special Effects Variables
    [Header("Camera FX")]
    [SerializeField] ParticleSystem SpeedLinesFX;

    [Header("Destruction Effects")]
    [SerializeField] ParticleSystem BaseDestructionEffectLQ;
    [SerializeField] ParticleSystem BaseDestructionEffectHQ;
    [SerializeField] ParticleSystem ExplosionEffect;
    [SerializeField] LayerMask LayerAffectedByExplosions;

    [Header("Tyre Variables")]
    List<TrailRenderer> tyreMarksRenderers;
    bool trailRendersAssigned = false;

    List<ParticleSystem> TyreSmokes;
    bool smokesAssigned = false;

    [Header("Delivery Effects")]
    //The pizza box object that gets thrown out the car when delivered
    [SerializeField] GameObject PizzaBox;
    [SerializeField] float ThrowForce;
     [SerializeField]Transform PointToThrowPizzaTrans;

    [Header("Car Destruction Effect")]
    //Assign in inspector
    [SerializeField] Transform PizzaStartPos;
    [SerializeField] GameObject BurntPizzaObj;
    [SerializeField] List<ParticleSystem> DamageFx;
    [SerializeField] MeshRenderer NormalCarBody;
    [SerializeField] GameObject DestroyedCarBody;
    bool damageFxAssigned = false;
    

    #endregion

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        instance = this;

    }

    public void PlayParticleEffect(ParticleSystem particleSystem)
    {
        particleSystem.Play();
    }

    public void StopParticleEffect(ParticleSystem particleSystem)
    {
        particleSystem.Stop();
    }

    #region Get Special Variables

    public void GetTyreMarkRenders(List<TrailRenderer> tym)
    {
        tyreMarksRenderers = tym;

        foreach (TrailRenderer tr in tym)
        {
            tr.autodestruct = false;
        }

        trailRendersAssigned = true;
    }

    public void GetTyreSmokes(List<ParticleSystem> tyresmokes)
    {
        TyreSmokes = tyresmokes;

        smokesAssigned = true;
    }

    public void GetDamageParticles(List<ParticleSystem> damageParticles) 
    {
       DamageFx = damageParticles;

        foreach (ParticleSystem particle in damageParticles) 
        { 
          particle.gameObject.SetActive(false);
        }
       
       damageFxAssigned = true;
    }

    public void GetCarBodies(MeshRenderer normal, GameObject destroyed) 
    { 
        NormalCarBody = normal;
        DestroyedCarBody = destroyed;
    }

    //call this on car destroy to ensure particle effects get assigned correctly when car is destroyed
    public void ResetSpecialVariables()
    {
        damageFxAssigned = false; 
        smokesAssigned = false;
        trailRendersAssigned = false;
    }
    #endregion



    #region Play Special Effects
    public void PlayBaseDestructionEffectAT(Vector3 position)
    {
        switch (quality)
        {
            case ParticleQuality.Low:

                ParticleSystem particleSystem = Instantiate(BaseDestructionEffectLQ);

                particleSystem.transform.position = position;

                particleSystem.Play();

                Destroy(particleSystem.gameObject, 2f);
                break;

            case ParticleQuality.Medium:

                ParticleSystem particleSystem1 = Instantiate(BaseDestructionEffectLQ);

                particleSystem1.transform.position = position;

                particleSystem1.Play();

                Destroy(particleSystem1.gameObject, 2f);
                break;

            case ParticleQuality.High:

                ParticleSystem particleSystem2 = Instantiate(BaseDestructionEffectHQ);

                particleSystem2.transform.position = position;

                particleSystem2.Play();

                Destroy(particleSystem2.gameObject, 2f);
                break;
        }
    }

    public void PlayExplosionAt(Vector3 position, float radius, float force) 
    {
        Debug.LogError("Explode");
        ParticleSystem particleSystem = Instantiate(ExplosionEffect);

        particleSystem.transform.position = position;

        particleSystem.Play();

        AddExplosionForceToArea(position, radius, force);

        Destroy(particleSystem.gameObject, 2f);
    }

    public void RenderTyreMarks(bool renderTyreMarks, bool renderOnlyRears = false)
    {
        if (!trailRendersAssigned) return;

        if (renderTyreMarks && tyreMarksRenderers[0].emitting == false)
        {
            if (renderOnlyRears)
            {
                for (int i = 2; i < tyreMarksRenderers.Count; i++)
                {
                    tyreMarksRenderers[i].emitting = true;
                }
            }
            else 
            {
                for (int i = 0; i < tyreMarksRenderers.Count; i++)
                {
                    tyreMarksRenderers[i].emitting = true;
                }
            }
        }
        else if (!renderTyreMarks && tyreMarksRenderers[0].emitting == true)
        {
            for (int i = 0; i < tyreMarksRenderers.Count; i++)
            {
                tyreMarksRenderers[i].emitting = false;
            }
        }

    }

    public void PlayTyreSmoke(bool renderTyreSmoke)
    {
        if(!smokesAssigned) return;
        
        foreach (ParticleSystem s in TyreSmokes)
        {
            if (renderTyreSmoke && !s.isEmitting)
            {
                s.Play();
            }
            else if(!renderTyreSmoke && s.isEmitting)
            {
                s.Stop();
            }
        }
    }

    public void ThrowPizzaOutCar(GameObject carObj)
    {
        GameObject pizzaTemp = Instantiate(PizzaBox);
        Rigidbody pizzaRB = pizzaTemp.GetComponent<Rigidbody>();

        pizzaTemp.transform.position = carObj.transform.position;

        if (pizzaRB != null)
        {
          
            //Ignore collision between car and pizza
            Physics.IgnoreCollision(pizzaTemp.GetComponent<Collider>(), carObj.GetComponentInChildren<Collider>(), true);

            if(PointToThrowPizzaTrans != null) 
            {
                Vector3 direction = (PointToThrowPizzaTrans.position - GameManager.instance.ReturnCarPosition()).normalized;
                pizzaRB.AddForce(direction * ThrowForce, ForceMode.Impulse);
                Debug.LogError("Yes");
            }
            else 
            {
                //needs to be impulse
                pizzaRB.AddRelativeForce(new Vector3(1, 1) * ThrowForce, ForceMode.Impulse);
                Debug.LogError("No");
            }
   
        }

        Destroy(pizzaTemp, 5f);
    }

    public void GivePizzaToThrowToPos(Transform PTT) 
    { 
     PointToThrowPizzaTrans = PTT;
    }
    
    public void PizzaFallOnCarDestroy(bool carDestroyed) 
    {
        if (carDestroyed)
        {
            LightingManager.instance.SetTimeOfDay("PizzaFallOnCarDestroy/ParticleFxCont",12, true); 
            BurntPizzaObj.transform.position = PizzaStartPos.position;
            BurntPizzaObj.gameObject.SetActive(true);
        }
        else 
        {
            LightingManager.instance.SetTimeOfDay("PizzaFallOnCarDestroy/ParticleFxCont",12, false);
            BurntPizzaObj.transform.position = PizzaStartPos.position;
            BurntPizzaObj.gameObject.SetActive(false);
        }
    }

    //this function should get called everytime the car health changes
    //this function can then play the corresponding damage effect
    public void PlayCarDamageFx(float health, float maxHealth) 
    {
        if (!damageFxAssigned) return;

        if (health >= maxHealth * 0.75f )
        {
            foreach (ParticleSystem ps in DamageFx)
            {
                //Check for null to avoid a null ref bug
                if (ps != null)
                {
                    if (ps.isPlaying)
                    {
                        ps.Stop();
                    }
                    ps.gameObject.SetActive(false);
                }
                else 
                {
                    Debug.LogError("Paricle system is null");
                }
            
            }
        }
        
        //checks if health is between 75% and 50%
        else if (health < maxHealth * 0.75f && health > maxHealth * 0.50f) 
        {
            #region Loop 
            //Disables all but the nessecary particle system obj
            foreach (ParticleSystem ps in DamageFx) 
            { 
                if(ps == DamageFx[0]) 
                {
                    ps.gameObject.SetActive(true);
                }
                else 
                {
                    ps.gameObject.SetActive(false);
                }
            }
            #endregion
            //Plays Light Damage Effect
            DamageFx[0].Play();
        }
        
        //Checks if health is between 50% and 25%
        else if(health < maxHealth * 0.50f && health >= maxHealth * 0.25f) 
        {
            #region Loop
            //Disables all but the nessecary particle system obj
            foreach (ParticleSystem ps in DamageFx)
            {
                if (ps == DamageFx[1])
                {
                    ps.gameObject.SetActive(true);
                }
                else
                {
                    ps.gameObject.SetActive(false);
                }
            }
            #endregion
            //Plays medium damage effect
            DamageFx[1].Play();
        }
        
        //Checks if health is between 25% and 1%
        else if(health < maxHealth * 0.25f && health >= maxHealth * 0.01f) 
        {
            #region Loop
            //Disables all but the nessecary particle system obj
            foreach (ParticleSystem ps in DamageFx)
            {
                if (ps == DamageFx[2])
                {
                    ps.gameObject.SetActive(true);
                }
                else
                {
                    ps.gameObject.SetActive(false);
                }
            }
            #endregion
            //Plays Heavy Damage Effect
            DamageFx[2].Play();
        }
        
        else if(health <= 0) 
        {
            #region Loop
            //Disables all but the nessecary particle system obj
            foreach (ParticleSystem ps in DamageFx)
            {
                if (ps == DamageFx[3])
                {
                    ps.gameObject.SetActive(true);
                }
                else
                {
                    ps.gameObject.SetActive(false);
                }
            }
            #endregion
            DamageFx[3].Play();
        }
    }

    public void PlayCarDestructionFX() 
    {
        PlayExplosionAt(NormalCarBody.transform.position, 10f, 15f);
        //One doesnt have a gameobject... and the other doesnt have a mesh renderer so this is the easy fix
        NormalCarBody.enabled = false;
        DestroyedCarBody.SetActive(true);
    }

    public void PlaySpeedLineFX(bool play)
    {
        switch (play)
        {
            case true:
                SpeedLinesFX.Play();
                break;
            case false:
                SpeedLinesFX.Stop();
                break;
        }
    }

    public void AddExplosionForceToArea(Vector3 origin, float radius, float force, float player_car_damage = 50) 
    {
        string player_tag = GameManager.instance.PLAYER_CAR_TAG;
       
        RaycastHit[] hits;
        hits = Physics.SphereCastAll(origin, radius, origin,  15, LayerAffectedByExplosions, QueryTriggerInteraction.Collide);

        foreach (RaycastHit hit in hits) 
        {
            if (hit.rigidbody != null)
            {
             
                hit.rigidbody.constraints = RigidbodyConstraints.None;
               
                hit.rigidbody.AddExplosionForce(1000, hit.transform.position, 100);

                if (hit.transform.CompareTag(player_tag)) 
                {
                    GameManager.instance.ReturnPlayerManager().DamageCar(player_car_damage);
                }

                if(hit.transform.TryGetComponent<Destructible>(out Destructible destructible)) 
                { 
                    destructible.HandleDestruction();
                }
            }
        }        
    }

 

    public void ResetCarFX(  ) 
    {
        damageFxAssigned = false;
     }

    public void OnDrawGizmos()
    {
       
    }
    #endregion

}