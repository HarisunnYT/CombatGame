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
        yield return new WaitForEndOfFrame();

        roomPlayer.CmdAssignPlayerID((int)netIdentity.netId, index);
    }
}
