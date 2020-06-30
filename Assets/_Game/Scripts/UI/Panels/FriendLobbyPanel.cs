using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendLobbyPanel : Panel
{
    [SerializeField]
    private GameObject privacyToggle;

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

    public void LobbyPrivacyChanged(bool isOn)
    {
        if (isOn)
            SteamMatchMakingManager.Instance.CurrentLobby.SetFriendsOnly();
        else
            SteamMatchMakingManager.Instance.CurrentLobby.SetPrivate();
    }
}
