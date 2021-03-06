﻿using Mirror.FizzySteam;
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
        ExitManager.Instance.ExitMatch(SteamLobbyManager.Instance.IsPrivateMatch ? ExitType.HostLeftWithParty : ExitType.Leave);
    }
}
