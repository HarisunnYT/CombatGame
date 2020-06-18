using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : NetworkRoomManager
{
    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        MatchManager.Instance.BeginMatch();
    }
}
