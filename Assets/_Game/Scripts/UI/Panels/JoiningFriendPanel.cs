using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoiningFriendPanel : Panel
{
    public void Cancel()
    {
        SteamLobbyManager.Instance.LeavePrivateLobby();
        PanelManager.Instance.ShowPanel<MainMenuPanel>();
    }
}
