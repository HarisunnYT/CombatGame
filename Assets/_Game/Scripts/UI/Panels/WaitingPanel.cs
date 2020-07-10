using Mirror.FizzySteam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingPanel : Panel
{
    private float targetTime = -1;

    protected override void OnShow()
    {
        targetTime = Time.time + FizzySteamworks.Instance.Timeout;
    }

    public void Cancel()
    {
        ExitManager.Instance.ExitMatch(ExitType.Leave);
    }

    private void Update()
    {
        if (targetTime != -1 && Time.time > targetTime)
            ExitManager.Instance.ExitMatch(ExitType.Leave);
    }
}
