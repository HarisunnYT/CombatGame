﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FightManager : Singleton<FightManager>, IFightEvents
{
    [SerializeField]
    private int countDownTimeInSeconds = 3;

    public List<PlayerController> AlivePlayers { get; private set; } = new List<PlayerController>();

    private bool countdownInProgress = false;
    private float countdownTimer = 0;

    private HUDPanel hudPanel;

    protected override void Initialize()
    {
        base.Initialize();

        hudPanel = PanelManager.Instance.GetPanel<HUDPanel>();

    }

    private void Start()
    {
        AddListener();
        BeginFightCountdown();
    }

    protected override void OnDestroy()
    {
        RemoveListener();
        base.OnDestroy();
    }

    private void Update()
    {
        if (countdownInProgress)
        {
            int roundedTime = Mathf.RoundToInt(countdownTimer - Time.time);
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
        countdownInProgress = true;
        countdownTimer = Time.time + countDownTimeInSeconds;
    }

    private void CountdownOver()
    {
        countdownInProgress = false;
        hudPanel.HideCountdownText();

        foreach(var player in AlivePlayers)
        {
            player.EnableInput();
        }
    }

    public void OnPlayerDied(PlayerController killer, PlayerController victim)
    {
        //give the dead player their placement cash
        DetermineCashForPlayer(victim, AlivePlayers.Count);

        //remove player from alive players and see if there's only a single player left
        AlivePlayers.Remove(victim);
        if (AlivePlayers.Count <= 1)
        {
            FightOver(AlivePlayers[0]);
        }
    }

    public void FightOver(PlayerController winner)
    {
        DetermineCashForPlayer(winner, 1);

        CameraManager.Instance.CameraFollow.ZoomInOnPlayer(winner.gameObject, new Vector2(0, 0.75f), 2, 1, () =>
        {
            MatchManager.Instance.BeginPhase(MatchManager.RoundPhase.Buy_Phase);
        });
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
