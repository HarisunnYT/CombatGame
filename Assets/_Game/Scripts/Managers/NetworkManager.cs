using Mirror;
using Mirror.FizzySteam;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : NetworkRoomManager
{
    public static NetworkManager Instance;

    public CustomNetworkRoomPlayer RoomPlayer { get; set; }

    private Dictionary<int, int> playerData = new Dictionary<int, int>();

    public override void Awake()
    {
        base.Awake();

        Instance = this;
    }

    private void Update()
    {
        if (transport == null)
        {
            transport = FizzySteamworks.Instance;
            Transport.activeTransport = transport;
        }
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        if (sceneName.Contains("Game"))
            FinishedLoadingScene();
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);

        if (!ClientScene.localPlayer.isServer && SceneManager.GetActiveScene().name == "Game")
            FinishedLoadingScene();
    }

    private void FinishedLoadingScene()
    {
        TransitionManager.Instance.HideTransition(() =>
        {
            MatchManager.Instance.BeginMatch();
        });
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

    public override void OnServerError(NetworkConnection conn, int errorCode)
    {
        Debug.Log(errorCode);
        base.OnServerError(conn, errorCode);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        if (MatchManager.Instance)
            MatchManager.Instance.ExitMatch(true);
        else
            CharacterSelectManager.Instance.ExitLobby(true);
    }

    public int GetPrefabID(GameObject prefab)
    {
        for (int i = 0; i < spawnPrefabs.Count; i++)
        {
            if (spawnPrefabs[i] == prefab)
            {
                return i;
            }
        }

        return -1;
    }

    public GameObject GetPrefabFromID(int id)
    {
        if (id != -1)
            return spawnPrefabs[id];
        else
            throw new System.Exception("Object not added to network spawnables list");
    }

    public override void ChangeScene(string sceneName)
    {
        TransitionManager.Instance.ShowTransition(() =>
        {
            base.ChangeScene(sceneName);
        });
    }
}
