using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FriendLobbyPanel : Panel
{
    [SerializeField]
    private GameObject privacyToggle;

    [SerializeField]
    private TMP_Text privacyStateText;

    [Space()]
    [SerializeField]
    private ConnectPlayerCell[] connectedPlayerCells;

    protected override void OnShow()
    {
        SteamLobbyManager.Instance.CreateLobby();
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnChatMessage += OnChatMessageReceived;
    }

    protected override void OnClose()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnChatMessage -= OnChatMessageReceived;
    }

    public void Search()
    {
        PanelManager.Instance.ShowPanel<MatchMakingSearchPanel>();
    }

    public void Private()
    {
        SteamLobbyManager.Instance.CurrentLobby.Value.SendChatString(SteamLobbyManager.PrivateLobbyStatedKey);
        SceneLoader.Instance.LoadScene("Lobby");
    }

    public void LobbyPrivacyChanged(bool isOn)
    {
        if (isOn)
            SteamLobbyManager.Instance.CurrentLobby.Value.SetFriendsOnly();
        else
            SteamLobbyManager.Instance.CurrentLobby.Value.SetPrivate();

        privacyStateText.text = isOn ? "Public" : "Private";
    }

    private void UpdatePlayerCells()
    {
        //disable all the cells first, makes it easier
        foreach(var playerCell in connectedPlayerCells)
        {
            playerCell.gameObject.SetActive(false);
        }

        for (int i = 0; i < SteamLobbyManager.Instance.CurrentLobby.Value.Members.Count(); i++)
        {
            Friend friend = SteamLobbyManager.Instance.CurrentLobby.Value.Members.ElementAt(i);
            connectedPlayerCells[i].Configure(friend.Name);
        }
    }

    private void OnChatMessageReceived(Steamworks.Data.Lobby lobby, Friend friend, string message)
    {
        if (message == SteamLobbyManager.PrivateLobbyStatedKey)
        {
            SceneLoader.Instance.LoadScene("Lobby");
        }
    }

    private void OnLobbyEntered(Steamworks.Data.Lobby obj)
    {
        UpdatePlayerCells();
    }

    private void OnLobbyMemberJoined(Steamworks.Data.Lobby arg1, Friend arg2)
    {
        UpdatePlayerCells();
    }

    private void OnLobbyCreated(Result arg1, Steamworks.Data.Lobby arg2)
    {
        UpdatePlayerCells();
    }

    private void OnLobbyMemberLeave(Steamworks.Data.Lobby arg1, Friend arg2)
    {
        UpdatePlayerCells();
    }
}
