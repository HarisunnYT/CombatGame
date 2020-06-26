using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using UnityEngine;

public class NetworkManager : NetworkRoomManager
{
    public static NetworkManager Instance;

    public CustomNetworkRoomPlayer RoomPlayer { get; set; }

    public delegate void ConnectionEvent(NetworkConnection conn);
    public event ConnectionEvent OnClientEnteredRoom;
    public event ConnectionEvent OnClientExitedRoom;

    public override void Awake()
    {
        base.Awake();

        Instance = this;
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        if (sceneName.Contains("Game.unity"))
        {
            MatchManager.Instance.BeginMatch();
        }
    }

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnection conn, GameObject roomPlayer)
    {
        PlayerController player = Instantiate(playerPrefab).GetComponent<PlayerController>();
        player.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);

        return player.gameObject;
    }

    public override void OnClientEnterRoom(NetworkConnection conn)
    {
        OnClientEnteredRoom?.Invoke(conn);
    }

    public override void OnClientExitRoom(NetworkConnection conn)
    {
        OnClientExitedRoom?.Invoke(conn);
    }
}
