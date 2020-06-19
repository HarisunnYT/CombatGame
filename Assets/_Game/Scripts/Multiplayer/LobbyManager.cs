using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : Singleton<LobbyManager>
{
    private void Start()
    {
        if (PlayFabMatchMaking.Instance)
        {
            string networkAddress = PlayFabMatchMaking.Instance.CurrentServerIP;
            NetworkManager.singleton.networkAddress = networkAddress;

            if (networkAddress == NetworkManager.GetIP())
            {
                NetworkManager.singleton.StartHost();
            }
            else
            {
                NetworkManager.singleton.StartClient();
            }
        }
    }
}
