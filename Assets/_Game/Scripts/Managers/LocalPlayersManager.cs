using InControl;
using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalPlayersManager : PersistentSingleton<LocalPlayersManager>
{
    public delegate void LocalPlayerConnectionEvent(int playerIndex, System.Guid controllerGUID);
    public event LocalPlayerConnectionEvent OnLocalPlayerConnected;

    private List<int> localPlayersReady = new List<int>();

    public int LocalPlayersCount { get; private set; } = 1;

    //used for assigning an index to local players
    public int PlayerIndexForAssigning { get; set; } = 0;

    private void Update()
    {
        if (!ServerManager.Instance.IsOnlineMatch && SceneManager.GetActiveScene().name == "Lobby")
        {
            foreach (var device in InputManager.ActiveDevices)
            {
                if (device.CommandWasPressed && !HasLocalPlayerJoinedAlready(device.GUID))
                {
                    LocalPlayerJoined(LocalPlayersCount, device.GUID);

                    break;
                }
            }
        }
    }

    public void LocalPlayerJoined(int playerIndex, System.Guid controllerID)
    {
        ServerManager.Instance.AddConnectedPlayer(playerIndex, "Player " + (playerIndex + 1), SteamClient.SteamId.Value.ToString()).ControllerGUID = controllerID;
        CursorManager.Instance.GetCursor(0).InputProfile.RemoveController(controllerID); //remove this controller from player 1s controllers list
        if (CursorManager.Instance.GetCursor(0).InputProfile.IncludeDevices.Count == 0)
            CursorManager.Instance.GetCursor(0).AssignDevice(0, default); //reset device so it resets bindings

        OnLocalPlayerConnected?.Invoke(playerIndex, controllerID);
        LocalPlayersCount++;
    }

    public bool HasLocalPlayerJoinedAlready(System.Guid controllerID)
    {
        return ServerManager.Instance.ContainsPlayer(controllerID);
    }

    public bool HasLocalPlayerJoinedAlready(int playerIndex)
    {
        return ServerManager.Instance.ContainsPlayer(playerIndex);
    }

    public int GetPlayerIndexFromController(System.Guid controllerID)
    {
        return ServerManager.Instance.GetPlayer(controllerID).PlayerID;
    }

    public System.Guid GetGUIDFromPlayerIndex(int playerID)
    {
        return ServerManager.Instance.GetPlayer(playerID).ControllerGUID;
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

    public void LocalPlayerUnreadiedUp(int playerIndex)
    {
        if (playerIndex == -1)
            return;

        if (localPlayersReady.Contains(playerIndex))
        {
            localPlayersReady.Remove(playerIndex);
        }
    }

    public void LocalPlayerUnreadiedUp(System.Guid controllerID)
    {
        LocalPlayerUnreadiedUp(GetPlayerIndexFromController(controllerID));
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
