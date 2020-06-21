using InControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayersManager : PersistentSingleton<LocalPlayersManager>
{
    public delegate void LocalPlayerConnectionEvent(int playerIndex, System.Guid controllerGUID);
    public event LocalPlayerConnectionEvent OnLocalPlayerConnected;

    //player index and controller id
    private Dictionary<int, System.Guid> localPlayers = new Dictionary<int, System.Guid>();

    private List<int> localPlayersReady = new List<int>();

    public int LocalPlayers { get; private set; } = 1;

    public void LocalPlayerJoined(int playerIndex, System.Guid controllerID)
    {
        localPlayers.Add(playerIndex, controllerID);
        OnLocalPlayerConnected?.Invoke(playerIndex, controllerID);

        LocalPlayers++;
    }

    public bool HasLocalPlayerJoinedAlready(System.Guid controllerID)
    {
        return localPlayers.ContainsValue(controllerID);
    }

    public bool HasLocalPlayerJoinedAlready(int playerIndex)
    {
        return localPlayers.ContainsKey(playerIndex);
    }

    public int GetPlayerIndexFromController(System.Guid controllerID)
    {
        foreach(var player in localPlayers)
        {
            if (player.Value == controllerID)
            {
                return player.Key;
            }
        }

        return -1;
    }

    public void LocalPlayerReadiedUp(int playerIndex)
    {
        if (playerIndex == -1)
            return;

        if (!localPlayersReady.Contains(playerIndex))
        {
            localPlayersReady.Add(playerIndex);
        }

        if (localPlayersReady.Count >= LocalPlayers)
        {
            NetworkManager.Instance.RoomPlayer.CmdChangeReadyState(true);
        }
    }

    public void LocalPlayerReadiedUp(System.Guid controllerID)
    {
        LocalPlayerReadiedUp(GetPlayerIndexFromController(controllerID));
    }
}
