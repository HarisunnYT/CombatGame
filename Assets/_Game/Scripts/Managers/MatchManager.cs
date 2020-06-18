using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : Singleton<MatchManager>
{
    public enum RoundPhase
    {
        Fight_Phase,
        Buy_Phase
    }

    public int WinsRequired { get; private set; } = 5;

    private PlayerRoundInformation playerRoundInformation;

    private FightManager currentFight;

    private RoundPhase currentPhase;

    private void Start()
    {
        //TODO BEGIN ROUND WHEN PLAYERS ARE READY
        BeginPhase(RoundPhase.Fight_Phase);

        //create the player round info ... stores info like current cash, wins etc
        GameObject manager = new GameObject("Player Round Information");
        playerRoundInformation = manager.AddComponent<PlayerRoundInformation>();
    }

    public void BeginPhase(RoundPhase phase)
    {
        if (phase == RoundPhase.Fight_Phase)
            BeginFightPhase();
        else
            BeginBuyPhase();

        currentPhase = phase;
    }

    private void BeginFightPhase()
    {
        GameObject manager = new GameObject("Fight Manager");
        currentFight = manager.AddComponent<FightManager>();
    }

    private void BeginBuyPhase()
    {

    }
}
