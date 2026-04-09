using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour, IDataPersistance
{
    #region Player Progress
    [SerializeField] int delisCompleted = 0;
    [SerializeField]private float Money;

    #endregion

    #region Car Health
    private float MaxCarHealth = 100;
    [SerializeField]private float CarHealth = 100;

    //this variable is used as the number of seconds which the car is on fire for
    //before blowing up
    //this will create a 3d era gta car explosion effect
    private float MaxDestroyedCarHealth = 10;
    
    private float DestroyedCarHealth = 5;

    [SerializeField]bool isCarDestroyed;
    #endregion

    private void Start()
    {
        CanvasController.instance.UpdateMoneyText(Money);
        
    }

    private void Update()
    {
        //See method for description
        if(CarHealth <= 0 && DestroyedCarHealth > 0) 
        {
            DamageDestroyedCar(1);
        }

        if (Input.GetKey(KeyCode.L))
        {
            InstantDestroy();
        }
    }

    #region Money
    public void GiveMoney(float amount)
    {
        Money += amount;
        CanvasController.instance.UpdateMoneyText(Money);
    }

    public void TakeMoney(float amount) 
    {
        Money -= amount;
        CanvasController.instance.UpdateMoneyText(Money);
    }

    public float ReturnMoney() 
    { 
       return Money;
    }
    #endregion

    #region Car Health Managment
    //move to car controller
    public void DamageCar(float amount)
    {
        CarHealth -= amount;
        
        ParticleEffectsControl.instance.PlayCarDamageFx(CarHealth, MaxCarHealth);
    }

    //This method is used when the car health reaches 0
    //When this is the case, the car sets on fire and
    //this is essential the countdown until the car blows up and gets
    //destroyed
    private void DamageDestroyedCar(float amount) 
    { 
        DestroyedCarHealth -= amount * Time.deltaTime;

        if (DestroyedCarHealth <= MaxDestroyedCarHealth / 2 && !isCarDestroyed) 
        {
            GameManager.instance.DisableCar(true);
            ParticleEffectsControl.instance.PlayCarDestructionFX();

            isCarDestroyed = true;
        }

        if (DestroyedCarHealth < 0)
        {
            GameManager.instance.DestroyCar();
        }
    }

    public void InstantDestroy()
    {
        if (isCarDestroyed) return;
        
        isCarDestroyed = true;
        GameManager.instance.DisableCar(true);
        ParticleEffectsControl.instance.PlayCarDestructionFX();
        StartCoroutine(DestroyCarAfterExplosion(3));
    }

    IEnumerator DestroyCarAfterExplosion(float delay) 
    { 
        yield return new WaitForSeconds(delay);
        GameManager.instance.DestroyCar();
       
    }

    public void RepairCar(float amount) 
    {
        if (CarHealth >= MaxCarHealth)
        {
            CarHealth = MaxCarHealth;
            return;
        }

        ParticleEffectsControl.instance.PlayCarDamageFx(CarHealth, MaxCarHealth);
        CarHealth += amount;
        isCarDestroyed = false;
    }
    
    public void ResetHealth() 
    {
        CarHealth = MaxCarHealth;
        DestroyedCarHealth = MaxDestroyedCarHealth;
       

        ParticleEffectsControl.instance.PlayCarDamageFx(CarHealth, MaxCarHealth);

        isCarDestroyed = false;
    }
    #endregion

    public void AddToDelisComplete() 
    {
        delisCompleted++;
    }

    public int ReturnDelisCompleted() 
    {
        return delisCompleted;
    }

    public void SaveGameData(ref GameData gameData)
    {
        gameData.playerMoney = this.Money;
        gameData.deliveriesCompleted = this.delisCompleted;
    }

    public void LoadGameData(GameData gameData)
    {
        this.Money = gameData.playerMoney;
        this.delisCompleted = gameData.deliveriesCompleted;
    }

    public void SaveSettingsData(ref SettingsData settingsData)
    {
       
    }

    public void LoadSettingsData(SettingsData settingsData)
    {
       
    }
}
