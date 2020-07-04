using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;
using Steamworks;
using System.Linq;

public class SteamLobbyManager : PersistentSingleton<SteamLobbyManager>
{
    #region EXTENSIONS

    #endregion

    #region CONST_VARIABLES

    public const int MaxLobbyMembers = 4;

    private const string privateLobbyStartedKey = "private_lobby_started";
    private const string publicSearchKey = "public_search";

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
                return false;
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
                return false;
            else
            {
                return PublicLobby.Value.Owner.Id == SteamClient.SteamId;
            }
        }
    }

    #endregion

    #region RUNTIME_VARIABLES

    private bool searching = false;

    #endregion

    #region TASKS

    private Task<Lobby?> creatingPrivateLobbyTask;
    private Task<Lobby?> creatingPublicLobbyTask;

    private Task<Lobby[]> retrievingLobbiesTask;

    private System.Action<Lobby[]> retreivedLobbiesCallback;

    #endregion

    #region CALLBACKS

    public delegate void LobbyEvent();
    public event LobbyEvent OnBeganSearch;
    public event LobbyEvent OnCancelledSearch;

    #endregion

    protected override void Initialize()
    {
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        SteamMatchmaking.OnLobbyDataChanged += OnLobbyDataChanged;

        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
    }

    protected override void Deinitialize()
    {
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;

        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;

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
    }

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
                    //TODO move this
                    SceneLoader.Instance.LoadScene("Lobby");
                }
                else if (data.Key == publicSearchKey)
                {
                    HostCreatedPublicMatch(data.Value);
                }
            }
        }
    }

    private void HostCreatedPublicMatch(string message)
    {
        //if the message is false it means the host cancelled the search
        if (message == "false")
        {
            searching = false;

            PublicLobby = null;
            OnCancelledSearch?.Invoke();
        }
        else //otherwise it'll be the lobby id to join
        {
            searching = true;

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

    #endregion

    #region PRIVATE_LOBBY

    public void CreatePrivateLobby()
    {
        if (creatingPrivateLobbyTask != null)
            return;

        creatingPrivateLobbyTask = SteamMatchmaking.CreateLobbyAsync(MaxLobbyMembers);
        Debug.Log("Created private lobby");
    }

    private void JoinedPrivateLobby(Lobby? lobby)
    {
        PrivateLobby = lobby;
        PrivateLobby.Value.Join();

        PrivateLobby.Value.SetFriendsOnly();
        PrivateLobby.Value.SetJoinable(true);

        PanelManager.Instance.ShowPanel<PrivateLobbyPanel>();

        Debug.Log("Joined private lobby");
    }

    //this is when the user accepts an invite or clicks 'join game' in steam UI
    private void OnGameLobbyJoinRequested(Lobby lobby, SteamId friendID)
    {
        //this callback could happen at any point so we need to account for anything
        //TODO probably need to do this a bit better to account for scene loading times

        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "Game")
            MatchManager.Instance.ExitMatch(false);
        else if (currentSceneName == "Lobby")
            LobbyManager.Instance.ExitLobby(false);

        if (currentSceneName != "MainMenu")
            SceneLoader.Instance.LoadScene("MainMenu");

        JoinedPrivateLobby(lobby);
    }

    public void LeavePrivateLobby()
    {
        if (PrivateLobby != null)
            PrivateLobby.Value.Leave();
    }

    public void PlayPrivateMatch()
    {
        PublicLobby = PrivateLobby;
        PrivateLobby.Value.SetData(privateLobbyStartedKey, "true");

        SceneLoader.Instance.LoadScene("Lobby");
    }

    #endregion

    #region MATCH_MAKING

    public void SearchForMatch()
    {
        searching = true;
        PrivateLobby.Value.SetJoinable(false);

        OnBeganSearch?.Invoke();

        //first we need to get a list of servers, if any are available
        LobbyQuery lobbyQuery = SteamMatchmaking.LobbyList.WithSlotsAvailable(MaxLobbyMembers - PrivateLobby.Value.Members.Count()); //we can assign rules to this query
        retrievingLobbiesTask = lobbyQuery.RequestAsync();
        retreivedLobbiesCallback = (Lobby[] lobbies) =>
        {
            if (lobbies != null && lobbies.Length > 0)
            {
                List<Lobby> availableLobbies = lobbies.ToList();

                //just in case a lobby is owned by this player and wasn't terminated correctly
                for (int i = 0; i < availableLobbies.Count; i++)
                {
                    Lobby lobby = availableLobbies[i];
                    if (lobby.IsOwnedBy(SteamClient.SteamId))
                    {
                        if (lobby.Id != PrivateLobby.Value.Id)
                            lobby.Leave();

                        availableLobbies.RemoveAt(i);
                    }
                }

                if (availableLobbies.Count > 0)
                {
                    //found the correct lobby
                    //TODO sort by skill, location, ping etc
                    JoinedPublicLobby(availableLobbies[0]);
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
        searching = false;
        PrivateLobby.Value.SetJoinable(true);

        OnCancelledSearch?.Invoke();
        PrivateLobby.Value.SetData(publicSearchKey, "false");
    }

    private void CreatePublicMatchLobby()
    {
        if (PublicLobby != null)
            return;

        creatingPublicLobbyTask = SteamMatchmaking.CreateLobbyAsync(MaxLobbyMembers);
    }

    private void CreatedPublicMatch(Lobby? lobby)
    {
        PublicLobby = lobby;
        PublicLobby.Value.SetPublic();

        //send a message to all members in the private lobby to join the public lobby that has been created
        PrivateLobby.Value.SetData(publicSearchKey, lobby.Value.Id.Value.ToString());

        Debug.Log("Created public match lobby");
    }

    private void JoinedPublicLobby(Lobby? lobby)
    {
        searching = false;
        PublicLobby = lobby;
        PublicLobby.Value.Join();

        Debug.Log("Joined public match");

        //TODO put this somewhere else
        //SceneLoader.Instance.LoadScene("Lobby");
    }

    public void LeavePublicLobby()
    {
        if (PublicLobby != null)
            PublicLobby.Value.Leave();
    }

    #endregion

    #region CALLBACKS

    private void OnLobbyMemberLeave(Lobby arg1, Friend arg2)
    {
        UpdateLobby(arg1);
    }

    private void OnLobbyEntered(Lobby obj)
    {
        UpdateLobby(obj);
    }

    private void OnLobbyMemberJoined(Lobby arg1, Friend arg2)
    {
        UpdateLobby(arg1);
    }

    private void UpdateLobby(Lobby lobby)
    {
        //if we aren't searching and the lobby has updated, it must be a private lobby
        if (searching)
            PublicLobby = lobby;
        else
            PrivateLobby = lobby;
    }

    public void LeaveAllLobbies()
    {
        LeavePrivateLobby();
        LeavePublicLobby();
    }

    private void OnApplicationQuit()
    {
        LeaveAllLobbies();
    }

    #endregion
}
