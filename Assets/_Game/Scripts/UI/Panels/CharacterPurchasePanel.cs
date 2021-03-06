﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using Mirror;

public class CharacterPurchasePanel : Panel
{
    [SerializeField]
    private Timer timer;

    [SerializeField]
    private LevelEditorCamera levelEditorCamera;

    [Space()]
    [SerializeField]
    private Transform movesContent;

    [SerializeField]
    private PurchasableMoveCell moveCellPrefab;

    [SerializeField]
    private GameObject fullDarkness;

    public PurchasableMoveCell CurrentPurchasingMove { get; private set; }

    public override void Initialise()
    {
        MatchManager.Instance.OnPhaseChanged += OnPhaseChanged;
    }

    private void OnDestroy()
    {
        if (MatchManager.Instance)
            MatchManager.Instance.OnPhaseChanged -= OnPhaseChanged;
    }

    private void Awake()
    {
        MoveData[] moves = new MoveData[0];

        if (ServerManager.Instance.IsOnlineMatch)
            moves = MatchManager.Instance.GetClientPlayer().Fighter.Moves;
        else if (levelEditorCamera.LocalPlayerIndex != -1)
            moves = MatchManager.Instance.GetPlayer(levelEditorCamera.LocalPlayerIndex).Fighter.Moves;

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
    }

    private void OnPhaseChanged(MatchManager.RoundPhase phase)
    {
        if (phase == MatchManager.RoundPhase.Buy_Phase)
        {
            timer.Configure(ServerManager.Time + MatchManager.Instance.BuyPhaseTimeInSeconds);
        }
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
        ServerManager.ConnectedPlayer player = ServerManager.Instance.IsOnlineMatch ? ServerManager.Instance.GetPlayerLocal() : ServerManager.Instance.GetPlayer(levelEditorCamera.LocalPlayerIndex);

        CurrentPurchasingMove = null;
        SetDarkness(false);

        player.PlayerController.PlayerRoundInfo.Purchase(move.Price);
        player.PlayerController.PlayerRoundInfo.EquipedMove(move, position);
    }

    public void OpenLevelEditor()
    {
        if (ServerManager.Instance.IsOnlineMatch)
            PanelManager.Instance.ShowPanel<LevelEditorPanel>();
        else
        {
            transform.parent.GetComponentInChildren<LevelEditorPanel>(true).ShowPanel(); //if it's local, get the level editor panel that's on the same parent as this
            Close();
        }
    }
}
