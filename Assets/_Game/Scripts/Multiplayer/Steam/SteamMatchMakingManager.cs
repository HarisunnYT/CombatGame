using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System.Threading.Tasks;
using Mirror;

public class SteamMatchMakingManager : PersistentSingleton<SteamMatchMakingManager>
{
    public bool IsHost { get; private set; }
    public Lobby CurrentLobby { get; private set; }

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
        CurrentLobby = lobby;
        SceneLoader.Instance.LoadScene("Lobby");
    }

    private void Update()
    {
        if (retrievingLobbiesTask != null && retrievingLobbiesTask.IsCompleted)
        {
            //we found at least one suitable lobby
            if (retrievingLobbiesTask.Result != null && retrievingLobbiesTask.Result.Length > 0)
            {
                IsHost = false;
                JoinMatchMakingLobby(retrievingLobbiesTask.Result[0]);
            }
            else //no good lobbies, let's create one
            {
                IsHost = true;
                OnLobbyJoined(SteamLobbyManager.Instance.CurrentLobby.Value);
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

    public void SetGameServer(string hostAddress, ushort hostPort)
    {
        CurrentLobby.SetGameServer(hostAddress, hostPort);
        CurrentLobby.SetPublic();
    }
}
