using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ExitType
{
    None,
    Leave,
    HostDisconnected,
    ClientDisconnected,

    HostLeftWithParty = 10,
    LeftLocal,
    LeftToJoinLobby
}

public class ExitManager : PersistentSingleton<ExitManager>
{
    public ExitType ExitType { get; private set; } = ExitType.None;

    private Coroutine exitMatchCoroutine;

    public void ExitMatch(ExitType exitType)
    {
        if (ExitType < ExitType.HostLeftWithParty) //anything after host left with party is special and isn't a disconnect
            ExitType = exitType;

        if (exitMatchCoroutine == null)
            exitMatchCoroutine = StartCoroutine(ExitMatchIE());
    }

    public void ExitMatchWithParty()
    {
        SteamLobbyManager.Instance.ExitMatchWithParty();
        ExitMatch(ExitType.HostLeftWithParty);
    }

    private IEnumerator ExitMatchIE()
    {
        TransitionManager.Instance.ShowTransition();

        yield return new WaitForSeconds(1f);

        if (!ServerManager.Instance.IsOnlineMatch || SteamLobbyManager.Instance.PublicHost)
            NetworkManager.Instance.StopHost();

        NetworkManager.Instance.StopClient();

        DestroySingletons();

        SteamLobbyManager.Instance.LeavePublicLobby();
        if (ExitType != ExitType.HostLeftWithParty && ExitType != ExitType.LeftToJoinLobby)
            SteamLobbyManager.Instance.LeavePrivateLobby();

        yield return StartCoroutine(DelayedRemoval());

        if (ExitType == ExitType.HostDisconnected || ExitType == ExitType.ClientDisconnected)
            ErrorManager.Instance.DisconnectedError();

        SceneLoader.Instance.LoadScene("MainMenu", () =>
        {
            if (ExitType == ExitType.HostLeftWithParty || ExitType == ExitType.LeftToJoinLobby)
                PanelManager.Instance.ShowPanel<PrivateLobbyPanel>();

            Destroy(gameObject);
        });
    }

    private void DestroySingletons()
    {
        ServerManager.Instance.DestroyInstance();
        CursorManager.Instance.DestroyInstance();
        LocalPlayersManager.Instance.DestroyInstance();

        if (FightManager.Instance)
            Destroy(FightManager.Instance.gameObject);
        if (CharacterSelectManager.Instance)
            Destroy(CharacterSelectManager.Instance.gameObject);
        if (MatchManager.Instance)
            Destroy(MatchManager.Instance.gameObject);
    }

    private IEnumerator DelayedRemoval()
    {
        yield return new WaitForEndOfFrame();

        Destroy(NetworkManager.Instance.gameObject);
        NetworkManager.Instance = null;

        yield return new WaitForEndOfFrame();
    }

    private void OnApplicationQuit()
    {
        ExitMatch(ExitType.ClientDisconnected);
    }
}
