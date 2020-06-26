using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

public class CharacterPurchasePanel : Panel
{
    //this is used for local player only
    [SerializeField]
    private int playerIndex = -1;

    [SerializeField]
    private TMP_Text countdownTimer;

    [Space()]
    [SerializeField]
    private Transform movesContent;

    [SerializeField]
    private PurchasableMoveCell moveCellPrefab;

    [SerializeField]
    private GameObject fullDarkness;

    public PurchasableMoveCell CurrentPurchasingMove { get; private set; }

    private void Awake()
    {
        MoveData[] moves;

        if (ServerManager.Instance.IsOnlineMatch)
            moves = MatchManager.Instance.GetClientPlayer().Fighter.Moves;
        else
            moves = MatchManager.Instance.GetPlayer(playerIndex).Fighter.Moves;

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
        SetDarkness(false);
        CursorManager.Instance.ShowAllCursors();
        MatchManager.Instance.OnBuyPhaseTimePassed += UpdateCountdownTimer;
    }

    protected override void OnClose()
    {
        CursorManager.Instance.HideAllCursors();
        MatchManager.Instance.OnBuyPhaseTimePassed -= UpdateCountdownTimer;
    }

    public void UpdateCountdownTimer(int time)
    {
        countdownTimer.text = time.ToString();
    }

    public void PurchasingMove(PurchasableMoveCell move)
    {
        SetDarkness(true);
        CurrentPurchasingMove = move;
    }

    public void SetDarkness(bool darkness)
    {
        fullDarkness.gameObject.SetActive(darkness);
    }

    public void PurchasedMove(MoveData move, int position)
    {
        PlayerRoundInformation.Instance.RemoveCash(move.Price);

        CurrentPurchasingMove = null;
        SetDarkness(false);

        FighterManager.Instance.EquipedMove(move, position);
    }
}
