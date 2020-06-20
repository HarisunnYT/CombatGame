using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerManager : PersistentSingleton<ServerManager>
{
    public bool IsServer { get; private set; }

    protected override void Initialize()
    {
        IsServer = SystemInfo.graphicsDeviceID == 0;    
    }
}
