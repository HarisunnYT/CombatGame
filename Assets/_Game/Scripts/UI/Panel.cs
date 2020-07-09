﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour, IAnimationHandler
{
    [System.Serializable]
    public enum PanelType
    {
        Normal,
        Modal,
        LeaveOut
    }

    [SerializeField]
    private PanelType panelType;
    public PanelType PanelOpenType { get { return panelType; } }

    [SerializeField]
    private bool pauseTime = false;
    public bool PauseTime { get { return pauseTime; } }

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

    private PanelManager panelManager;

    public virtual void Initialise() { }

    public void Close()
    {
        PanelManager.Instance.PanelClosed(this);

        if (animator && gameObject.activeSelf)
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

    public void ObjectDisabled()
    {
        gameObject.SetActive(false);
        OnClose();
    }

    public void ShowPanel()
    {
        if (animator && gameObject.activeSelf)
        {
            animator.SetTrigger("Open");
        }

        gameObject.SetActive(true);

        if (panelManager == null)
            panelManager = GetComponentInParent<PanelManager>();

        if (panelManager != null)
        {
            panelManager.PanelShown(this);

            if (panelType != PanelType.Modal)
                panelManager.CloseAllPanels(this);
        }

        if (pauseTime)
        {
            Time.timeScale = 0;
        }

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
