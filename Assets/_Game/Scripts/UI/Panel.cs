using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour, IAnimationHandler
{
    [System.Serializable]
    enum PanelType
    {
        Normal,
        Modal
    }

    [SerializeField]
    private PanelType panelType;

    [SerializeField]
    private bool pauseTime = false;
    public bool PauseTime { get { return pauseTime; } }

    [Header("Optional")]
    [SerializeField]
    public GameObject selectedButton;

    private Animator animatorGetter;
    private Animator animator
    {
        get
        {
            //lazy initialise
            if (animatorGetter == null)
                animatorGetter = GetComponent<Animator>();

            return animatorGetter;
        }
    }

    public void Close()
    {
        PanelManager.Instance.PanelClosed(this);

        if (animator)
        {
            animator.SetTrigger("Close");
        }
        else
        {
            ObjectDisabled();
        }
    }

    public void ForceClose()
    {
        PanelManager.Instance.PanelClosed(this);

        ObjectDisabled();
    }

    private void ObjectDisabled()
    {
        gameObject.SetActive(false);
        OnClose();
    }

    public void ShowPanel()
    {
        if (gameObject.activeSelf)
            return;

        if (animator && gameObject.activeSelf)
        {
            animator.SetTrigger("Open");
        }

        gameObject.SetActive(true);

        PanelManager.Instance.PanelShown(this);

        if (panelType != PanelType.Modal)
        {
            PanelManager.Instance.CloseAllPanels(this);
        }

        if (pauseTime)
        {
            Time.timeScale = 0;
        }

        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(selectedButton);

        OnShow();
    }

    protected virtual void OnClose() { }
    protected virtual void OnShow() { }

    public void OnAnimationBegin() { }

    public void OnAnimationComplete() 
    {
        ObjectDisabled();
    }
}
