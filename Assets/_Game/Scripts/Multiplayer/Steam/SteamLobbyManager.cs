using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;
using Steamworks;
using System.Linq;
using UnityEngine.Assertions.Must;
using UnityScript.Steps;

public class SteamLobbyManager : PersistentSingleton<SteamLobbyManager>
{
    #region EXTENSIONS

    #endregion

    #region CONST_VARIABLES

    public const int MaxLobbyMembers = 4;

    private const string privateLobbyStartedKey = "private_lobby_started";
    private const string publicSearchStartedKey = "public_search_started";
    private const string publicSearchCancelledKey = "public_search_cancelled";

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
    }

    protected override void Deinitialize()
    {
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
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
        foreach (var data in obj.Data)
        {
            if (PrivateLobby != null && obj.Id == PrivateLobby.Value.Id.Value) //private lobby message received
            {
                if (data.Key == privateLobbyStartedKey)
                {
                    //TODO move this
                    SceneLoader.Instance.LoadScene("Lobby");
                }
                else if (data.Key == publicSearchStartedKey)
                {
                    HostCreatedPublicMatch(ulong.Parse(data.Value));
                }
                else if (data.Key == publicSearchCancelledKey)
                {
                    HostCancelledPublicMatch();
                }
            }
            else if (PublicLobby != null && obj.Id == PublicLobby.Value.Id.Value) //public lobby message received
            {

            }
        }
    }

    private void HostCreatedPublicMatch(ulong lobbyID)
    {
        LobbyQuery lobbyQuery = SteamMatchmaking.LobbyList.WithSlotsAvailable(MaxLobbyMembers - PrivateLobby.Value.Members.Count()); //we can assign rules to this query
        retrievingLobbiesTask = lobbyQuery.RequestAsync();
        retreivedLobbiesCallback = (Lobby[] lobbies) =>
        {
            foreach (var lobby in lobbies)
            {
                if (lobby.Id.Value == lobbyID)
                {
                    lobby.Join();
                    break;
                }
            }
        };

        OnBeganSearch?.Invoke();
    }

    private void HostCancelledPublicMatch()
    {
        PublicLobby = null;
        OnCancelledSearch?.Invoke();
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
        //PrivateLobby.Value.SetFriendsOnly(); TODO UNCOMMENT

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
        OnCancelledSearch?.Invoke();
        PrivateLobby.Value.SetData(publicSearchCancelledKey, "true");
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
        PrivateLobby.Value.SetData(publicSearchStartedKey, lobby.Value.Id.Value.ToString());

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

    public void LeaveAllLobbies()
    {
        LeavePrivateLobby();
        LeavePublicLobby();
    }

    private void OnApplicationQuit()
    {
        LeaveAllLobbies();
    }
}
