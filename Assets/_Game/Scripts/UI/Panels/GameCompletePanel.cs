using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameCompletePanel : Panel
{
    [SerializeField]
    private PlayerWinsCell winnerCell;

    [SerializeField]
    private PlayerWinsCell[] runnerUpCells;

    protected override void OnShow()
    {
        base.OnShow();

        var result = MatchManager.Instance.MatchResults.OrderByDescending(x => x.Value);
        winnerCell.Configure(ServerManager.Instance.GetPlayer(result.ElementAt(0).Key).PlayerID);

        if (ServerManager.Instance.Players.Count > 1)
        {
            for (int i = 0; i < runnerUpCells.Length; i++)
            {
                if (i < result.Count() - 1) //minus 1 as it's an index
                {
                    KeyValuePair<PlayerController, int> player = result.ElementAt(i + 1);
                    runnerUpCells[i].Configure(ServerManager.Instance.GetPlayer(player.Key).PlayerID);
                }
            }
        }

        StartCoroutine(KickBackToMenu());
    }

    //TODO probably do something better that involves user input
    private IEnumerator KickBackToMenu()
    {
        yield return new WaitForSecondsRealtime(5);

        ExitManager.Instance.ExitMatch(ExitType.Leave);
    }
}
