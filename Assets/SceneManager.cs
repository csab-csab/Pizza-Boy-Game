using System.Collections;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
   public static SceneManager instance;

    private bool isFirstStart = true;
    private bool isLoading = false;

    private float loadProgress = 0f;

    private void Awake()
    {
        if(instance != null)
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
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneIndex);

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
    #endregion
}
