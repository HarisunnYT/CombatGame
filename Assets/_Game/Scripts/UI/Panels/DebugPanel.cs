using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugPanel : Panel
{
    [SerializeField]
    private TMP_Text equipMoveButtonText;

    private void Update()
    {
        if (Input.GetButtonDown("Menu"))
        {
            Close();
        }
    }

    protected override void OnShow()
    {
        CursorManager.Instance.ShowAllCursors();
    }

    protected override void OnClose()
    {
        if (CursorManager.Instance)
            CursorManager.Instance.HideAllCursors();
    }

    public void AutoWin()
    {
        FightManager.Instance.FightOver(ServerManager.Instance.GetPlayer(FightManager.Instance.AlivePlayers[0]).PlayerID);
        ForceClose();
    }

    public void AddCash(int amount)
    {
        foreach(var player in ServerManager.Instance.Players)
            player.PlayerController.PlayerRoundInfo.AddCash(amount);
    }

    public void InstantHost()
    {
        NetworkManager.Instance.StartHost();
    }

    public void HealAllPlayers()
    {
        foreach (var player in ServerManager.Instance.Players)
            player.PlayerController.AddHealth(100);
    }

    int moveIndex = 0;
    public void EquipMove()
    {
        if (moveIndex >= ServerManager.Instance.Players[0].PlayerController.Fighter.Moves.Length)
            moveIndex = 0;

        foreach (var player in ServerManager.Instance.Players)
        {
            if (moveIndex < player.PlayerController.Fighter.Moves.Length)
            {
                MoveData move = player.PlayerController.Fighter.Moves[moveIndex];
                player.PlayerController.PlayerRoundInfo.EquipedMove(move, 1);
            }
        }

        moveIndex++;
        equipMoveButtonText.text = "Equip Move " + moveIndex.ToString();

        ForceClose();
    }
}
