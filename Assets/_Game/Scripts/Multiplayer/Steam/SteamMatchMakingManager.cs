﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System.Threading.Tasks;
using Mirror;

public class SteamMatchMakingManager : PersistentSingleton<SteamMatchMakingManager>
{
    public bool IsHost { get; set; }
    public Lobby CurrentMatchMakingLobby { get; private set; }

    private Task<Lobby[]> retrievingLobbiesTask;
    private Task<Lobby?> joiningLobbyTask;

    public void SearchForMatch(int players = 1)
    {
        //first we need to get a list of servers, if any are available
        LobbyQuery lobbyQuery = SteamMatchmaking.LobbyList.WithSlotsAvailable(1); //we can assign rules to this query
        retrievingLobbiesTask = lobbyQuery.RequestAsync();
    }

    private void JoinMatchMakingLobby(Lobby lobby)
    {
        joiningLobbyTask = SteamMatchmaking.JoinLobbyAsync(lobby.Id);
    }

    private void OnLobbyJoined(Lobby lobby)
    {
        CurrentMatchMakingLobby = lobby;
        SceneLoader.Instance.LoadScene("Lobby");
    }

    private void Update()
    {
        if (retrievingLobbiesTask != null && retrievingLobbiesTask.IsCompleted)
        {
            //we found at least one suitable lobby
            if (retrievingLobbiesTask.Result != null && retrievingLobbiesTask.Result.Length > 0)
            {
                //just in case a lobby is owned by this player and wasn't terminated correctly
                foreach(var lobby in retrievingLobbiesTask.Result)
                {
                    if (lobby.IsOwnedBy(SteamClient.SteamId))
                        lobby.Leave();
                }

                IsHost = false;
                JoinMatchMakingLobby(retrievingLobbiesTask.Result[0]);
            }
            else //no good lobbies, let's create one
            {
                IsHost = true;
                OnLobbyJoined(SteamLobbyManager.Instance.CurrentLobby.Value);
                SteamLobbyManager.Instance.CurrentLobby.Value.SetPublic();
                Debug.Log("Lobby Created");
            }

            retrievingLobbiesTask = null;
        }

        if (joiningLobbyTask != null && joiningLobbyTask.IsCompleted)
        {
            Debug.Log("Lobby Joined");
            OnLobbyJoined(joiningLobbyTask.Result.Value);
            joiningLobbyTask = null;
        }
    }
}
