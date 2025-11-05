using System.Collections;
using TMPro;
using UnityEngine;

public class PauseMenuControl : MonoBehaviour
{
    
    Camera PauseMenuCamera;
    
    [Header("Spotlight Reference")]
    [SerializeField] Light Spotlight;

    [Header("3D Ui References")]
    [SerializeField] TMP_Text ContinueText;
    [SerializeField] TMP_Text SettingsText;
    [SerializeField] TMP_Text QuitText;

    [SerializeField] float fontSize = 8;

    [Header("Animation References")]
    [SerializeField]Animator CameraAnimator;
    const string PAUSE_MENU_IN = "PauseMenuIn";
    const string PAUSE_MENU_OUT = "PauseMenuOut";
    [Header("Pause menu music")]
    [SerializeField] AudioSource pauseMenuMusic;

    //Misc
    bool isUnpausing = false;



    private void Start()
    {
        if (PauseMenuCamera == null) 
        {
            PauseMenuCamera = this.GetComponentInChildren<Camera>();
        }

        if(Spotlight == null) 
        { 
            Spotlight = this.GetComponentInChildren<Light>();
        }


        Spotlight.gameObject.SetActive(false);

        CameraAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    private void Update()
    {
       Ray ray = PauseMenuCamera.ScreenPointToRay(Input.mousePosition);
       
        if(Physics.Raycast(ray, out RaycastHit hit)) 
        {
            
            if (hit.collider.TryGetComponent<ContinueButton>(out ContinueButton continueButton))
            {
                HighlightMenuChoiceObj(continueButton.transform);
                
                if (Input.GetMouseButtonDown(0) && !isUnpausing)
                {
                    StartCoroutine(UnPauseGame());
                }

                if (ContinueText.fontSize < fontSize)
                {
                    scaleInText(ContinueText);
                }

                if (SettingsText.fontSize > 0 || QuitText.fontSize > 0) 
                {
                    scaleOutText(SettingsText);
                    scaleOutText(QuitText);
                }
            }
            else if(hit.collider.TryGetComponent<SettingsButton>(out SettingsButton settingsButton)) 
            { 
               HighlightMenuChoiceObj(settingsButton.transform);

                if (Input.GetMouseButtonDown(0))
                {
                    Debug.LogWarning("Continue Game");
                    //add actual ui to change settings
                }

                if (SettingsText.fontSize < fontSize)
                {
                    scaleInText(SettingsText);
                }


                if (ContinueText.fontSize > 0 || QuitText.fontSize > 0)
                {
                    scaleOutText(ContinueText);
                    scaleOutText(QuitText);
                }
            }
            else if (hit.collider.TryGetComponent<QuitGameButton>(out QuitGameButton quitGameButton))  
            {
                HighlightMenuChoiceObj(quitGameButton.transform);
                //QuitGame

                if (QuitText.fontSize < fontSize)
                {
                    scaleInText(QuitText);
                }

                if (ContinueText.fontSize > 0 || SettingsText.fontSize > 0)
                {
                    scaleOutText(ContinueText);
                    scaleOutText(SettingsText);
                }
            }
            else
            {
                TurnOffLight();
                if (ContinueText.fontSize > 0 || SettingsText.fontSize > 0 ||QuitText.fontSize > 0)
                {
                    scaleOutText(ContinueText);
                    scaleOutText(SettingsText);
                    scaleOutText(QuitText);
                }

            }
        }
       
    }

    private void HighlightMenuChoiceObj(Transform objectToLookAt) 
    {
        Spotlight.transform.LookAt(objectToLookAt.position);
        Spotlight.gameObject.SetActive(true);
    }    

    private void TurnOffLight() 
    {
        Spotlight.gameObject.SetActive(false);
    }


    #region Smooth Scale Effects
    private void scaleInText(TMP_Text textToScaleIn, float desiredFontSize = 8, float scaleInFactor = 40)
    {
        float new_fontSize = textToScaleIn.fontSize;
        new_fontSize = Mathf.MoveTowards(new_fontSize, desiredFontSize, scaleInFactor * Time.unscaledDeltaTime);
        textToScaleIn.fontSize = new_fontSize;
    }
   
    private void scaleOutText(TMP_Text textToScaleIn, float scaleInFactor = 40)
    {
        float new_fontSize = textToScaleIn.fontSize;
        new_fontSize = Mathf.MoveTowards(textToScaleIn.fontSize, 0, scaleInFactor * Time.unscaledDeltaTime);
        textToScaleIn.fontSize = new_fontSize;
    }
    #endregion


    //Plays the animation then once it's finished
    //Unpauses the game
    IEnumerator UnPauseGame()
    {
        isUnpausing = true;
        AnimationController.PlayAnimation(CameraAnimator, PAUSE_MENU_OUT);
        AnimatorStateInfo info = CameraAnimator.GetCurrentAnimatorStateInfo(0);

        yield return new WaitForSecondsRealtime(info.length);

        GameManager.instance.PauseGame();
        isUnpausing = false;
    }


    private void OnEnable()
    {
        if (PauseMenuCamera != null)
        {
            PauseMenuCamera.fieldOfView = 60;
        }
        AnimationController.PlayAnimation(CameraAnimator, PAUSE_MENU_IN);
        pauseMenuMusic.Play();
    }

    private void OnDisable()
    {
        PauseMenuCamera.fieldOfView = 60;
    }
}
