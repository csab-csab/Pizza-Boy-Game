using System;
using System.Collections;
using UnityEngine;
using Unity_SM = UnityEngine.SceneManagement; 

public class SceneManager : MonoBehaviour
{
   public static SceneManager instance{get; private set;}

    private bool isFirstStart = true;
    private bool isLoading = false;

    private float loadProgress = 0f;

  

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    
    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadSceneAsync(sceneIndex));
        isLoading = true;
    }

    IEnumerator LoadSceneAsync(int sceneIndex)
    {
        isLoading = true;
        loadProgress = 0f;

        AsyncOperation operation =
        Unity_SM.SceneManager.LoadSceneAsync(sceneIndex);

        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            loadProgress = operation.progress / 0.9f;
            yield return null;
        }

        loadProgress = 1f;

        // Force bar to be visible
        yield return new WaitForSeconds(0.5f);

        operation.allowSceneActivation = true;
        isLoading = false;
    }


   
    #region Return Values
    public bool ReturIsFirstStart()
    {
        return isFirstStart;
    }

    public bool ReturnIsLoading()
    {
        return isLoading;
    }

    public float ReturnLoadProgress()
    {
        return loadProgress;
    }
    public int ReturnCurrentSceneIndex()
    {
        Unity_SM.Scene scene = Unity_SM.SceneManager.GetActiveScene();
        return scene.buildIndex;
    }
    #endregion
}
