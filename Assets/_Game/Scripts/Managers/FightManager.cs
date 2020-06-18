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
        AlivePlayers.Remove(victim);

        if (AlivePlayers.Count <= 1)
        {
            CameraManager.Instance.CameraFollow.ZoomInOnPlayer(AlivePlayers[0].gameObject, new Vector2(0, 0.75f), 2, 1, () =>
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
