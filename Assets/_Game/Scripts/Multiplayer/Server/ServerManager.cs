using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.ComponentModel.Design;

public class ServerManager : PersistentSingleton<ServerManager>
{
    public bool IsServer { get; private set; }

    private int connectUsersCount = 0;

    protected override void Initialize()
    {
        IsServer = SystemInfo.graphicsDeviceID == 0;
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

    public void OnPlayerDisconnectedFromServer(NetworkConnection conn)
    {
        connectUsersCount--;

        if (connectUsersCount <= 0)
        {
            NetworkManager.singleton.StopServer();
            Application.Quit();
        }
    }

    public void OnPlayerConnectedToServer(NetworkConnection conn)
    {
        connectUsersCount++;
    }
}
