using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendInvitePanel : Panel
{
    [SerializeField]
    private float hideDelay = 5;

    private Friend friend;
    private Lobby lobby;

    public void ShowPanel(Friend friend, Lobby lobby)
    {
        this.friend = friend;
        this.lobby = lobby;

        ShowPanel();

        CursorManager.Instance.ShowCursor(0);

        Invoke("Decline", hideDelay);
    }

    protected override void OnClose()
    {
        CursorManager.Instance.HideCursor(0);
    }

    public void Accept()
    {
        SteamLobbyManager.Instance.LeavePrivateLobby();
        SteamLobbyManager.Instance.JoinFriendLobby(lobby);
        Close();
    }

    public void Decline()
    {
        Close();
    }
}
