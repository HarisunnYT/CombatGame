using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendLobbyPanel : Panel
{
    protected override void OnShow()
    {
        SteamLobbyManager.Instance.CreateLobby();
    }

    public void Search()
    {
        PanelManager.Instance.ShowPanel<MatchMakingSearchPanel>();
    }

    public void Private()
    {

    }
}
