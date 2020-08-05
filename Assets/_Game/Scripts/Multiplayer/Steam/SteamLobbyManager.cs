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

    [NonSerialized]
    public int MaxLobbyMembers = 3; //TODO SET TO 4

    private const string privateLobbyStartedKey = "private_lobby_started";
    private const string publicSearchKey = "public_search";
    private const string leaveMatchWithPartyKey = "leave_match_with_party";
    private const string kickPlayerKey = "kick_player";

    public const string VersionKey = "version";
    public const string PublicLobbyKey = "public_lobby";
    public const string InviteOnlyKey = "invite_only";

    #endregion

    #region EXPOSED_VARIABLES

    [SerializeField]
    private float queryMatchesDelay = 1;

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

    public bool PrivateLobbyJoinable { get; private set; } = true;

    #endregion

    #region RUNTIME_VARIABLES

    public SteamId InvitedFromId { get; set; }

    private bool matchFound = false;

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
    public event LobbyEvent OnMatchFound;

    public event LobbyEvent OnPrivateLobbyLeft;
    public event LobbyEvent OnPublicLobbyLeft;

    #endregion

    protected override void Initialize()
    {
        //TODO REMOVE
#if !UNITY_EDITOR
    MaxLobbyMembers = 3;
#endif

        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        SteamMatchmaking.OnLobbyDataChanged += OnLobbyDataChanged;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SceneManager.activeSceneChanged += activeSceneChanged;
    }

    protected override void Deinitialize()
    {
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
        SteamMatchmaking.OnLobbyDataChanged -= OnLobbyDataChanged;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
        SceneManager.activeSceneChanged -= activeSceneChanged;


        base.Deinitialize();
    }

    private void Update()
    {
        if (retrievingLobbiesTask != null && retrievingLobbiesTask.IsCompleted)
        {
            retreivedLobbiesCallback?.Invoke(retrievingLobbiesTask.Result);
            retrievingLobbiesTask = null;
            retreivedLobbiesCallback = null;
        }

        if (creatingPrivateLobbyTask != null && creatingPrivateLobbyTask.IsCompleted)
        {
            Lobby? lobby = creatingPrivateLobbyTask.Result;
            creatingPrivateLobbyTask = null;
            JoinedPrivateLobby(lobby);
        }

        if (creatingPublicLobbyTask != null && creatingPublicLobbyTask.IsCompleted)
        {
            Lobby? lobby = creatingPublicLobbyTask.Result;
            creatingPublicLobbyTask = null;
            CreatedPublicMatch(lobby);
        }

        if (joiningPrivateLobbyTask != null && joiningPrivateLobbyTask.IsCompleted)
        {
            joinedLobbyCallback?.Invoke(joiningPrivateLobbyTask.Result);
            joiningPrivateLobbyTask = null;
            joinedLobbyCallback = null;
        }

        if (Searching && matchFound)
            TryStartPublicGame();
    }

