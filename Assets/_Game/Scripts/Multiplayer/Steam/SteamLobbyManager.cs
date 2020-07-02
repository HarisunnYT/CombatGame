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

public class SteamLobbyManager : PersistentSingleton<SteamLobbyManager>
{
    #region EXTENSIONS

    #endregion

    #region CONST_VARIABLES

    private const int maxLobbyMembers = 4;

    public const string PrivateLobbyStatedKey = "private_lobby_started";

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
            //we found at least one suitable lobby
            if (retrievingLobbiesTask.Result != null && retrievingLobbiesTask.Result.Length > 0)
            {
                List<Lobby> availableLobbies = retrievingLobbiesTask.Result.ToList();

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

    public void PlayPrivateMatch()
    {
        PublicLobby = PrivateLobby;
        PrivateLobby.Value.SetData("private match", "true");

        SceneLoader.Instance.LoadScene("Lobby");
    }

    #region MESSAGES

    private void OnLobbyDataChanged(Lobby obj)
    {
        foreach (var data in obj.Data)
        {
            if (PrivateLobby != null && obj.Id == PrivateLobby.Value.Id.Value) //private lobby message received
            {
                Lobby? publicLobby = JsonUtility.FromJson<Lobby?>(data.Value);
                if (publicLobby != null) //if this isn't null, the message was a host created public match message
                    HostCreatedPublicMatch(publicLobby);

                if (data.Key == "private match")
                {
                    //TODO move this
                    SceneLoader.Instance.LoadScene("Lobby");
                }
            }
            else if (PublicLobby != null && obj.Id == PublicLobby.Value.Id.Value) //public lobby message received
            {

            }
        }
    }

    private void HostCreatedPublicMatch(Lobby? lobby)
    {
        lobby.Value.Join();
    }

    #endregion

    #region PRIVATE_LOBBY

    public void CreatePrivateLobby()
    {
        if (creatingPrivateLobbyTask != null)
            return;

        creatingPrivateLobbyTask = SteamMatchmaking.CreateLobbyAsync(maxLobbyMembers);
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

    #endregion

    #region MATCH_MAKING

    public void SearchForMatch()
    {
        searching = true;
        OnBeganSearch?.Invoke();

        //first we need to get a list of servers, if any are available
        LobbyQuery lobbyQuery = SteamMatchmaking.LobbyList.WithSlotsAvailable(maxLobbyMembers - PrivateLobby.Value.Members.Count()); //we can assign rules to this query
        retrievingLobbiesTask = lobbyQuery.RequestAsync();
    }

    public void CancelSearch()
    {
        searching = false;
        OnCancelledSearch?.Invoke();
    }

    private void CreatePublicMatchLobby()
    {
        if (PublicLobby != null)
            return;

        creatingPublicLobbyTask = SteamMatchmaking.CreateLobbyAsync(maxLobbyMembers);
    }

    private void CreatedPublicMatch(Lobby? lobby)
    {
        PublicLobby = lobby;
        PublicLobby.Value.SetPublic();

        //send a message to all members in the private lobby to join the public lobby that has been created
        PrivateLobby.Value.SetData("public match", JsonUtility.ToJson(lobby));

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
