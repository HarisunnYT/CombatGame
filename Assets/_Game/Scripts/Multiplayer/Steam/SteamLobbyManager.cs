using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

public class SteamLobbyManager : Singleton<SteamLobbyManager>
{
    //private const int maxLobbyMembers = 4;

    //public Lobby? CurrentLobby { get; private set; }

    //private Task<Lobby?> creatingLobbyTask;

    //public const string PrivateLobbyStatedKey = "private_lobby_started";

    //protected override void Initialize()
    //{
    //    SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
    //    SteamMatchmaking.OnLobbyEntered += OnLobbyJoined;
    //    SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    //}

    //protected override void Deinitialize()
    //{
    //    base.Deinitialize();
    //}

    //private void OnLobbyJoined(Lobby lobby)
    //{
    //    CurrentLobby = lobby;
    //}

    //public void CreateLobby()
    //{
    //    creatingLobbyTask = SteamMatchmaking.CreateLobbyAsync(maxLobbyMembers);
    //}

    //private void OnLobbyCreated(Lobby? lobby)
    //{
    //    CurrentLobby = lobby;
    //    if (CurrentLobby != null)
    //    {
    //        SteamMatchMakingManager.Instance.IsHost = true;
    //        CurrentLobby.Value.SetFriendsOnly();
    //        CurrentLobby.Value.SetFriendsOnly();
    //        Debug.Log("Successfully created lobby");
    //    }
    //}

    //private void Update()
    //{
    //    if (creatingLobbyTask != null && creatingLobbyTask.IsCompleted)
    //    {
    //        OnLobbyCreated(creatingLobbyTask.Result);
    //        creatingLobbyTask = null;
    //    }
    //    else if (creatingLobbyTask != null && creatingLobbyTask.IsFaulted)
    //    {
    //        Debug.Log("Failed to create lobby");
    //    }
    //}

    //private void OnLobbyInvite(Friend arg1, Lobby arg2)
    //{
    //    //TODO show invite prompt in-game
    //}

    ////this is when the user accepts an invite or clicks 'join game' in steam UI
    //private void OnGameLobbyJoinRequested(Lobby lobby, SteamId friendID)
    //{
    //    CurrentLobby = lobby;
    //    SceneLoader.Instance.LoadScene("Lobby");
    //    Debug.Log("joined friends game");
    //}
}
