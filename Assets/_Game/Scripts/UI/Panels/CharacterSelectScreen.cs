using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectScreen : Panel
{
    [SerializeField]
    private ConnectPlayerCell[] connectedPlayerCells;

    [SerializeField]
    private CharacterCell[] characterCells;

    private void Awake()
    {
        if (ServerManager.Instance)
        {
            ServerManager.Instance.OnPlayerAdded += AddPlayerCell;
            ServerManager.Instance.OnPlayerRemoved += RemovePlayerCell;
        }

        if (LobbyManager.Instance)
            LobbyManager.Instance.OnCharacterSelected += OnCharacterSelected;

        if (LocalPlayersManager.Instance)
            LocalPlayersManager.Instance.OnLocalPlayerConnected += OnLocalPlayerConnected;

    }

    private void OnDestroy()
    {
        if (ServerManager.Instance)
        {
            ServerManager.Instance.OnPlayerAdded -= AddPlayerCell;
            ServerManager.Instance.OnPlayerRemoved -= RemovePlayerCell;
        }

        if (LobbyManager.Instance)
            LobbyManager.Instance.OnCharacterSelected -= OnCharacterSelected;

        if (LocalPlayersManager.Instance)
            LocalPlayersManager.Instance.OnLocalPlayerConnected -= OnLocalPlayerConnected;
    }

    protected override void OnShow()
    {
        base.OnShow();

        //disable all cells first
        foreach (var cell in connectedPlayerCells)
        {
            cell.gameObject.SetActive(false);
        }

        if (ServerManager.Instance.IsOnlineMatch)
        {
            for (int i = 0; i < ServerManager.Instance.Players.Count; i++)
            {
                connectedPlayerCells[i].Configure(ServerManager.Instance.Players[i].PlayerID, ServerManager.Instance.Players[i].SteamName);
            }
        }
    }

    private void AddPlayerCell(ServerManager.ConnectedPlayer player)
    {
        for (int i = 0; i < connectedPlayerCells.Length; i++)
        {
            if (!connectedPlayerCells[i].Assigned)
            {
                string name = ServerManager.Instance.IsOnlineMatch ? player.SteamName : "Player " + (i + 1);
                connectedPlayerCells[i].Configure(player.PlayerID, name); 
                break;
            }
        }
    }

    private void RemovePlayerCell(ServerManager.ConnectedPlayer player)
    {
        for (int i = 0; i < connectedPlayerCells.Length; i++)
        {
            if (connectedPlayerCells[i].Assigned && connectedPlayerCells[i].PlayerID == player.PlayerID)
            {
                connectedPlayerCells[i].DisableCell();
                break;
            }
        }
    }

    private void OnCharacterSelected(int playerID, string characterName)
    {
        foreach(var characterCell in characterCells)
        {
            if (characterCell.CharacterName == characterName)
            {
                characterCell.SetCharacterSelected(true);
            }
        }
    }

    private void OnLocalPlayerConnected(int playerIndex, System.Guid controllerGUID)
    {
        connectedPlayerCells[playerIndex].Configure("Player " + (playerIndex + 1)); //TODO show proper name
    }

    public void Cancel()
    {
        LobbyManager.Instance.ExitLobby();
    }
}
