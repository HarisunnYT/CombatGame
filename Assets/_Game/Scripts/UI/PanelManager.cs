using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelManager : Singleton<PanelManager>
{
    [SerializeField]
    private Panel initialPanel;

    private Panel[] panels;

    public GraphicRaycaster Raycaster { get; private set; }

    private int timeScaleCounter = 0;

    protected override void Initialize()
    {
        panels = transform.GetComponentsInChildren<Panel>(true);
        Raycaster = GetComponent<GraphicRaycaster>();

        initialPanel?.ShowPanel();

        foreach(var panel in panels)
        {
            panel.Initialise();
        }
    }

    public void ShowPanel<T>() where T : Panel
    {
        GetPanel<T>().ShowPanel();
    }

    public T GetPanel<T>() where T : Panel
    {
        foreach (var panel in panels)
        {
            if (panel is T)
                return panel as T;
        }

        return null;
    }

    public void ClosePanel<T>() where T : Panel
    {
        GetPanel<T>().Close();
    }

    public void ForceClosePanel<T>() where T : Panel
    {
        GetPanel<T>().ForceClose();
    }

    public void CloseAllPanels(Panel leaveOpenPanel = null)
    {
        foreach (var panel in panels)
        {
            if ((leaveOpenPanel == null || panel != leaveOpenPanel) && panel.PanelOpenType != Panel.PanelType.LeaveOut)
            {
                panel.Close();
            }
        }
    }

    public void ForceCloseAllPanels()
    {
        foreach (var panel in panels)
        {
            panel.Close();
        }
    }

    public void PanelShown(Panel panel)
    {
        if (panel.PauseTime)
        {
            timeScaleCounter++;

            Time.timeScale = 0;
        }
    }

    public void PanelClosed(Panel panel)
    {
        if (panel.PauseTime)
        {
            timeScaleCounter--;
            timeScaleCounter = Mathf.Clamp(timeScaleCounter, 0, int.MaxValue);

            if (timeScaleCounter <= 0)
            {
                Time.timeScale = 1;
            }
        }
    }

    public bool CanOpenPanel()
    {
        foreach (var panel in panels)
        {
            if (panel.gameObject.activeSelf && panel.PanelOpenType != Panel.PanelType.Modal && !(panel is HUDPanel))
            {
                return false;
            }
        }

        return true;
    }
}
