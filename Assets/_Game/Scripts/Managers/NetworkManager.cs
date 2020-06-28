using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using UnityEngine;

public class NetworkManager : NobleRoomManager
{
    public static NetworkManager Instance;

    public CustomNetworkRoomPlayer RoomPlayer { get; set; }

    private Dictionary<int, int> playerData = new Dictionary<int, int>();

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
        return player.gameObject;
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        CustomNetworkRoomPlayer customRoomPlayer = roomPlayer.GetComponent<CustomNetworkRoomPlayer>();
        PlayerController playerController = gamePlayer.GetComponent<PlayerController>();

        StartCoroutine(FrameDelayForID(customRoomPlayer, playerController.netIdentity, customRoomPlayer.index));

        return true;
    }

    //we need to wait a frame so the net id can be assigned
    private IEnumerator FrameDelayForID(CustomNetworkRoomPlayer roomPlayer, NetworkIdentity netIdentity, int index)
    {
        while (netIdentity.netId == 0)
        {
            yield return new WaitForEndOfFrame();
        }

        playerData.Add((int)netIdentity.netId, index);
        if (playerData.Count >= ServerManager.Instance.Players.Count)
        {
            roomPlayer.CmdAssignPlayerID(playerData.Keys.ToArray(), playerData.Values.ToArray());
            playerData.Clear();
        }
    }

    public override void OnServerPrepared(string hostAddress, ushort hostPort)
    {
        if (ServerManager.Instance.IsOnlineMatch)
            SteamMatchMakingManager.Instance.SetGameServer(hostAddress, hostPort);

        PanelManager.Instance.ShowPanel<CharacterSelectScreen>();
    }

    public override void OnRoomClientConnect(NetworkConnection conn)
    {
        base.OnRoomClientConnect(conn);

        if (!SteamMatchMakingManager.Instance.IsHost)
            PanelManager.Instance.ShowPanel<CharacterSelectScreen>();
    }

    public void ResetInstance()
    {
        playerData.Clear();
    }
}
