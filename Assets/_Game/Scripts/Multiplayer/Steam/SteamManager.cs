using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SteamManager : PersistentSingleton<SteamManager>
{
    protected override void Initialize()
    {
        try
        {
            SteamClient.Init(1359350);
        }
        catch (System.Exception e)
        {

        }
    }

    private void Update()
    {
        SteamClient.RunCallbacks();
    }

    protected override void Deinitialize()
    {
        SteamClient.Shutdown();
    }
}
