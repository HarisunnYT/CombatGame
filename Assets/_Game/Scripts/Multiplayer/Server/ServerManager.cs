﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.ComponentModel.Design;
using PlayFab.MultiplayerModels;

public class ServerManager : PersistentSingleton<ServerManager>
{
    [SerializeField]
    private bool debugServer = false;

    public bool IsServer { get; private set; }
    public bool IsInUse { get; private set; }

    public List<NetworkConnection> ConnectedPlayers { get; private set; } = new List<NetworkConnection>();
    private List<int> SelectedCharacterIndexes = new List<int>();

    protected override void Initialize()
    {
        IsServer = SystemInfo.graphicsDeviceID == 0;
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

    public void OnPlayerConnectedToServer(NetworkConnection conn)
    {
        ConnectedPlayers.Add(conn);
    }

    public void OnPlayerDisconnectedFromServer(NetworkConnection conn)
    {
        ConnectedPlayers.Remove(conn);
        if (ConnectedPlayers.Count <= 0 && IsInUse)
        {
            NetworkManager.singleton.StopServer();
            Application.Quit();
        }
    }

    public void BeganMatch()
    {
        IsInUse = true;
    }

    public string GetPlayerName(NetworkConnection conn)
    {
        return "Player";
    }

    public void SetCharacterSelected(int characterID)
    {
        SelectedCharacterIndexes.Add(characterID);
    }

    public void SetCharacterUnselected(int characterID)
    {
        SelectedCharacterIndexes.Remove(characterID);
    }

    public bool IsCharacterSelected(int characterID)
    {
        return SelectedCharacterIndexes.Contains(characterID);
    }

    public int GetTick()
    {
        return Time.frameCount; 
    }
}