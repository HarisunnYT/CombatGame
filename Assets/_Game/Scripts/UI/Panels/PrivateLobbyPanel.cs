﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Steamworks.Data;
using Steamworks;
using JetBrains.Annotations;
using Mirror.FizzySteam;

public class PrivateLobbyPanel : Panel
{
    [SerializeField]
    private BetterButton[] playButtons;

    [SerializeField]
    private GameObject privacyToggle;

    [SerializeField]
    private BetterButton leaveButton;

    [Space()]
    [SerializeField]
    private GameObject searchingObj;

    [SerializeField]
    private GameObject cancelButton;

    [SerializeField]
    private TMP_Text matchSearchTitle;

    [SerializeField]
    private TMP_Text playersFoundText;

    [Space()]
    [SerializeField]
    private SelectedCharacterCell[] connectedPlayerCells;

    private bool matchFound = false;

    protected override void OnShow()
    {
        PanelManager.Instance.ShowPanel<ChatPanel>();
        PanelManager.Instance.GetPanel<ChatPanel>().ClearMessages();

        ServerManager.Instance.IsOnlineMatch = true;

        if (SteamLobbyManager.Instance.PrivateLobby != null)
        {
            if (SteamLobbyManager.Instance.PrivateLobby.Value.MemberCount > 1) //don't worry about showing this panel as the user is by themselves anyway
                PanelManager.Instance.ShowPanel<JoiningFriendPanel>();

            Invoke("DelayedInit", 0.5f); //this is a must to stop fizzy steamworks errors
        }

        SubToEvents();
        matchFound = false;

        //disable / enable all buttons based on if they're host or not
        privacyToggle.gameObject.SetActive(SteamLobbyManager.Instance.PrivateHost);
        foreach(var button in playButtons)
        {
            button.SetInteractable(SteamLobbyManager.Instance.PrivateHost);
        }

        cancelButton.SetActive(false);

        UpdatePlayerCells();
    }

    protected override void OnClose()
    {
        foreach (var playerCell in connectedPlayerCells)
        {
            playerCell.ForceHide();
        }

        PanelManager.Instance.ClosePanel<ChatPanel>();
    }

    private void DelayedInit()
    {
        if (SteamLobbyManager.Instance.PrivateHost && !FizzySteamworks.Instance.ServerActive())
            SteamLobbyManager.Instance.CreatePrivateLobby();
        else if (!SteamLobbyManager.Instance.PrivateHost)
            SteamLobbyManager.Instance.CreateClient(SteamLobbyManager.Instance.PrivateLobby.Value.Owner.Id.Value.ToString());
    }

    private void OnDestroy()
    {
        UnSubToEvents();
    }

    private void OnDisable()
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

    public void ShowFriendsList()
    {
        PanelManager.Instance.ShowPanel<FriendsListPanel>();
    }

    private void UpdatePlayerCells()
    {
        Lobby? privateLobby = SteamLobbyManager.Instance.PrivateLobby;
        if (privateLobby == null)
            return;

        //disable all the cells first, makes it easier
        foreach (var playerCell in connectedPlayerCells)
        {
            playerCell.Unconfigure(false);
        }

        string fighterName = privateLobby.Value.GetMemberData(privateLobby.Value.Owner, FighterManager.LastPlayerFighterKey);
        if (fighterName == null)
            fighterName = "";

        string ownerName = SteamLobbyManager.Instance.PrivateLobby.Value.Owner.Name;

        //configure host cell
        connectedPlayerCells[0].Configure(ownerName, FighterManager.Instance.GetFighter(fighterName)); 
        connectedPlayerCells[0].GetComponent<Animator>().SetBool("Connected", true);

        for (int i = 0; i < ServerManager.Instance.Players.Count; i++)
        {
            if (privateLobby.Value.MemberCount > i)
            {
                Friend friend = privateLobby.Value.Members.ElementAt(i);
                if (friend.Id != 0 && friend.Id.Value != privateLobby.Value.Owner.Id.Value)
                {
                    fighterName = privateLobby.Value.GetMemberData(friend, FighterManager.LastPlayerFighterKey);
                    foreach (var playerCell in connectedPlayerCells)
                    {
                        if (!playerCell.Occuipied)
                        {
                            playerCell.Configure(friend.Name, FighterManager.Instance.GetFighter(fighterName));
                            playerCell.GetComponent<Animator>().SetBool("Connected", true);
                            break;
                        }
                    }
                }
            }
        }

        foreach (var playerCell in connectedPlayerCells)
        {
            if (!playerCell.Occuipied)
            {
                playerCell.GetComponent<Animator>().SetBool("Connected", false);
            }
        }

        privacyToggle.gameObject.SetActive(SteamLobbyManager.Instance.PrivateHost);

        UpdatePlayersFoundText();

        if (!SteamLobbyManager.Instance.Searching)
            SetPlayButtonsInteractable(true, 0);
    }

