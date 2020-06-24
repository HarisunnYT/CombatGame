using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinsPanel : Panel
{
    [SerializeField]
    private PlayerWinsCell[] playerCells;

    public override void Initialise()
    {
        for (int i = 0; i < LobbyManager.Instance.Players.Count; i++)
        {
            playerCells[i].Configure(LobbyManager.Instance.Players[i]);
        }
    }

    public void BeginBuyPhase()
    {
        MatchManager.Instance.BeginPhase(MatchManager.RoundPhase.Buy_Phase);
    }
}
