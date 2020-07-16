using System.Collections;
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
    private TMP_Text privacyStateText;

    [Space()]
    [SerializeField]
    private GameObject searchingObj;

    [SerializeField]
    private GameObject cancelButton;

    [SerializeField]
    private TMP_Text playersFoundText;

    [Space()]
    [SerializeField]
    private SelectedCharacterCell[] connectedPlayerCells;

    protected override void OnShow()
    {
        Invoke("DelayedInit", 0.5f); //this is a must to stop fizzy steamworks errors

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

    private void DelayedInit()
    {
        if (SteamLobbyManager.Instance.PrivateLobby != null)
        {
            if (SteamLobbyManager.Instance.PrivateHost && !FizzySteamworks.Instance.ServerActive())
                SteamLobbyManager.Instance.CreatePrivateLobby();
            else if (!SteamLobbyManager.Instance.PrivateHost)
                SteamLobbyManager.Instance.CreateClient(SteamLobbyManager.Instance.PrivateLobby.Value.Owner.Id.Value.ToString());

            PanelManager.Instance.ShowPanel<JoiningFriendPanel>();
        }
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

        //configure client cell
        connectedPlayerCells[0].Configure(SteamLobbyManager.Instance.PrivateLobby.Value.Owner.Name, FighterManager.Instance.GetFighter(fighterName)); 
        connectedPlayerCells[0].GetComponent<Animator>().SetBool("Connected", true);

        if (privateLobby != null)
        {
            for (int i = 1; i < ServerManager.Instance.Players.Count; i++)
            {
                Friend friend = privateLobby.Value.Members.ElementAt(i);
                if (friend.Id != 0 && friend.Id.Value != privateLobby.Value.Owner.Id.Value)
                {
                    fighterName = privateLobby.Value.GetMemberData(friend, FighterManager.LastPlayerFighterKey);
                    connectedPlayerCells[i].Configure(friend.Name, FighterManager.Instance.GetFighter(fighterName));
                    connectedPlayerCells[i].GetComponent<Animator>().SetBool("Connected", true);
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

        if (SteamLobbyManager.Instance.PublicLobby != null)
            playersFoundText.text = SteamLobbyManager.Instance.PublicLobby.Value.MemberCount + "/" + SteamLobbyManager.MaxLobbyMembers;

        if (SteamLobbyManager.Instance.Searching && SteamLobbyManager.Instance.AllPrivateMembersConnectedToPublic())
            cancelButton.SetActive(SteamLobbyManager.Instance.PrivateHost);

        if (!SteamLobbyManager.Instance.Searching)
            SetPlayButtonsInteractable(true);
    }

    private void OnBeganSearch()
    {
        SetPlayButtonsInteractable(false);
        searchingObj.SetActive(true);
    }

    private void OnCancelledSearch()
    {
        SetPlayButtonsInteractable(true);
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

    private void SetPlayButtonsInteractable(bool interactable)
    {
        foreach (var button in playButtons)
        {
            button.interactable = interactable ? SteamLobbyManager.Instance.PrivateHost : false;
        }
    }

    private void SubToEvents()
    {
        ServerManager.Instance.OnPlayerAdded += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyMemberDataChanged += OnMemberDataChanged;

        SteamLobbyManager.Instance.OnBeganSearch += OnBeganSearch;
        SteamLobbyManager.Instance.OnCancelledSearch += OnCancelledSearch; 
    }

    private void UnSubToEvents()
    {
        ServerManager.Instance.OnPlayerAdded -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyMemberDataChanged -= OnMemberDataChanged;

        if (SteamLobbyManager.Instance)
        {
            SteamLobbyManager.Instance.OnBeganSearch -= OnBeganSearch;
            SteamLobbyManager.Instance.OnCancelledSearch -= OnCancelledSearch;
        }
    }

    private void OnLobbyEntered(Lobby obj)
    {
        UpdatePlayerCells();
    }

    private void OnLobbyMemberJoined(ServerManager.ConnectedPlayer player)
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
