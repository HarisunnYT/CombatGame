using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : NetworkRoomManager
{
    private List<NetworkConnection> currentConnectedPlayers = new List<NetworkConnection>();

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        if (sceneName == "Game")
        {
            MatchManager.Instance.BeginMatch();
        }
    }

    public override void OnRoomServerAddPlayer(NetworkConnection conn)
    {
        base.OnRoomServerAddPlayer(conn);

        //Transform startPos = GetStartPosition();
        //GameObject player = startPos != null
        //    ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
        //    : Instantiate(playerPrefab);

        //player.SetActive(false);

        //currentConnectedPlayers.Add(conn);

        //NetworkServer.AddPlayerForConnection(conn, player);
    }

    public override void OnRoomServerDisconnect(NetworkConnection conn)
    {
        base.OnRoomServerDisconnect(conn);

        currentConnectedPlayers.Add(conn);
    }

    public void SpawnPlayers()
    {
        foreach(var obj in currentConnectedPlayers)
        {
            obj.identity.gameObject.SetActive(true);
        }
    }
}