#region CLIENT_SERVER

    public void StopServer()
    {
        Debug.Log("stop server");

        if (FizzySteamworks.Instance.ServerActive())
            NetworkManager.Instance.StopServer();
    }

    public void StopClient()
    {
        Debug.Log("stop client");

        if (FizzySteamworks.Instance.ClientActive())
        {
            NetworkManager.Instance.StopClient();
            VoiceCommsManager.Instance.Stop();
        }
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
        if (PrivateLobby.HasValue && obj.Id == PrivateLobby.Value.Id)
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
                    else if (data.Key == kickPlayerKey)
                    {
                        OnPlayerKicked(data.Value);
                    }
                }
            }
        }
        else if (PublicLobby.HasValue && obj.Id == PublicLobby.Value.Id)
        {
            if (obj.Owner.Id != SteamClient.SteamId)
            {
                foreach (var data in obj.Data) //lobby host that you joined, joined a different lobby so join that one
                {
                    if (data.Key == publicSearchKey)
                    {
                        HostCreatedPublicMatch(data.Value);
                    }
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
        IsPrivateMatch = false;

        if (joiningPrivateLobbyTask != null) //already joining a lobby
            return;

        if (!FizzySteamworks.Instance.ServerActive())
            CreateServer();

        if (creatingPrivateLobbyTask != null || PrivateLobby != null)
        {
            SetPrivateLobbyJoinable(PrivateLobbyJoinable, true);
            return;
        }

        creatingPrivateLobbyTask = SteamMatchmaking.CreateLobbyAsync(MaxLobbyMembers);
        Debug.Log("Created private lobby");
    }

    private void JoinedPrivateLobby(Lobby? lobby)
    {
        PrivateLobby = lobby;

        if (PrivateHost)
            SetPrivateLobbyJoinable(true);

        joiningPrivateLobbyTask = PrivateLobby.Value.Join();
        joinedLobbyCallback = (RoomEnter roomEnter) =>
        {
            joinedLobbyCallback = null;
            joiningPrivateLobbyTask = null;

            if (roomEnter == RoomEnter.Success) 
            {
                PrivateLobby.Value.SetMemberData(FighterManager.LastPlayerFighterKey, FighterManager.Instance.LastPlayedFighterName);
                if (PrivateHost)
                {
                    ConnectedToPrivateLobbyServer();
                    CreatePrivateLobby();
                    PrivateLobby.Value.SetData(VersionKey, Application.version);
                }
                else
                {
                    if (PrivateLobby.Value.GetData(VersionKey) != Application.version)
                        CouldntJoinPrivateLobby("104");
                    else if (PrivateLobby.Value.GetData(InviteOnlyKey) == "True" && PrivateLobby.Value.Owner.Id != InvitedFromId)
                        CouldntJoinPrivateLobby("105");
                    else
                        CreateClient(PrivateLobby.Value.Owner.Id.Value.ToString());
                }
            }
        };

        Debug.Log("Joined private lobby");
    }

    private void CouldntJoinPrivateLobby(string errorCode)
    {
        if (ExitManager.Instance)
            ExitManager.Instance.ExitMatch(ExitType.Leave);
        else
        {
            PanelManager.Instance.ShowPanel<PlayPanel>();
            LeavePrivateLobby();
        }

        ErrorManager.Instance.EncounteredError(errorCode);
    }

    public void ConnectedToPrivateLobbyServer()
    {
        if (PanelManager.Instance.GetPanel<JoiningFriendPanel>() && (!PrivateLobby.HasValue || ServerManager.Instance.Players.Count >= PrivateLobby.Value.MemberCount))
        {
            PanelManager.Instance.ClosePanel<JoiningFriendPanel>();
            StartVoiceComms();
        }

        if (PanelManager.Instance.GetPanel<PrivateLobbyPanel>())
            PanelManager.Instance.ShowPanel<PrivateLobbyPanel>();
    }

    public void StartVoiceComms()
    {
        if (PrivateHost && PublicHost)
            VoiceCommsManager.Instance.StartServer();
        else if (!PublicHost || !PublicLobby.HasValue)
            VoiceCommsManager.Instance.StartClient();
    }

    //this is when the user accepts an invite or clicks 'join game' in steam UI
    private void OnGameLobbyJoinRequested(Lobby lobby, SteamId friendID)
    {
        JoinFriendLobby(lobby);
    }

    public void JoinFriendLobby(Lobby lobby)
    {
        if (SceneLoader.IsMainMenu)
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

                OnPrivateLobbyLeft?.Invoke();
            }
        }
    }

    public void PlayPrivateMatch()
    {
        PublicLobby = PrivateLobby;
        SetPrivateLobbyJoinable(false, true);

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
            {
                if (data.Key != VersionKey && data.Key != InviteOnlyKey) //we don't want to delete this type of key
                    PrivateLobby.Value.DeleteData(data.Key);
            }
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
        SendPrivateMessage(kickPlayerKey, steamId.ToString());
    }

    private void OnPlayerKicked(string steamId)
    {
        if (SteamClient.SteamId.Value.ToString() == steamId && PrivateLobby != null)
        {
            LeavePrivateLobby(); 
            OnKicked?.Invoke();

            ErrorManager.Instance.EncounteredError("103");
        }
    }

    public void SetPrivateLobbyJoinable(bool joinable, bool forced = false)
    {
        if (!forced)
        {
            PrivateLobbyJoinable = joinable;
            PrivateLobby.Value.SetData(InviteOnlyKey, (!joinable).ToString());
            PrivateLobby.Value.SetJoinable(true);
        }
        else
            PrivateLobby.Value.SetJoinable(joinable);

        PrivateLobby.Value.SetFriendsOnly();
    }

#endregion

