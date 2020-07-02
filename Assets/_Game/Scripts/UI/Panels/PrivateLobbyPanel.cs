using SteamworksNet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Steamworks.Data;
using Steamworks;

public class PrivateLobbyPanel : Panel
{
    [SerializeField]
    private BetterButton[] playButtons;

    [SerializeField]
    private GameObject privacyToggle;

    [SerializeField]
    private TMP_Text privacyStateText;

    [Space()]
    [SerializeField]
    private ConnectPlayerCell[] connectedPlayerCells;

    protected override void OnShow()
    {
        SubToEvents();

        //disable / enable all buttons based on if they're host or not
        privacyToggle.gameObject.SetActive(SteamLobbyManager.Instance.PrivateHost);
        foreach(var button in playButtons)
        {
            button.interactable = SteamLobbyManager.Instance.PrivateHost;
        }

        UpdatePlayerCells();
    }

    protected override void OnClose()
    {
        UnSubToEvents();
    }

    public void Search()
    {
        SteamLobbyManager.Instance.SearchForMatch();
    }

    public void Private()
    {
        SteamLobbyManager.Instance.PlayPrivateMatch();
    }

    public void LobbyPrivacyChanged(bool isOn)
    {
        if (isOn)
            SteamLobbyManager.Instance.PrivateLobby.Value.SetFriendsOnly();
        else
            SteamLobbyManager.Instance.PrivateLobby.Value.SetPrivate();

        privacyStateText.text = isOn ? "Public" : "Private";
    }

    private void UpdatePlayerCells()
    {
        //disable all the cells first, makes it easier
        foreach (var playerCell in connectedPlayerCells)
        {
            playerCell.gameObject.SetActive(false);
        }

        for (int i = 0; i < SteamLobbyManager.Instance.PrivateLobby.Value.Members.Count(); i++)
        {
            Friend friend = SteamLobbyManager.Instance.PrivateLobby.Value.Members.ElementAt(i);
            connectedPlayerCells[i].Configure(friend.Name);
        }
    }

    private void OnChatMessageReceived(Lobby lobby, Friend friend, string message)
    {
        if (message == SteamLobbyManager.PrivateLobbyStatedKey)
        {
            //SceneLoader.Instance.LoadScene("Lobby");
        }
    }

    private void OnLobbyEntered(Lobby obj)
    {
        UpdatePlayerCells();
    }

    private void OnLobbyMemberJoined(Lobby arg1, Friend arg2)
    {
        UpdatePlayerCells();
    }

    private void OnLobbyMemberLeave(Lobby arg1, Friend arg2)
    {
        UpdatePlayerCells();
    }

    private void SubToEvents()
    {
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnChatMessage += OnChatMessageReceived;
    }

    private void UnSubToEvents()
    {
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnChatMessage -= OnChatMessageReceived;
    }
}
