﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Steamworks.Data;
using Steamworks;
using JetBrains.Annotations;

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
    private GameObject searchingObj;

    [SerializeField]
    private GameObject cancelButton;

    [SerializeField]
    private TMP_Text cancelButtonText;

    [SerializeField]
    private TMP_Text playersFoundText;

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

        cancelButton.SetActive(false);

        UpdatePlayerCells();
    }

    private void OnDestroy()
    {
        UnSubToEvents();
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

        //configure client cell
        connectedPlayerCells[0].Configure(SteamClient.Name);

        if (SteamLobbyManager.Instance.PrivateLobby != null)
        {
            int cellIndex = 1;
            for (int i = 0; i < SteamLobbyManager.Instance.PrivateLobby.Value.Members.Count(); i++)
            {
                Friend friend = SteamLobbyManager.Instance.PrivateLobby.Value.Members.ElementAt(i);
                if (friend.Id != 0 && friend.Id != SteamClient.SteamId)
                {
                    connectedPlayerCells[cellIndex++].Configure(friend.Name);
                }
            }
        }

        if (SteamLobbyManager.Instance.PublicLobby != null)
            playersFoundText.text = SteamLobbyManager.Instance.PublicLobby.Value.MemberCount + "/" + SteamLobbyManager.MaxLobbyMembers;

        if (SteamLobbyManager.Instance.Searching && SteamLobbyManager.Instance.AllPrivateMembersConnectedToPublic())
            cancelButton.SetActive(SteamLobbyManager.Instance.PrivateHost);
    }

    private void OnBeganSearch()
    {
        foreach(var button in playButtons)
        {
            button.interactable = false;
        }

        searchingObj.SetActive(true);
    }

    private void OnCancelledSearch()
    {
        foreach (var button in playButtons)
        {
            button.interactable = SteamLobbyManager.Instance.PrivateHost;
        }

        searchingObj.SetActive(false);
    }

    private void ShowCancelButton()
    {
        cancelButton.SetActive(SteamLobbyManager.Instance.PrivateHost);
    }

    public void CancelSearch()
    {
        cancelButton.SetActive(false);
        SteamLobbyManager.Instance.CancelSearch();
    }

    public void LeaveLobby()
    {
        PanelManager.Instance.GetPanel<AreYouSurePanel>().Configure(this, () =>
        {
            PanelManager.Instance.ShowPanel<PlayPanel>();
            SteamLobbyManager.Instance.PrivateLobby.Value.Leave();
        });
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

        SteamLobbyManager.Instance.OnBeganSearch += OnBeganSearch;
        SteamLobbyManager.Instance.OnCancelledSearch += OnCancelledSearch; 
    }

    private void UnSubToEvents()
    {
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;

        if (SteamLobbyManager.Instance)
        {
            SteamLobbyManager.Instance.OnBeganSearch -= OnBeganSearch;
            SteamLobbyManager.Instance.OnCancelledSearch -= OnCancelledSearch;
        }
    }
}
