using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FightManager : Singleton<FightManager>, IFightEvents
{
    private List<PlayerController> alivePlayers = new List<PlayerController>();

    protected override void Initialize()
    {
        alivePlayers = MatchManager.Instance.Players.Values.ToList();

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
            CameraManager.Instance.CameraFollow.ZoomInOnPlayer(alivePlayers[0].gameObject, new Vector2(0, 0.75f), 2, 1, () =>
             {
                 MatchManager.Instance.BeginPhase(MatchManager.RoundPhase.Buy_Phase);
             });
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
