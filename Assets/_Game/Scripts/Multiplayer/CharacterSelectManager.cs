﻿using Mirror;
using Mirror.FizzySteam;
using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterSelectManager : Singleton<CharacterSelectManager>
{
    [SerializeField]
    private int characterSelectTime = 20;
    public int CharacterSelectTime { get { return characterSelectTime; } }

    public delegate void CharacterEvent(int playerID, string characterName);
    public event CharacterEvent OnCharacterSelected;
    public event CharacterEvent OnCharacterUnselected;

    private bool matchLoaded = false;

    private void Start()
    {
        if (ServerManager.Instance.IsOnlineMatch)
        {
            if (!SteamLobbyManager.Instance.IsPrivateMatch && !SteamLobbyManager.Instance.PublicHost)
                NetworkManager.Instance.RoomPlayer.CmdRequestTimer();
        }
        else
            NetworkManager.Instance.StartHost();

        if (SteamLobbyManager.Instance.PrivateLobby.HasValue)
            SteamLobbyManager.Instance.ClearPrivateLobbyData();
    }

    private void Update()
    {
        if (!matchLoaded && (!ServerManager.Instance.IsOnlineMatch || ServerManager.Instance.Players.Count >= SteamLobbyManager.Instance.PublicLobby.Value.MemberCount))
        {
            PanelManager.Instance.ShowPanel<CharacterSelectScreen>();
            matchLoaded = true;
        }
    }

    public void CharacterSelected(int playerID, string characterName)
    {
        foreach(var player in ServerManager.Instance.Players)
        {
            if (player.Fighter == characterName)
                return;
        }

        if (ServerManager.Instance.IsOnlineMatch && playerID == NetworkManager.Instance.RoomPlayer.index)
        {
            NetworkManager.Instance.RoomPlayer.CmdChangeReadyState(true);
            CursorManager.Instance.HideAllCursors();
        }

        ServerManager.Instance.GetPlayer(playerID).Fighter = characterName;
        OnCharacterSelected?.Invoke(playerID, characterName);
    }

    public void CharacterUnselected(int playerID, string characterName)
    {
        if (ServerManager.Instance.IsOnlineMatch && playerID == NetworkManager.Instance.RoomPlayer.index)
            NetworkManager.Instance.RoomPlayer.CmdChangeReadyState(false);

        ServerManager.Instance.GetPlayer(playerID).Fighter = "";
        OnCharacterUnselected?.Invoke(playerID, characterName);
    }
}
