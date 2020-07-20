using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;
using Steamworks;
using System.Linq;
using Mirror.FizzySteam;
using Mirror;

public class SteamLobbyManager : PersistentSingleton<SteamLobbyManager>
{
    #region EXTENSIONS

    #endregion

    #region CONST_VARIABLES

    public const int MaxLobbyMembers = 2; //TODO SET TO 4

    private const string privateLobbyStartedKey = "private_lobby_started";
    private const string publicSearchKey = "public_search";
    private const string leaveMatchWithPartyKey = "leave_match_with_party";
    private const string kickPlayer = "kick_player";

    #endregion

    #region EXPOSED_VARIABLES

    #endregion

    #region PROPERTIES

    /// <summary>
    /// The friend only lobby the user is in
    /// </summary>
    public Lobby? PrivateLobby;

    public bool PrivateHost
    {
        get
        {
            if (PrivateLobby == null)
                return true;
            else
            {
                return PrivateLobby.Value.Owner.Id == SteamClient.SteamId;
            }
        }
    }

    /// <summary>
    /// The public match lobby the user is in
    /// </summary>
    public Lobby? PublicLobby;

    public bool PublicHost
    {
        get
        {
            if (PublicLobby == null)
                return true;
            else
            {
                return PublicLobby.Value.Owner.Id == SteamClient.SteamId;
            }
        }
    }

    public bool IsPrivateMatch { get; private set; }

    public bool Searching { get; private set; } = false;

    public bool InSoloLobby { get { return PrivateLobby == null || PrivateLobby.Value.MemberCount <= 1; } }

    public bool PrivateHostIsPublicHost { get { return PrivateLobby.Value.Owner.Id == PublicLobby.Value.Owner.Id; } }

    #endregion

    #region RUNTIME_VARIABLES

    #endregion

    #region TASKS

    private Task<Lobby?> creatingPrivateLobbyTask;
    private Task<RoomEnter> joiningPrivateLobbyTask;

    private Task<Lobby?> creatingPublicLobbyTask;
    private Task<Lobby[]> retrievingLobbiesTask;

    private System.Action<Lobby[]> retreivedLobbiesCallback;
    public System.Action<RoomEnter> joinedLobbyCallback;

    private Coroutine creatingServerCoroutine;
    private Coroutine creatingClientCoroutine;

    #endregion

    #region CALLBACKS

    public delegate void LobbyEvent();
    public event LobbyEvent OnBeganSearch;
    public event LobbyEvent OnCancelledSearch;
    public event LobbyEvent OnPublicMatchCreated;
    public event LobbyEvent OnKicked;

    #endregion

    protected override void Initialize()
    {
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        SteamMatchmaking.OnLobbyDataChanged += OnLobbyDataChanged;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
    }

    protected override void Deinitialize()
    {
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
        SteamMatchmaking.OnLobbyDataChanged -= OnLobbyDataChanged;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;

        base.Deinitialize();
    }

    private void Update()
    {
        if (retrievingLobbiesTask != null && retrievingLobbiesTask.IsCompleted)
        {
            retreivedLobbiesCallback?.Invoke(retrievingLobbiesTask.Result);
            retreivedLobbiesCallback = null;
            retrievingLobbiesTask = null;
        }

        if (creatingPrivateLobbyTask != null && creatingPrivateLobbyTask.IsCompleted)
        {
            JoinedPrivateLobby(creatingPrivateLobbyTask.Result);
            creatingPrivateLobbyTask = null;
        }

        if (creatingPublicLobbyTask != null && creatingPublicLobbyTask.IsCompleted)
        {
            CreatedPublicMatch(creatingPublicLobbyTask.Result);
            creatingPublicLobbyTask = null;
        }

        if (joiningPrivateLobbyTask != null && joiningPrivateLobbyTask.IsCompleted)
        {
            joinedLobbyCallback?.Invoke(joiningPrivateLobbyTask.Result);
            joiningPrivateLobbyTask = null;
            joinedLobbyCallback = null;
        }
    }

    #region CLIENT_SERVER

    private void StopServer()
    {
        if (FizzySteamworks.Instance.ServerActive())
            NetworkManager.Instance.StopServer();
    }

    public void StopClient()
    {
        if (FizzySteamworks.Instance.ClientActive())
            NetworkManager.Instance.StopClient();
    }

