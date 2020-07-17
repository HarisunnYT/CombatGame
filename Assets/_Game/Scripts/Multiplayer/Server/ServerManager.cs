using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerManager : PersistentSingleton<ServerManager>
{
    public class ConnectedPlayer
    {
        public int PlayerID;
        public int NetID;
        public string Name;
        public string Figher;
        public PlayerController PlayerController;
        public System.Guid ControllerGUID;
        public string SteamId;

        public ConnectedPlayer(int playerID, string name, string steamId)
        {
            PlayerID = playerID;
            Name = name;
            SteamId = steamId;

            Figher = "";
            PlayerController = null;
            ControllerGUID = default;
        }
    }
    public bool IsOnlineMatch { get; set; }

    public List<ConnectedPlayer> Players { get; private set; } = new List<ConnectedPlayer>();
    private List<string> SelectedCharacters = new List<string>();

    public delegate void PlayerEvent(ConnectedPlayer connectedPlayer);
    public event PlayerEvent OnPlayerAdded;
    public event PlayerEvent OnPlayerRemoved;

    public ConnectedPlayer AddConnectedPlayer(int id, string playerName, string steamId)
    {
        if (GetPlayer(id) != null)
            return GetPlayer(id);

        ConnectedPlayer player = new ConnectedPlayer(id, playerName, steamId);
        Players.Add(player);

        OnPlayerAdded?.Invoke(player);

        return player;
    }

    public void RemovePlayer(int id)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].PlayerID == id)
            {
                OnPlayerRemoved?.Invoke(Players[i]);
                Players.RemoveAt(i);

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

    public ConnectedPlayer GetPlayer(uint netId)
    {
        foreach (var player in Players)
        {
            if (player.NetID == netId)
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
        return GetPlayer(id).Name;
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

    public string GetRandomUnselectedCharacter()
    {
        while (true)
        {
            FighterData fighter = FighterManager.Instance.GetRandomFighter();
            if (!IsCharacterSelected(fighter.FighterName))
            {
                return fighter.FighterName;
            }
        }
    }

    public int GetTick()
    {
        return Time.frameCount; 
    }
}
