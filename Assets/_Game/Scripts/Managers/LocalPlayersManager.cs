using InControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayersManager : PersistentSingleton<LocalPlayersManager>
{
    public delegate void LocalPlayerConnectionEvent(int playerIndex, System.Guid controllerGUID);
    public event LocalPlayerConnectionEvent OnLocalPlayerConnected;

    //player index and controller id
    public Dictionary<int, System.Guid> LocalPlayers { get; private set; } = new Dictionary<int, System.Guid>();

    private List<int> localPlayersReady = new List<int>();

    public int LocalPlayersCount { get; private set; } = 1;

    public void LocalPlayerJoined(int playerIndex, System.Guid controllerID)
    {
        LocalPlayers.Add(playerIndex, controllerID);
        OnLocalPlayerConnected?.Invoke(playerIndex, controllerID);

        LocalPlayersCount++;
    }

    public bool HasLocalPlayerJoinedAlready(System.Guid controllerID)
    {
        return LocalPlayers.ContainsValue(controllerID);
    }

    public bool HasLocalPlayerJoinedAlready(int playerIndex)
    {
        return LocalPlayers.ContainsKey(playerIndex);
    }

    public int GetPlayerIndexFromController(System.Guid controllerID)
    {
        foreach(var player in LocalPlayers)
        {
            if (player.Value == controllerID)
            {
                return player.Key;
            }
        }

        return -1;
    }

    public System.Guid GetGUIDFromPlayerIndex(int playerIndex)
    {
        foreach (var player in LocalPlayers)
        {
            if (player.Key == playerIndex)
            {
                return player.Value;
            }
        }

        return default;
    }

    public void LocalPlayerReadiedUp(int playerIndex)
    {
        if (playerIndex == -1)
            return;

        if (!localPlayersReady.Contains(playerIndex))
        {
            localPlayersReady.Add(playerIndex);
        }

        if (localPlayersReady.Count >= LocalPlayersCount)
        {
            NetworkManager.Instance.RoomPlayer.CmdChangeReadyState(true);
        }
    }

    public void LocalPlayerReadiedUp(System.Guid controllerID)
    {
        LocalPlayerReadiedUp(GetPlayerIndexFromController(controllerID));
    }

    public bool HasLocalPlayerReadiedUp(int playerIndex)
    {
        return localPlayersReady.Contains(playerIndex);
    }

    public bool HasLocalPlayerReadiedUp(System.Guid controllerID)
    {
        return localPlayersReady.Contains(GetPlayerIndexFromController(controllerID));
    }
}
