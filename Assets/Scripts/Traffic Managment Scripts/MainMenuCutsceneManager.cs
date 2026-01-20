using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MainMenuCutsceneManager : MonoBehaviour
{
    [SerializeField] Transform carTransform;
    
    [SerializeField] Transform carStartPosition;

    [SerializeField] PlayableDirector playableDirector;

    [SerializeField]GameObject StartUpSplah;

    [SerializeField] MainMenuManager mainMenuManager;

    [SerializeField]float EnableUiTime = 20.5f;
    bool isUiEnabled = false;

    private void Start()
    {
        carTransform.position = carStartPosition.position;
        
    }

    private void Update()
    {
        if(Input.GetButtonDown("Submit") && playableDirector.time < 16f && playableDirector.time > 0)
        {
            playableDirector.Pause();
            playableDirector.time = 16.7f;
            playableDirector.Evaluate();
            playableDirector.Play();
        }

        if(playableDirector.time >= 16f && StartUpSplah.activeInHierarchy)
        {
            
            StartUpSplah.SetActive(false);
        }

        if(!isUiEnabled && playableDirector.time >= EnableUiTime)
        {
            isUiEnabled = true;
            mainMenuManager.OpenTitleScreen();
        }
    }
}
