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
        if (NetworkManager.Instance)
        {
            NetworkManager.Instance.OnClientEnteredRoom += AddPlayerCell;
            NetworkManager.Instance.OnClientExitedRoom += RemovePlayerCell;
        }

        if (LobbyManager.Instance)
            LobbyManager.Instance.OnCharacterSelected += OnCharacterSelected;

        if (LocalPlayersManager.Instance)
            LocalPlayersManager.Instance.OnLocalPlayerConnected += OnLocalPlayerConnected;

    }

    private void OnDestroy()
    {
        if (NetworkManager.Instance)
        {
            NetworkManager.Instance.OnClientEnteredRoom -= AddPlayerCell;
            NetworkManager.Instance.OnClientExitedRoom -= RemovePlayerCell;
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
            for (int i = 0; i < NetworkManager.Instance.roomSlots.Count; i++)
            {
                NetworkRoomPlayer player = NetworkManager.Instance.roomSlots[i];
                connectedPlayerCells[i].Configure(player.connectionToServer, ServerManager.Instance.GetPlayerName(player.connectionToServer));
            }
        }
    }

    private void AddPlayerCell(NetworkConnection conn)
    {
        for (int i = 0; i < connectedPlayerCells.Length; i++)
        {
            if (!connectedPlayerCells[i].Assigned)
            {
                connectedPlayerCells[i].Configure(conn, "Player " + (i + 1)); //TODO GET PLAYER NAME
                break;
            }
        }
    }

    private void RemovePlayerCell(NetworkConnection conn)
    {
        for (int i = 0; i < connectedPlayerCells.Length; i++)
        {
            if (connectedPlayerCells[i].Assigned && connectedPlayerCells[i].Connection == conn)
            {
                connectedPlayerCells[i].DisableCell();
                break;
            }
        }
    }

    private void OnCharacterSelected(uint connectionID, string characterName)
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
}
