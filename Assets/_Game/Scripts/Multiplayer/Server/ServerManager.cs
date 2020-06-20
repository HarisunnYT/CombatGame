using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerManager : PersistentSingleton<ServerManager>
{
    public bool IsServer { get; private set; }

    private void Start()
    {
        IsServer = SystemInfo.graphicsDeviceID == 0;

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
}
