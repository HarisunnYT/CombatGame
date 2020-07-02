using InControl;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPanel : Panel
{
    public void Online()
    {
        SteamLobbyManager.Instance.CreatePrivateLobby();
        ServerManager.Instance.IsOnlineMatch = true;
    }

    public void Local()
    {
        ServerManager.Instance.IsOnlineMatch = false;
        SceneLoader.Instance.LoadScene("Lobby");
    }
}
