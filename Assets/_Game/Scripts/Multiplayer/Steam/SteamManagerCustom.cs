using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamManagerCustom : SteamManager
{
    private bool initialised = false;

    protected override void Initialize()
    {
        base.Initialize();
        
        if (!initialised)
            SteamClient.Init(1359350);

        initialised = true;
    }
}
