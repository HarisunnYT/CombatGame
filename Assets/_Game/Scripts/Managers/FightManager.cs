using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FightManager : Singleton<FightManager>, IFightEvents
{
    public List<PlayerController> AlivePlayers { get; private set; } = new List<PlayerController>();

    private void Start()
    {
        AddListener();
    }

    protected override void OnDestroy()
    {
        RemoveListener();
        base.OnDestroy();
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
