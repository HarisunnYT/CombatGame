using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TransitionManager : PersistentSingleton<TransitionManager>
{
    private TransitionPanel transitionPanel;

    private bool transitionShown = false;

    protected override void Initialize()
    {
        transitionPanel = GetComponentInChildren<TransitionPanel>(true);
    }

    public void ShowTransition(System.Action onTransitionShown = null)
    {
        if (transitionShown)
        {
            onTransitionShown?.Invoke();
        }
        else
        {
            transitionShown = true;
            transitionPanel.ShowPanel();
            StartCoroutine(TransitionIE(onTransitionShown));
        }
    }

    public void HideTransition(System.Action onTransitionHidden = null)
    {
        if (!transitionShown)
        {
            onTransitionHidden?.Invoke();
        }
        else
        {
            transitionShown = false;
            transitionPanel.Close();
            StartCoroutine(TransitionIE(onTransitionHidden));
        }
    }

    private IEnumerator TransitionIE(System.Action callback)
    {
        yield return new WaitForSecondsRealtime(1);

        callback?.Invoke();
    }
}
