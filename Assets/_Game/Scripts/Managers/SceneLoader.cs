using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : PersistentSingleton<SceneLoader>
{
    public void LoadScene(string sceneName, System.Action onSceneLoaded = null)
    {
        TransitionManager.Instance.ShowTransition(() =>
        {
            StartCoroutine(LoadSceneAsync(sceneName, () =>
            {
                onSceneLoaded?.Invoke();
                TransitionManager.Instance.HideTransition();
            }));
        });
    }

    private IEnumerator LoadSceneAsync(string sceneName, System.Action onSceneLoaded)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        onSceneLoaded.Invoke();
    }
}
