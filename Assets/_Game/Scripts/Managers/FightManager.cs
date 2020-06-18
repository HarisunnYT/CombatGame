using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightManager : Singleton<FightManager>, IFightEvents
{
    private List<PlayerController> alivePlayers = new List<PlayerController>();

    protected override void Initialize()
    {
        alivePlayers = GameManager.Instance.Players;

        AddListener();
    }

    protected override void OnDestroy()
    {
        RemoveListener();
        base.OnDestroy();
    }

    public void OnPlayerDied(PlayerController player)
    {
        alivePlayers.Remove(player);

        if (alivePlayers.Count <= 1)
        {
            //TODO add win camera zoom in and stuff
            MatchManager.Instance.BeginPhase(MatchManager.RoundPhase.Buy_Phase);
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
