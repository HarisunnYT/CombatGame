using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : PersistentSingleton<LobbyManager>
{
    public delegate void CharacterEvent(uint playerID, string characterName);
    public event CharacterEvent OnCharacterSelected;

    public delegate void PlayerControllerEvent(uint playerID, PlayerController playerController);
    public event PlayerControllerEvent OnPlayerCreated;

    //for both online and local
    public List<uint> Players { get; set; } = new List<uint>();

    private void Start()
    {
        if (!ServerManager.Instance.IsServer && PlayFabMatchMaking.Instance)
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

    public void CharacterSelected(uint playerID, string characterName)
    {
        if (Players.Contains(playerID))
            return;

        if (ServerManager.Instance.IsOnlineMatch && playerID == NetworkManager.Instance.RoomPlayer.netId - 1)
        {
            NetworkManager.Instance.RoomPlayer.CmdChangeReadyState(true);
            CursorManager.Instance.HideAllCursors();
        }

        Players.Add(playerID);

        FighterManager.Instance.FighterSelected(playerID, characterName);

        OnCharacterSelected?.Invoke(playerID, characterName);
    }

    public void PlayerCreated(uint playerID, PlayerController player)
    {
        OnPlayerCreated?.Invoke(playerID, player);
    }
}
