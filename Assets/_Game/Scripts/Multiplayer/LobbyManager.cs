using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : Singleton<LobbyManager>
{
    private void Start()
    {
        if (PlayFabMatchMaking.Instance)
        {
            NetworkManager.singleton.networkAddress = "172.197.128.73";
            //NetworkManager.singleton.StartClient();
        }
    }
}