    public void CreateServer()
    {
        if (creatingServerCoroutine == null)
            creatingServerCoroutine = StartCoroutine(CreateServerIE());
    }

    private IEnumerator CreateServerIE()
    {
        StopServer();

        yield return new WaitForEndOfFrame();

        NetworkManager.Instance.StartHost();
        creatingServerCoroutine = null;
    }

    public void CreateClient(string networkAddress)
    {
        if (creatingClientCoroutine == null)
            creatingClientCoroutine = StartCoroutine(CreateClientIE(networkAddress));
    }

    private IEnumerator CreateClientIE(string networkAddress)
    {
        if (FizzySteamworks.Instance.ClientActive())
            yield break;

        yield return new WaitForEndOfFrame();

        NetworkManager.Instance.networkAddress = networkAddress;
        NetworkManager.Instance.StartClient();
        creatingClientCoroutine = null;
    }

    #endregion

    #region MESSAGES

    private void OnLobbyDataChanged(Lobby obj)
    {
        //the owner sends the messages, they don't need to receive them
        if (obj.Owner.Id != SteamClient.SteamId)
        {
            foreach (var data in obj.Data)
            {
                if (data.Key == privateLobbyStartedKey)
                {
                    PlayPrivateMatch();
                    return;
                }
                else if (data.Key == publicSearchKey)
                {
                    HostCreatedPublicMatch(data.Value);
                }
                else if (data.Key == leaveMatchWithPartyKey)
                {
                    HostPulledPartyFromMatch();
                }
                else if (data.Key == kickPlayer)
                {
                    OnPlayerKicked(data.Value);
                }
            }
        }
    }

    private void HostCreatedPublicMatch(string message)
    {
        //if the message is false it means the host cancelled the search
        if (message == "false")
        {
            Searching = false;

            LeavePublicLobby();

            OnCancelledSearch?.Invoke();
        }
        else //otherwise it'll be the lobby id to join
        {
            Searching = true;

            LobbyQuery lobbyQuery = SteamMatchmaking.LobbyList.WithSlotsAvailable(MaxLobbyMembers - PrivateLobby.Value.Members.Count()); //we can assign rules to this query
            retrievingLobbiesTask = lobbyQuery.RequestAsync();
            retreivedLobbiesCallback = (Lobby[] lobbies) =>
            {
                ulong lobbyID = ulong.Parse(message);
                if (lobbies != null)
                {
                    foreach (var lobby in lobbies)
                    {
                        if (lobby.Id.Value == lobbyID)
                        {
                            UpdateLobby(lobby);
                            lobby.Join();
                            break;
                        }
                    }
                }
            };

            OnBeganSearch?.Invoke();
        }
    }

    private void SendPrivateMessage(string key, string message)
    {
        if (PrivateLobby != null)
        {
            ClearPrivateLobbyData();
            PrivateLobby.Value.SetData(key, message);
        }
    }

    #endregion

    #region PRIVATE_LOBBY

    public void CreatePrivateLobby()
    {
        if (joiningPrivateLobbyTask != null) //already joining a lobby
            return;

        if (!FizzySteamworks.Instance.ServerActive())
            CreateServer();

        if (PrivateLobby != null)
        {
            PrivateLobby.Value.SetFriendsOnly();
            PrivateLobby.Value.SetJoinable(true);
        }

        if (creatingPrivateLobbyTask != null || PrivateLobby != null)
            return;

        creatingPrivateLobbyTask = SteamMatchmaking.CreateLobbyAsync(MaxLobbyMembers);
        Debug.Log("Created private lobby");
    }

    private void JoinedPrivateLobby(Lobby? lobby)
    {
        PrivateLobby = lobby;

        joiningPrivateLobbyTask = PrivateLobby.Value.Join();
        joinedLobbyCallback = (RoomEnter roomEnter) =>
        {
            joinedLobbyCallback = null;
            joiningPrivateLobbyTask = null;

            if (roomEnter == RoomEnter.Success) //TODO SHOW ERRORS AND STUFF WHEN TRYING TO JOIN LOBBY
            {
                PrivateLobby.Value.SetMemberData(FighterManager.LastPlayerFighterKey, FighterManager.Instance.LastPlayedFighterName);
                if (PrivateHost)
                {
                    ConnectedToPrivateLobbyServer();
                    CreatePrivateLobby();
                }
                else
                    CreateClient(PrivateLobby.Value.Owner.Id.Value.ToString());
            }
        };

        Debug.Log("Joined private lobby");
    }

