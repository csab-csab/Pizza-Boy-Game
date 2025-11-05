using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public static void PlayAnimation(Animator animator, string stateName) 
    {
        try 
        {
            animator.Play(stateName);
        }
        catch 
        {
            Debug.LogError("Unable to play animation. There is likely an issue with the state name. \n" +
                "Please make sure you are passing the correct animation state name (not nessecarily the clip name)");
        }
    }
}
