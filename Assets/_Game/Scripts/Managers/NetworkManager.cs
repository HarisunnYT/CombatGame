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

    public override GameObject OnServerAddPlayer(NetworkConnection conn)
    {
        GameObject player = base.OnServerAddPlayer(conn);
        OnPlayerCreated(conn, player.GetComponent<PlayerController>(), true);
        return player;
    }

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnection conn, GameObject roomPlayer)
    {
        PlayerController player = Instantiate(playerPrefab).GetComponent<PlayerController>();
        OnPlayerCreated(conn, player, true);
        return player.gameObject;
    }

    public void OnPlayerCreated(NetworkConnection conn, PlayerController player, bool isServer)
    {
        foreach (var slot in roomSlots)
        {
            if ((isServer && slot.connectionToClient == conn) || (!isServer && slot.connectionToServer == conn))
            {
                player.AssignID(slot.index);
                break;
            }
        }
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