    public void ConnectedToPrivateLobbyServer()
    {
        if (PanelManager.Instance.GetPanel<JoiningFriendPanel>() && (!PrivateLobby.HasValue || ServerManager.Instance.Players.Count >= PrivateLobby.Value.MemberCount))
            PanelManager.Instance.ClosePanel<JoiningFriendPanel>();

        if (PanelManager.Instance.GetPanel<PrivateLobbyPanel>())
            PanelManager.Instance.ShowPanel<PrivateLobbyPanel>();
    }

    //this is when the user accepts an invite or clicks 'join game' in steam UI
    private void OnGameLobbyJoinRequested(Lobby lobby, SteamId friendID)
    {
        JoinFriendLobby(lobby);
    }

    public void JoinFriendLobby(Lobby lobby)
    {
        PanelManager.Instance.ShowPanel<JoiningFriendPanel>();

        StopClient(); 
        StopServer();

        if (ExitManager.Instance)
        {
            ExitManager.Instance.ExitMatch(ExitType.LeftToJoinLobby, () =>
            {
                JoinedPrivateLobby(lobby);
                PanelManager.Instance.ShowPanel<PrivateLobbyPanel>();
            });
        }
        else
            JoinedPrivateLobby(lobby);
    }

    public void LeavePrivateLobby()
    {
        if (PrivateLobby != null && creatingPrivateLobbyTask == null && joiningPrivateLobbyTask == null)
        {
            if (PrivateHost)
                StopServer();
            else
                StopClient();

            if (PrivateLobby != null)
            {
                PrivateLobby.Value.Leave();
                PrivateLobby = null;
            }
        }
    }

    public void PlayPrivateMatch()
    {
        PublicLobby = PrivateLobby;
        PrivateLobby.Value.SetJoinable(false);

        IsPrivateMatch = true;

        if (PublicHost)
            SendPrivateMessage(privateLobbyStartedKey, "true");

        SceneLoader.Instance.LoadScene("Lobby");

        Searching = false;
    }

    public void ClearPrivateLobbyData()
    {
        if (PrivateLobby != null)
        {
            foreach (var data in PrivateLobby.Value.Data)
                PrivateLobby.Value.DeleteData(data.Key);
        }
    }

    private void HostPulledPartyFromMatch()
    {
        UpdateLobby(PrivateLobby.Value);
        ExitManager.Instance.ExitMatchWithParty();
    }

    public Friend? GetPrivateMember(ulong steamId)
    {
        foreach(var member in PrivateLobby.Value.Members)
        {
            if (member.Id.Value == steamId)
            {
                return member;
            }
        }

        return null;
    }

    public void KickPlayer(ulong steamId)
    {
        SendPrivateMessage(kickPlayer, steamId.ToString());
    }

    private void OnPlayerKicked(string steamId)
    {
        if (SteamClient.SteamId.Value.ToString() == steamId && PrivateLobby != null)
        {
            LeavePrivateLobby(); //TODO Show kicked message
            OnKicked?.Invoke();

            ErrorManager.Instance.EncounteredError("103");
        }
    }

    #endregion

    #region MATCH_MAKING

    public void SearchForMatch()
    {
        Searching = true;
        PrivateLobby.Value.SetJoinable(false);

        OnBeganSearch?.Invoke();

        LookForAvailablePublicMatch(MaxLobbyMembers - PrivateLobby.Value.Members.Count());
    }

    private void LookForAvailablePublicMatch(int slotsAvailable)
    {
        //first we need to get a list of servers, if any are available
        LobbyQuery lobbyQuery = SteamMatchmaking.LobbyList.WithSlotsAvailable(slotsAvailable); //we can assign rules to this query
        retrievingLobbiesTask = lobbyQuery.RequestAsync();
        retreivedLobbiesCallback = (Lobby[] lobbies) =>
        {
            if (lobbies != null && lobbies.Length > 0)
            {
                if (lobbies.Length > 0)
                {
                    List<Lobby> availableLobbies = lobbies.ToList();
                    for (int i = 0; i < availableLobbies.Count; i++)
                    {
                        if (PublicLobby != null && availableLobbies[i].Id.Value == PublicLobby.Value.Id.Value ||
                            (PrivateLobby != null && availableLobbies[i].Id.Value == PrivateLobby.Value.Id.Value))
                        {
                            availableLobbies.RemoveAt(i);
                            break;
                        }
                    }

                    //found the correct lobby
                    //TODO sort by skill, location, ping etc
                    if (availableLobbies.Count > 0)
                        JoinedPublicLobby(availableLobbies[0]);
                    else
                        CreatePublicMatchLobby();
                }
                else
                {
                    CreatePublicMatchLobby();
                }
            }
            else //no good lobbies, let's create one
            {
                CreatePublicMatchLobby();
            }
        };
    }

