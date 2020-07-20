﻿using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchManager : Singleton<MatchManager>
{
    #region EXTENSIONS
    public enum RoundPhase
    {
        Fight_Phase,
        Buy_Phase
    }

    #endregion

    #region EXPOSED_VARIABLES

    [SerializeField]
    private int buyPhaseTimeInSeconds = 45;
    public int BuyPhaseTimeInSeconds { get { return buyPhaseTimeInSeconds; } }

    [SerializeField]
    private SpawnPosition[] spawnPositions;

    #endregion

    #region COMPONENTS

    #endregion

    #region RUNTIME_VARIABLES

    public int WinsRequired { get; private set; } = 5;

    public bool MatchStarted { get; private set; } = false;
    public bool FightStarted { get; private set; } = false;

    //int being the amount of wins the player has
    private Dictionary<PlayerController, int> wins = new Dictionary<PlayerController, int>();
    public Dictionary<PlayerController, int> MatchResults { get { return wins; } }

    private FightManager currentFight;
    private RoundPhase currentPhase;

    private float buyPhaseCountdownTimer = 0;

    private int spawnIndex = 0;

    #endregion

    #region CALLBACKS

    public delegate void PhaseEvent(RoundPhase phase);
    public event PhaseEvent OnPhaseChanged;

    #endregion

    private void Start()
    {
        if (!ServerManager.Instance.IsOnlineMatch)
        {
            //we start at 1 as the main player has already spawned
            for (int i = 1; i < LocalPlayersManager.Instance.LocalPlayersCount; i++)
            {
                NetworkManager.Instance.OnServerAddPlayer(NetworkClient.connection);
            }
        }
    }

    public void BeginMatch()
    {
        BeginPhase(RoundPhase.Fight_Phase);
        MatchStarted = true;
    }

    public void BeginPhase(RoundPhase phase)
    {
        if (SteamLobbyManager.Instance.PublicHost)
        {
            NetworkManager.Instance.RoomPlayer.RpcBeginPhase((int)phase);
        }
    }

    /// <summary>
    /// call CustomNetworkRoomPlayer RpcBeginPhase instead
    /// </summary>
    /// <param name="phase"></param>
    public void BeginPhaseClient(RoundPhase phase)
    {
        if (phase == RoundPhase.Fight_Phase)
            BeginFightPhase();
        else
            BeginBuyPhase();

        currentPhase = phase;
        OnPhaseChanged?.Invoke(currentPhase);
    }

    private void BeginFightPhase()
    {
        CameraManager.Instance.CameraFollow.ResetCamera();
        CursorManager.Instance.Cursors.ResetCamera();

        LevelEditorManager.Instance.RevealRecentObjects();

        if (ServerManager.Instance.IsOnlineMatch)
            PanelManager.Instance.ClosePanel<CharacterPurchasePanel>();
        else
            LocalPlayerUIManager.Instance.DisplayLocalScreens(false);

        CreateFightManager();
    }

    private void BeginBuyPhase()
    {
        spawnIndex = 0;

        foreach (var player in ServerManager.Instance.Players)
        {
            player.PlayerController.ResetCharacter();
        }

        buyPhaseCountdownTimer = (float)NetworkTime.time + buyPhaseTimeInSeconds;

        if (ServerManager.Instance.IsOnlineMatch)
            PanelManager.Instance.ShowPanel<CharacterPurchasePanel>();
        else
            LocalPlayerUIManager.Instance.DisplayLocalScreens(true);

        CursorManager.Instance.ShowAllCursors();
        currentFight.AlivePlayers.Clear();

        Destroy(currentFight.gameObject);
        currentFight = null;

        FightStarted = false;

        StartCoroutine(DisablePlayerObjects());
    }

    private void BuyPhaseFinished()
    {
        CursorManager.Instance.HideAllCursors();
        BeginPhase(RoundPhase.Fight_Phase);
    }

    private IEnumerator DisablePlayerObjects()
    {
        yield return new WaitForSecondsRealtime(1);

        foreach(var player in ServerManager.Instance.Players)
        {
            player.PlayerController.gameObject.SetActive(false);
        }
    }

    private void CreateFightManager()
    {
        if (currentFight == null)
        {
            GameObject manager = new GameObject("Fight Manager");
            currentFight = manager.AddComponent<FightManager>();
        }
    }

    private void Update()
    {
        if (currentPhase == RoundPhase.Buy_Phase)
        {
            int roundedTime = Mathf.RoundToInt(buyPhaseCountdownTimer - (float)NetworkTime.time);
            if (roundedTime <= 0)
            {
                BuyPhaseFinished();
            }
        }
    }

    public void AddWin(PlayerController player)
    {
        if (wins.ContainsKey(player))
            wins[player]++;
        else
            wins.Add(player, 1);
    }

    public int GetWins(PlayerController player)
    {
        if (player != null && wins.ContainsKey(player))
            return wins[player];
        else
            return 0;
    }

    public void SetPlayerSpawn(PlayerController player)
    {
        spawnPositions[spawnIndex++].SetPlayerSpawn(player);
    }

    public bool HasPlayerWon()
    {
        foreach(var player in wins)
        {
            if (player.Value >= WinsRequired)
            {
                return true;
            }
        }

        return false;
    }

    public void OnFightStarted()
    {
        FightStarted = true;
    }

    #region PLAYER_ASSIGNMENTS

    public void AddPlayer(PlayerController player, int id)
    {
        ServerManager.Instance.GetPlayer(id).PlayerController = player;

        CreateFightManager(); //lazy initialise fight manager

        if (!currentFight.AlivePlayers.Contains(id))
            currentFight.AlivePlayers.Add(id);

        SetPlayerSpawn(player);
        wins.Add(player, 0);
    }

    public PlayerController GetPlayer(int playerID)
    {
        return ServerManager.Instance.GetPlayer(playerID).PlayerController;
    }

    public int GetPlayerID(PlayerController player)
    {
        return ServerManager.Instance.GetPlayer(player).PlayerID;
    }

    public PlayerController GetClientPlayer()
    {
        foreach (var player in ServerManager.Instance.Players)
        {
            if (player.PlayerController.isLocalPlayer)
            {
                return player.PlayerController;
            }
        }

        return null;
    }

    #endregion
}
