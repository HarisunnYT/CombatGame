using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : Singleton<LobbyManager>
{
    private void Start()
    {
        if (!ServerManager.Instance.IsServer && PlayFabMatchMaking.Instance)
        {
            CreateClient();
        }
    }

    private void CreateClient()
    {
        NetworkManager.singleton.networkAddress = "172.197.128.73";
        NetworkManager.singleton.StartClient();
    }
}
