using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : PersistentSingleton<LobbyManager>
{
    public delegate void CharacterEvent(int playerID, string characterName);
    public event CharacterEvent OnCharacterSelected;

    private void Start()
    {
        if (ServerManager.Instance.IsOnlineMatch)
        {
            if (SteamMatchMakingManager.Instance.IsHost)
                NetworkManager.Instance.StartHost();
            else
                CreateClient();
        }
        else
        {
            NetworkManager.Instance.StartHostLANOnly();
        }
    }

    private void CreateClient()
    {
        uint ip = 0;
        ushort port = 0;
        Steamworks.SteamId serverID = new Steamworks.SteamId();

        SteamMatchMakingManager.Instance.CurrentLobby.GetGameServer(ref ip, ref port, ref serverID);

        NetworkManager.Instance.networkAddress = ip.ToString();
        NetworkManager.Instance.networkPort = port;

        NetworkManager.Instance.StartClient();
    }

    public void CharacterSelected(int playerID, string characterName)
    {
        foreach(var player in ServerManager.Instance.Players)
        {
            if (player.Figher == characterName)
                return;
        }

        if (ServerManager.Instance.IsOnlineMatch && playerID == NetworkManager.Instance.RoomPlayer.index)
        {
            NetworkManager.Instance.RoomPlayer.CmdChangeReadyState(true);
            CursorManager.Instance.HideAllCursors();
        }

        ServerManager.Instance.GetPlayer(playerID).Figher = characterName;
        OnCharacterSelected?.Invoke(playerID, characterName);
    }
}
