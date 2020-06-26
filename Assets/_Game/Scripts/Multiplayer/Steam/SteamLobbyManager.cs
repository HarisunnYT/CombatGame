using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System.Threading.Tasks;

public class SteamLobbyManager : Singleton<SteamLobbyManager>
{
    private const int maxLobbyMembers = 4;

    public Lobby? CurrentLobby { get; private set; }

    private Task<Lobby?> creatingLobbyTask;

    public void CreateLobby()
    {
        creatingLobbyTask = SteamMatchmaking.CreateLobbyAsync(maxLobbyMembers);
    }

    private void OnLobbyCreated(Lobby? lobby)
    {
        CurrentLobby = lobby;
        Debug.Log("Successfully created lobby");
    }

    private void Update()
    {
        if (creatingLobbyTask != null && creatingLobbyTask.IsCompleted)
        {
            OnLobbyCreated(creatingLobbyTask.Result);
            creatingLobbyTask = null;
        }
        else if (creatingLobbyTask != null && creatingLobbyTask.IsFaulted)
        {
            Debug.Log("Failed to create lobby");
        }
    }

    public void DestroyLobby()
    {

    }
}
