using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : NetworkRoomManager
{
    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        if (sceneName == "Game")
        {
            MatchManager.Instance.BeginMatch();
            if (ServerManager.Instance.IsServer)
            {
                ServerManager.Instance.BeganMatch();
            }
        }
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);

        ServerManager.Instance.OnPlayerConnectedToServer(conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);

        ServerManager.Instance.OnPlayerDisconnectedFromServer(conn);
    }
}
