using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.ComponentModel.Design;
using PlayFab.MultiplayerModels;
using Steamworks;

public class ServerManager : PersistentSingleton<ServerManager>
{
    public class ConnectedPlayer
    {
        public int PlayerID;
        public uint SteamClientID;
        public string Figher;
        public PlayerController PlayerController;
        public System.Guid ControllerGUID;

        public ConnectedPlayer(int playerID, uint steamClientID)
        {
            PlayerID = playerID;
            SteamClientID = steamClientID;

            Figher = "";
            PlayerController = null;
            ControllerGUID = default;
        }
    }

    [SerializeField]
    private bool debugServer = false;

    public bool IsOnlineMatch { get; set; } = false;

    public bool IsServer { get; private set; }

    public List<ConnectedPlayer> Players { get; private set; } = new List<ConnectedPlayer>();
    private List<string> SelectedCharacters = new List<string>();

    protected override void Initialize()
    {
        IsServer = SystemInfo.graphicsDeviceID == 0;
        if (IsServer)
        {
            IsOnlineMatch = true;
        }
#if UNITY_EDITOR
        IsServer = debugServer;
#endif
    }

    private void Start()
    {
        if (IsServer)
        {
            StartUpServer();
        }
    }

    private void StartUpServer()
    {
        SceneManager.LoadScene("Lobby");
        NetworkManager.singleton.StartServer();
        Debug.Log("SERVER SET UP");
    }

    public void OnPlayerDisconnectedFromServer()
    {
        if (Players.Count <= 0)
        {
            NetworkManager.singleton.StopServer();
            Application.Quit();
        }
    }

    public ConnectedPlayer AddConnectedPlayer(int id, uint steamClientID)
    {
        if (GetPlayer(id) != null)
            return GetPlayer(id);

        ConnectedPlayer player = new ConnectedPlayer(id, steamClientID);
        Players.Add(player);

        return player;
    }

    public void RemovePlayer(int id)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].PlayerID == id)
            {
                Players.RemoveAt(i);
                OnPlayerDisconnectedFromServer();

                break;
            }
        }
    }

    public void RemovePlayer(NetworkConnection conn)
    {
        for (int i = 0; i < NetworkManager.Instance.roomSlots.Count; i++)
        {
            if (NetworkManager.Instance.roomSlots[i].connectionToClient == conn)
            {
                NetworkManager.Instance.roomSlots.RemoveAt(i);
                OnPlayerDisconnectedFromServer();

                break;
            }
        }
    }

    public ConnectedPlayer GetPlayer(int id)
    {
        foreach(var player in Players)
        {
            if (player.PlayerID == id)
            {
                return player;
            }
        }

        return default;
    }

    public ConnectedPlayer GetPlayer(PlayerController playerController)
    {
        foreach (var player in Players)
        {
            if (player.PlayerController == playerController)
            {
                return player;
            }
        }

        return default;
    }

    public ConnectedPlayer GetPlayer(System.Guid id)
    {
        foreach (var player in Players)
        {
            if (player.ControllerGUID == id)
            {
                return player;
            }
        }

        return default;
    }

    public ConnectedPlayer GetPlayerLocal()
    {
        foreach (var player in Players)
        {
            if (player.PlayerID == NetworkManager.Instance.RoomPlayer.index)
            {
                return player;
            }
        }

        return default;
    }

    public bool ContainsPlayer(int id)
    {
        foreach (var player in Players)
        {
            if (player.PlayerID == id)
            {
                return true;
            }
        }

        return false;
    }

    public bool ContainsPlayer(System.Guid id)
    {
        foreach (var player in Players)
        {
            if (player.ControllerGUID == id)
            {
                return true;
            }
        }

        return false;
    }

    public string GetPlayerName(int id)
    {
        return "Player";
    }

    public void SetCharacterSelected(string characterName)
    {
        SelectedCharacters.Add(characterName);
    }

    public void SetCharacterUnselected(string characterName)
    {
        SelectedCharacters.Remove(characterName);
    }

    public bool IsCharacterSelected(string characterName)
    {
        return SelectedCharacters.Contains(characterName);
    }

    public int GetTick()
    {
        return Time.frameCount; 
    }
}
