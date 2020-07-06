using Mirror;
using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterSelectManager : Singleton<CharacterSelectManager>
{
    [SerializeField]
    private int characterSelectTime = 20;
    public int CharacterSelectTime { get { return characterSelectTime; } }

    public delegate void CharacterEvent(int playerID, string characterName);
    public event CharacterEvent OnCharacterSelected;
    public event CharacterEvent OnCharacterUnselected;

    private bool matchLoaded = false;

    private void Start()
    {
        if (ServerManager.Instance.IsOnlineMatch)
        {
            if (SteamLobbyManager.Instance.PublicHost)
                NetworkManager.Instance.StartHost();
            else
                CreateClient();
        }
        else
        {
            NetworkManager.Instance.StartHost();
        }
    }

    private void Update()
    {
        if (!matchLoaded && (!ServerManager.Instance.IsOnlineMatch || ServerManager.Instance.Players.Count >= SteamLobbyManager.Instance.PublicLobby.Value.MemberCount))
        {
            PanelManager.Instance.ShowPanel<CharacterSelectScreen>();
            matchLoaded = true;
        }
    }

    private void CreateClient()
    {
        NetworkManager.Instance.networkAddress = SteamLobbyManager.Instance.PublicLobby.Value.Owner.Id.Value.ToString();
        NetworkManager.Instance.StartClient();
    }

    public void CharacterSelected(int playerID, string characterName)
    {
        foreach(var player in ServerManager.Instance.Players)
        {
            if (player.Figher == characterName)
                return;
        }

        if (ServerManager.Instance.IsOnlineMatch && playerID == NetworkManager.Instance.RoomPlayer.index)
        {
            NetworkManager.Instance.RoomPlayer.CmdChangeReadyState(true);
            CursorManager.Instance.HideAllCursors();
        }

        ServerManager.Instance.GetPlayer(playerID).Figher = characterName;
        OnCharacterSelected?.Invoke(playerID, characterName);
    }

    public void CharacterUnselected(int playerID, string characterName)
    {
        if (ServerManager.Instance.IsOnlineMatch && playerID == NetworkManager.Instance.RoomPlayer.index)
            NetworkManager.Instance.RoomPlayer.CmdChangeReadyState(false);

        ServerManager.Instance.GetPlayer(playerID).Figher = "";
        OnCharacterUnselected?.Invoke(playerID, characterName);
    }

    /// <summary>
    /// this is for exiting lobby, exiting match is on the MatchManager
    /// </summary>
    public void ExitLobby(bool forced)
    {
        if ((!ServerManager.Instance.IsOnlineMatch || SteamLobbyManager.Instance.PrivateHost) && NetworkManager.Instance)
            NetworkManager.Instance.StopHost();

        ServerManager.Instance.DestroyInstance();
        CursorManager.Instance.DestroyInstance();
        LocalPlayersManager.Instance.DestroyInstance();

        SteamLobbyManager.Instance.LeavePublicLobby();

        if (forced)
            ErrorManager.Instance.DisconnectedError();

        NetworkManager.Instance.StopClient();
        StartCoroutine(DelayedRemovalOfInstances());
    }

    private IEnumerator DelayedRemovalOfInstances()
    {
        yield return new WaitForEndOfFrame();

        Destroy(NetworkManager.Instance.gameObject);
        NetworkManager.Instance = null;

        yield return new WaitForEndOfFrame();

        CharacterSelectManager.Instance = null;
        Destroy(gameObject);

        SceneLoader.Instance.LoadScene("MainMenu");
    }

    private void OnApplicationQuit()
    {
        if (MatchManager.Instance)
            MatchManager.Instance.ExitMatch(false);
        else
            ExitLobby(false);
    }
}
