using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(DictionarySerializer))]
public class MainMenuManager : MonoBehaviour
{
    public DictionarySerializer dictionarySerializer;
    private Dictionary<int, GameObject> menuIndexDic = new Dictionary<int, GameObject>();

    private int currentScreenIndex = 0;

    [Header("Smooth ScaleInUiVals")]
    [SerializeField] float scaleRate = 0.25f;

    [Header("LoadingScreen")]
    [SerializeField] Image LoadingBar;

    private void Start()
    {
        for (int i = 0; i < dictionarySerializer.screenIndex.Count; i++)
        {
            menuIndexDic.Add(dictionarySerializer.screenIndex[i], dictionarySerializer.gameObjects[i]);
        }

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }


    private void NavigateMainMenu(int newScreen)
    {
        if (newScreen < 0)
        {
            newScreen = 0;
        }

        //disabled current ui
        if (menuIndexDic.ContainsKey(currentScreenIndex))
        {
            StartCoroutine(ScaleOutUi(menuIndexDic[currentScreenIndex]));
        }

        //enable new ui
        if (menuIndexDic.ContainsKey(newScreen))
        {
            StartCoroutine(ScaleInUi(menuIndexDic[newScreen]));
            currentScreenIndex = newScreen;
        }

    }

    //Methods that are called by pressing buttons
    #region Button methods
    public void OpenTitleScreen()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        NavigateMainMenu(0);
    }

    public void OpenMainMenu()
    {
        NavigateMainMenu(1);
    }

    public void OpenSettings()
    {
        NavigateMainMenu(2);
    }

    public void ReturnToPrevScreen()
    {
        NavigateMainMenu(currentScreenIndex - 1);
    }

    public void StartGame()
    {
        NavigateMainMenu(4);
        SceneManager.instance.LoadScene(1);
    }

    public void Quit()
    {
        Application.Quit();
    }


    #endregion

    #region Scale in ui

    private IEnumerator ScaleInUi(GameObject UiComp)
    {
        float currentScale = 0;
        while (!Mathf.Approximately(UiComp.transform.localScale.x, 1f))
        {
            currentScale = Mathf.MoveTowards(currentScale, 1f, scaleRate * Time.deltaTime);
            UiComp.transform.localScale = new Vector3(currentScale, 1f, 1f);
            yield return null; //wait a frame
        }
    }

    private IEnumerator ScaleOutUi(GameObject UiComp)
    {
        float currentScale = UiComp.transform.localScale.x;
        while (!Mathf.Approximately(UiComp.transform.localScale.x, 0f))
        {
            currentScale = Mathf.MoveTowards(currentScale, 0f, scaleRate * Time.deltaTime);
            UiComp.transform.localScale = new Vector3(currentScale, 1f, 1f);
            yield return null; //wait a frame
        }
    }
    #endregion

    #region Updating Load Screen Ui

    private void Update()
    {
        if (SceneManager.instance.ReturnIsLoading())
        {
            LoadingBar.fillAmount = SceneManager.instance.ReturnLoadProgress();
            print(SceneManager.instance.ReturnLoadProgress());
        }
    }

    #endregion
}
