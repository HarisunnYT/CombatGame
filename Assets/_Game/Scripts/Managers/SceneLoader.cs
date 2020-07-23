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

    public static bool IsMainMenu
    {
        get
        {
            return SceneManager.GetActiveScene().name == "MainMenu";
        }
    }

    public static bool IsCharacterScreen
    {
        get
        {
            return SceneManager.GetActiveScene().name == "Lobby";
        }
    }

    public static bool IsGame
    {
        get
        {
            return SceneManager.GetActiveScene().name == "Game";
        }
    }
}
