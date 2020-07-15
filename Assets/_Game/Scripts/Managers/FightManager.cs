using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FightManager : Singleton<FightManager>, IFightEvents
{
    private const float startFightCountDownTimeInSeconds = 3.5f;

    //int == player id
    public List<int> AlivePlayers { get; private set; } = new List<int>();

    private bool startFightCountdownInProgress = false;
    private float startFightCountdownTimer = 0;

    private HUDPanel hudPanel;

    private bool fightOver = false;

    private void Start()
    {
        hudPanel = PanelManager.Instance.GetPanel<HUDPanel>();

        AddListener();
        BeginFightCountdown();

        hudPanel.ShowPanel();
        hudPanel.HidePlayerCells(false);

        if (AlivePlayers.Count == 0)
        {
            foreach(var player in ServerManager.Instance.Players)
            {
                AlivePlayers.Add(player.PlayerID);

                if (player.PlayerController != null) //there's a chance it may not exist yet if it's the first round
                {
                    MatchManager.Instance.SetPlayerSpawn(player.PlayerController);
                }
            }
        }
    }

    protected override void OnDestroy()
    {
        RemoveListener();
        base.OnDestroy();
    }

    private void Update()
    {
        if (startFightCountdownInProgress)
        {
            int roundedTime = Mathf.RoundToInt(startFightCountdownTimer - (float)NetworkTime.time);
            if (roundedTime <= 0)
            {
                CountdownOver();
            }
        }
        else
        {
            if (!fightOver && ServerManager.Instance.Players.Count <= 1 && ExitManager.Instance.ExitType == ExitType.None)
            {
                GameComplete();
            }
        }
    }

    private void BeginFightCountdown()
    {
        startFightCountdownInProgress = true;
        startFightCountdownTimer = (float)NetworkTime.time + startFightCountDownTimeInSeconds;

        hudPanel.BeginFightCountdown(startFightCountdownTimer);
    }

    private void CountdownOver()
    {
        startFightCountdownInProgress = false;
        hudPanel.HideCountdownText();

        MatchManager.Instance.OnFightStarted();

        foreach(var player in AlivePlayers)
        {
            if (ServerManager.Instance)
            {
                PlayerController playerController = ServerManager.Instance.GetPlayer(player).PlayerController;
                playerController.EnableInput();
                playerController.SetAlive();
            }
        }
    }

    public void OnPlayerDied(PlayerController killer, PlayerController victim)
    {
        //give the dead player their placement cash
        DetermineCashForPlayer(victim, AlivePlayers.Count);

        //remove player from alive players and see if there's only a single player left
        AlivePlayers.Remove(ServerManager.Instance.GetPlayer(victim).PlayerID);
        if (AlivePlayers.Count <= 1)
        {
            if (SteamLobbyManager.Instance.PublicHost)
                NetworkManager.Instance.RoomPlayer.CmdFightOver(ServerManager.Instance.GetPlayer(AlivePlayers[0]).PlayerID);
            else if (!ServerManager.Instance.IsOnlineMatch)
                FightOver(ServerManager.Instance.GetPlayer(AlivePlayers[0]).PlayerID);
        }
    }

    public void FightOver(int winnerPlayerID)
    {
        if (fightOver)
            return;

        fightOver = true;
        PlayerController winner = ServerManager.Instance.GetPlayer(winnerPlayerID).PlayerController;

        winner.DisableInput();
        MatchManager.Instance.AddWin(winner);

        hudPanel.HidePlayerCells(true);
        DetermineCashForPlayer(winner, 1);

        CameraManager.Instance.CameraFollow.ZoomInOnPlayer(winner.gameObject, new Vector2(0, 0.75f), 2, 1, () =>
        {
            if (MatchManager.Instance.HasPlayerWon())
                GameComplete();
            else
                PanelManager.Instance.ShowPanel<WinsPanel>();
        });
    }

    private void GameComplete()
    {
        PanelManager.Instance.ShowPanel<GameCompletePanel>();
    }

    private void DetermineCashForPlayer(PlayerController player, int placement)
    {
        //each client will do this, we only need to determine that cash for this client
        if (player.isLocalPlayer || !ServerManager.Instance.IsOnlineMatch)
        {
            player.PlayerRoundInfo.AddPlacementCash(placement);
        }
    }

    public void AddListener()
    {
        CombatInterfaces.AddListener(this);
    }

    public void RemoveListener()
    {
        CombatInterfaces.RemoveListener(this);
    }
}
