using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamManagerCustom : PersistentSingleton<SteamManagerCustom>
{

    protected override void Initialize()
    {
        SteamClient.Init(1359350);
    }
}
