using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCutsceneManager : MonoBehaviour
{
    [SerializeField] Transform carTransform;
    
    [SerializeField] Transform carStartPosition;

    private void Start()
    {
        carTransform.position = carStartPosition.position;
    }
}
