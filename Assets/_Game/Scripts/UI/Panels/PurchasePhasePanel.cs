using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

public class PurchasePhasePanel : Panel
{
    [SerializeField]
    private TMP_Text countdownTimer;

    [Space()]
    [SerializeField]
    private Transform movesContent;

    [SerializeField]
    private PurchasableMoveCell moveCellPrefab;

    private void Awake()
    {
        MoveData[] moves = MatchManager.Instance.GetClientPlayer().Fighter.Moves;
        foreach (var move in moves.OrderBy(x => x.Price)) //sort by lowest price first
        {
            CreateCell(move);
        }
    }

    private void CreateCell(MoveData move)
    {
        PurchasableMoveCell moveCell = Instantiate(moveCellPrefab, movesContent);
        moveCell.Configure(move);
    }

    protected override void OnShow()
    {
        CursorManager.Instance.ShowAllCursors();
    }

    protected override void OnClose()
    {
        CursorManager.Instance.HideAllCursors();
    }

    public void UpdateCountdownTimer(string time)
    {
        countdownTimer.text = time;
    }
}
