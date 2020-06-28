using InControl;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPanel : Panel
{
    public void Online()
    {
        PanelManager.Instance.ShowPanel<FriendLobbyPanel>();
        ServerManager.Instance.IsOnlineMatch = true;
    }

    public void Local()
    {
        ServerManager.Instance.IsOnlineMatch = false;
        SceneLoader.Instance.LoadScene("Lobby");
    }
}
