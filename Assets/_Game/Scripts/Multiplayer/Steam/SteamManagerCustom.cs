using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamManagerCustom : SteamManager
{
    protected override void Awake()
    {
        SteamClient.Init(1359350);
        base.Awake();
    }
}
