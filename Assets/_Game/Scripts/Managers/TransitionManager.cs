using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TransitionManager : PersistentSingleton<TransitionManager>
{
    private TransitionPanel transitionPanel;

    protected override void Initialize()
    {
        transitionPanel = GetComponentInChildren<TransitionPanel>(true);
    }

    public void ShowTransition(System.Action onTransitionShown = null)
    {
        transitionPanel.ShowPanel();
        StartCoroutine(TransitionIE(onTransitionShown));
    }

    public void HideTransition(System.Action onTransitionHidden = null)
    {
        transitionPanel.Close();
        StartCoroutine(TransitionIE(onTransitionHidden));
    }

    private IEnumerator TransitionIE(System.Action callback)
    {
        yield return new WaitForSecondsRealtime(1);

        callback?.Invoke();
    }
}
