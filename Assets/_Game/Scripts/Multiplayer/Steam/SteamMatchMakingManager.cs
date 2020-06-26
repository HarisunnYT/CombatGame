using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System.Threading.Tasks;

public class SteamMatchMakingManager : Singleton<SteamMatchMakingManager>
{
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
        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        Debug.Log("Lobby Joined");
    }

    private void Update()
    {
        if (retrievingLobbiesTask != null && retrievingLobbiesTask.IsCompleted)
        {
            //we found at least one suitable lobby
            if (retrievingLobbiesTask.Result.Length > 0)
            {
                JoinMatchMakingLobby(retrievingLobbiesTask.Result[0]);
            }
            else //no good lobbies, let's create one
            {
                SteamLobbyManager.Instance.CurrentLobby.Value.SetPublic();
                OnLobbyJoined(SteamLobbyManager.Instance.CurrentLobby.Value);
            }

            retrievingLobbiesTask = null;
        }

        if (joiningLobbyTask != null && joiningLobbyTask.IsCompleted)
        {
            OnLobbyJoined(joiningLobbyTask.Result.Value);
            joiningLobbyTask = null;
        }
    }
}
