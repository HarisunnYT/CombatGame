using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreYouSurePanel : Panel
{
    private System.Action confirmAction;
    private Panel fromPanel;

    public void Configure(Panel fromPanel, System.Action confirmAction)
    {
        this.confirmAction = confirmAction;
        this.fromPanel = fromPanel;

        ShowPanel();
    }

    public void Confirm()
    {
        confirmAction?.Invoke();
        
        fromPanel.Close();
        Close();
    }

    public void Cancel()
    {
        Close();
    }
}
