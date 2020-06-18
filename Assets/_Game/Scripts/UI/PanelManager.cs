using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : Singleton<PanelManager>
{
    private Panel[] panels;

    private int timeScaleCounter = 0;

    protected override void Initialize()
    {
        panels = transform.GetComponentsInChildren<Panel>(true);
    }

    public void ShowPanel<T>() where T : Panel
    {
        GetPanel<T>().ShowPanel();
    }

    public T GetPanel<T>() where T : Panel
    {
        foreach(var panel in panels)
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
            if (leaveOpenPanel == null || panel != leaveOpenPanel)
            {
                panel.Close();
            }
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
}