    public void CancelSearch()
    {
        Searching = false;
        PrivateLobby.Value.SetJoinable(true);

        OnCancelledSearch?.Invoke();
        SendPrivateMessage(publicSearchKey, "false");

        LeavePublicLobby();

        Debug.Log("send cancel");
    }

    private void CreatePublicMatchLobby()
    {
        if (PublicLobby != null)
        {
            LookForAvailablePublicMatch(MaxLobbyMembers - PublicLobby.Value.Members.Count());
            return;
        }

        creatingPublicLobbyTask = SteamMatchmaking.CreateLobbyAsync(MaxLobbyMembers);
    }

    private void CreatedPublicMatch(Lobby? lobby)
    {
        PublicLobby = lobby;
        PublicLobby.Value.SetPublic();

        PrivateLobby.Value.SetJoinable(false);

        //send a message to all members in the private lobby to join the public lobby that has been created
        SendPrivateMessage(publicSearchKey, lobby.Value.Id.Value.ToString());

        OnPublicMatchCreated?.Invoke();

        if (PublicLobby.Value.MemberCount >= MaxLobbyMembers)
            TryStartPublicMatch();
        else
            LookForAvailablePublicMatch(MaxLobbyMembers - PrivateLobby.Value.Members.Count());

        Debug.Log("Created public match lobby");
    }

    private void JoinedPublicLobby(Lobby? lobby)
    {
        StopServer();

        IsPrivateMatch = false;

        LeavePublicLobby();

        PublicLobby = lobby;
        PublicLobby.Value.Join();

        //send a message to all members in the private lobby to join the public lobby that has been created
        SendPrivateMessage(publicSearchKey, lobby.Value.Id.Value.ToString());

        Debug.Log("Joined public match");
    }

    public void LeavePublicLobby()
    {
        if (PublicLobby != null)
        {
            if (PrivateLobby == null || PublicLobby.Value.Id != PrivateLobby.Value.Id)
            {
                PublicLobby.Value.Leave();
            }

            PublicLobby = null;
        }
    }

    public bool AllPrivateMembersConnectedToPublic()
    {
        return PrivateLobby == null || PublicLobby == null || PublicLobby.Value.MemberCount >= PrivateLobby.Value.MemberCount;
    }

    public void ExitMatchWithParty()
    {
        if (PrivateHost)
            SendPrivateMessage(leaveMatchWithPartyKey, "true");
    }

    private void TryStartPublicMatch()
    {
        if (PublicLobby.Value.MemberCount >= MaxLobbyMembers)
        {
            SceneLoader.Instance.LoadScene("Lobby");
            Searching = false;
        }
    }

    #endregion

    #region CALLBACKS

    private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        ClearPrivateLobbyData();
        UpdateLobby(lobby);
    }

    private void OnLobbyEntered(Lobby obj)
    {
        UpdateLobby(obj);
    }

    private void OnLobbyMemberJoined(Lobby arg1, Friend arg2)
    {
        UpdateLobby(arg1);
    }

    private void OnLobbyInvite(Friend arg1, Lobby arg2)
    {
        PanelManager.Instance.GetPanel<FriendInvitePanel>().ShowPanel(arg1, arg2);
    }

    private void UpdateLobby(Lobby lobby)
    {
        //if we aren't searching and the lobby has updated, it must be a private lobby
        if (Searching)
        {
            PublicLobby = lobby;
            TryStartPublicMatch();
        }
        else
        {
            if (lobby.Id != PrivateLobby.Value.Id)
                lobby.Leave();
            else
                PrivateLobby = lobby;
        }
    }

    public void LeaveAllLobbies()
    {
        LeavePrivateLobby();
        LeavePublicLobby();
    }

    private void OnApplicationQuit()
    {
        LeaveAllLobbies();

        SteamClient.Shutdown();
    }

    #endregion
}
