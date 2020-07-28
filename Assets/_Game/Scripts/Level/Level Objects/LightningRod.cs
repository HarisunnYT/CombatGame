using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningRod : LevelObject
{
    [Space()]
    [SerializeField]
    private int intervalSeconds = 5;

    private Animator animator;
    private float targetTime = -1;

    private void Start()
    {
        animator = GetComponent<Animator>();
        MatchManager.Instance.OnPhaseChanged += OnPhaseChanged;
    }

    private void OnPhaseChanged(MatchManager.RoundPhase phase)
    {
        if (phase == MatchManager.RoundPhase.Fight_Phase)
            targetTime = ServerManager.Time + intervalSeconds;
        else
            targetTime = -1;
    }

    protected override void Update()
    {
        base.Update();

        if (ServerManager.Instance.IsOnlineMatch && !isServer)
            return;

        if (targetTime != -1 && ServerManager.Time > targetTime)
        {
            animator.SetTrigger("Strike");
            targetTime = ServerManager.Time + intervalSeconds;
        }
    }
}
