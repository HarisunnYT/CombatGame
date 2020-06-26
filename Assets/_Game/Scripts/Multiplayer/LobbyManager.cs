using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : PersistentSingleton<LobbyManager>
{
    public delegate void CharacterEvent(int playerID, string characterName);
    public event CharacterEvent OnCharacterSelected;

    public delegate void PlayerControllerEvent(int playerID, PlayerController playerController);
    public event PlayerControllerEvent OnPlayerCreated;

    private void Start()
    {
        if (!ServerManager.Instance.IsServer)
        {
            if (ServerManager.Instance.IsOnlineMatch)
                CreateClient();
            else
                NetworkManager.Instance.StartHost();
        }
    }

    private void CreateClient()
    {
        NetworkManager.Instance.networkAddress = "localhost";
        NetworkManager.Instance.StartClient();
    }

    public void CharacterSelected(int playerID, string characterName)
    {
        foreach(var player in ServerManager.Instance.Players)
        {
            if (player.Figher == characterName)
                return;
        }

        if (!ServerManager.Instance.IsServer && ServerManager.Instance.IsOnlineMatch && playerID == NetworkManager.Instance.RoomPlayer.index)
        {
            NetworkManager.Instance.RoomPlayer.CmdChangeReadyState(true);
            CursorManager.Instance.HideAllCursors();
        }

        ServerManager.Instance.GetPlayer(playerID).Figher = characterName;
        OnCharacterSelected?.Invoke(playerID, characterName);
    }

    public void PlayerCreated(int playerID, PlayerController player)
    {
        OnPlayerCreated?.Invoke(playerID, player);
    }
}