    private void UpdatePlayersFoundText()
    {
        if (matchFound)
            return;

        if (SteamLobbyManager.Instance.PublicLobby != null)
            playersFoundText.text = SteamLobbyManager.Instance.PublicLobby.Value.MemberCount + "/" + SteamLobbyManager.Instance.MaxLobbyMembers;

        matchSearchTitle.text = "Players Found";
    }

    private void OnBeganSearch()
    {
        playersFoundText.text = SteamLobbyManager.Instance.PrivateLobby.Value.MemberCount + "/" + SteamLobbyManager.Instance.MaxLobbyMembers;
        SetPlayButtonsInteractable(false, 1);
        searchingObj.SetActive(true);

        cancelButton.SetActive(SteamLobbyManager.Instance.PrivateHost);
    }

    private void OnCancelledSearch()
    {
        SetPlayButtonsInteractable(true, 1);
        searchingObj.SetActive(false);
    }

    public void CancelSearch()
    {
        cancelButton.SetActive(false);
        SteamLobbyManager.Instance.CancelSearch();
    }

    public void LeaveLobby()
    {
        System.Action leaveAction = () =>
        {
            PanelManager.Instance.ShowPanel<PlayPanel>();
            SteamLobbyManager.Instance.LeavePrivateLobby();
        };

        if (SteamLobbyManager.Instance.PrivateLobby.Value.MemberCount > 1)
            PanelManager.Instance.GetPanel<AreYouSurePanel>().Configure(this, leaveAction);
        else
            leaveAction.Invoke();
    }

    private void SetPlayButtonsInteractable(bool interactable, int index)
    {
        foreach (var button in playButtons)
        {
            button.SetInteractable(interactable ? SteamLobbyManager.Instance.PrivateHost : false, index); 
        }

        leaveButton.SetInteractable(interactable);
    }

    private void SubToEvents()
    {
        ServerManager.Instance.OnPlayerAdded += OnPlayerAddedToPrivateLobby;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyMemberDataChanged += OnMemberDataChanged;

        SteamLobbyManager.Instance.OnBeganSearch += OnBeganSearch;
        SteamLobbyManager.Instance.OnCancelledSearch += OnCancelledSearch;
        SteamLobbyManager.Instance.OnKicked += OnKicked;
        SteamLobbyManager.Instance.OnMatchFound += OnMatchFound;
    }

    private void UnSubToEvents()
    {
        if (ServerManager.Instance)
            ServerManager.Instance.OnPlayerAdded -= OnPlayerAddedToPrivateLobby;

        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyMemberDataChanged -= OnMemberDataChanged;

        if (SteamLobbyManager.Instance)
        {
            SteamLobbyManager.Instance.OnBeganSearch -= OnBeganSearch;
            SteamLobbyManager.Instance.OnCancelledSearch -= OnCancelledSearch;
            SteamLobbyManager.Instance.OnKicked -= OnKicked;
            SteamLobbyManager.Instance.OnMatchFound -= OnMatchFound;
        }
    }

    private void OnKicked()
    {
        DelayedInit();
        UpdatePlayerCells();
    }

    private void OnMatchFound()
    {
        matchFound = true;
        cancelButton.SetActive(false);

        matchSearchTitle.text = "Match Found";
        playersFoundText.text = "Connecting";
    }

    public void OnLobbyMemberJoined(Lobby arg1, Friend arg2)
    {
        if (arg1.Id == SteamLobbyManager.Instance.PrivateLobby.Value.Id && ServerManager.Instance.GetPlayer(arg2.Id.Value) != null)
            UpdatePlayerCells();

        UpdatePlayersFoundText();
    }

    private void OnLobbyEntered(Lobby obj)
    {
        UpdatePlayerCells();
    }

    private void OnPlayerAddedToPrivateLobby(ServerManager.ConnectedPlayer player)
    {
        UpdatePlayerCells();
    }

    private void OnLobbyMemberLeave(Lobby arg1, Friend arg2)
    {
        UpdatePlayerCells();
    }

    private void OnMemberDataChanged(Lobby obj, Friend friend)
    {
        UpdatePlayerCells();
    }
}
