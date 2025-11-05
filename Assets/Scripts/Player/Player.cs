using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private float Money;
    private float xp;
    private float xpToNextLevel;
    private float xpLevel;

    public void GiveMoney(float amount)
    {
        Money += amount;
    }

    public void TakeMoney(float amount) 
    {
        Money -= amount;
    }
}
