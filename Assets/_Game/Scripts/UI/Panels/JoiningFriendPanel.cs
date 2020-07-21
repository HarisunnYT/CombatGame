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

    private void Update()
    {
        if (SteamLobbyManager.Instance.PrivateLobby.Value.MemberCount != 0 && ServerManager.Instance.Players.Count >= SteamLobbyManager.Instance.PrivateLobby.Value.MemberCount)
        {
            SteamLobbyManager.Instance.StartVoiceComms();
        }
    }
}