#region MATCH_MAKING

    public void SearchForMatch()
    {
        Searching = true;
        SetPrivateLobbyJoinable(false, true);

        OnBeganSearch?.Invoke();
        LookForAvailablePublicMatch(PrivateLobby.Value.Members.Count());
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
                            continue;
                        }

                        if (availableLobbies[i].Owner.Id == SteamClient.SteamId)
                        {
                            CreatePublicMatchLobby();
                            return;
                        }

                        //lobby must be of the same version as the client
                        if (availableLobbies[i].GetData(VersionKey) != Application.version)
                        {
                            availableLobbies.RemoveAt(i);
                            continue;
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
        SetPrivateLobbyJoinable(PrivateLobbyJoinable, true);

        OnCancelledSearch?.Invoke();
        SendPrivateMessage(publicSearchKey, "false");

        if (creatingPublicLobbyTask != null)
            creatingPublicLobbyTask = null;
        if (retrievingLobbiesTask != null)
            retrievingLobbiesTask = null;

        LeavePublicLobby();

        Debug.Log("send cancel");
    }

    private void CreatePublicMatchLobby()
    {
        if (PublicLobby != null)
        {
            StartCoroutine(DelayedLookForMatch());
            return;
        }

        creatingPublicLobbyTask = SteamMatchmaking.CreateLobbyAsync(MaxLobbyMembers);
    }

    private IEnumerator DelayedLookForMatch()
    {
        yield return new WaitForSecondsRealtime(queryMatchesDelay);

        if (PublicLobby.HasValue)
            LookForAvailablePublicMatch(PublicLobby.Value.Members.Count());
    }

    private void CreatedPublicMatch(Lobby? lobby)
    {
        if (!lobby.HasValue || !PrivateLobby.HasValue)
            return;

        PublicLobby = lobby;
        PublicLobby.Value.SetData(PublicLobbyKey, "true");
        PublicLobby.Value.SetData(VersionKey, Application.version);

        SetPrivateLobbyJoinable(false, true);

        ServerManager.Instance.ClearPlayersExcludingPrivateLobby();

        //send a message to all members in the private lobby to join the public lobby that has been created
        SendPrivateMessage(publicSearchKey, lobby.Value.Id.Value.ToString());

        OnPublicMatchCreated?.Invoke();

        if (PublicLobby.Value.MemberCount >= MaxLobbyMembers)
            TryJoinPublicServer();
        else
            LookForAvailablePublicMatch(PrivateLobby.Value.Members.Count());

        Debug.Log("Created public match lobby");
    }

    private void JoinedPublicLobby(Lobby? lobby)
    {
        IsPrivateMatch = false;

        if (PublicLobby.HasValue)
            SendPublicMessage(publicSearchKey, lobby.Value.Id.Value.ToString());

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
                OnPublicLobbyLeft?.Invoke();
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

    private void TryJoinPublicServer()
    {
        if (PublicLobby.Value.MemberCount >= MaxLobbyMembers)
        {
            //join first
            VoiceCommsManager.Instance.Stop();

            if (!PublicHost && !PrivateHostIsPublicHost)
            {
                StopServer();
                StopClient();
            }

            if (!PublicHost || !PublicLobby.HasValue)
            {
                CreateClient(PublicLobby.Value.Owner.Id.Value.ToString());
            }

            matchFound = true;
            OnMatchFound?.Invoke();
        }
    }

    private void TryStartPublicGame()
    {
        if (PublicLobby.HasValue && ServerManager.Instance.Players.Count >= PublicLobby.Value.MemberCount && SceneLoader.IsMainMenu)
        {
            matchFound = false;
            SceneLoader.Instance.LoadScene("Lobby");
        }
    }

    private void SendPublicMessage(string key, string message)
    {
        if (PublicLobby.HasValue)
        {
            ClearPublicLobbyData();
            PublicLobby.Value.SetData(key, message);
        }
    }

    private void ClearPublicLobbyData()
    {
        if (PublicLobby.HasValue)
        {
            foreach (var data in PublicLobby.Value.Data)
                PublicLobby.Value.DeleteData(data.Key);
        }
    }

    public bool PrivateLobbyContainsPlayer(SteamId id)
    {
        foreach(var player in PrivateLobby.Value.Members)
        {
            if (player.Id == id)
                return true;
        }

        return false;
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

        //this is needed for some reason as OnLobbyMemberJoined doesn't work on private lobby panel...
        PanelManager.Instance.GetPanel<PrivateLobbyPanel>()?.OnLobbyMemberJoined(arg1, arg2);
    }

    private void OnLobbyInvite(Friend arg1, Lobby arg2)
    {
        InvitedFromId = arg1.Id;
        PanelManager.Instance.GetPanel<FriendInvitePanel>().ShowPanel(arg1, arg2);
    }

    private void UpdateLobby(Lobby lobby)
    {
        if (!SceneLoader.IsMainMenu)
            return;

        //if we aren't searching and the lobby has updated, it must be a private lobby
        if (Searching)
        {
            PublicLobby = lobby;
            TryJoinPublicServer();
        }
        else
        {
            if (lobby.Id != PrivateLobby.Value.Id)
                lobby.Leave();
            else
                PrivateLobby = lobby;
        }
    }

    private void activeSceneChanged(Scene from, Scene to)
    {
        if (SceneLoader.IsMainMenu)
            Searching = false;
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
