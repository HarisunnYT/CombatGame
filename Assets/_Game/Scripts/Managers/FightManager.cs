using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FightManager : Singleton<FightManager>, IFightEvents
{
    [SerializeField]
    private int startFightCountDownTimeInSeconds = 3;

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
            int roundedTime = Mathf.RoundToInt(startFightCountdownTimer - Time.time);
            hudPanel.UpdateCountdownText(roundedTime.ToString());

            //countdown is finished
            if (roundedTime <= 0)
            {
                CountdownOver();
            }
        }
    }

    private void BeginFightCountdown()
    {
        startFightCountdownInProgress = true;
        startFightCountdownTimer = Time.time + startFightCountDownTimeInSeconds;
    }

    private void CountdownOver()
    {
        startFightCountdownInProgress = false;
        hudPanel.HideCountdownText();

        foreach(var player in AlivePlayers)
        {
            PlayerController playerController = ServerManager.Instance.GetPlayer(player).PlayerController;
            playerController.EnableInput();
            playerController.SetAlive();
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
        if (player.isLocalPlayer)
        {
            PlayerRoundInformation.Instance.AddPlacementCash(placement);
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
