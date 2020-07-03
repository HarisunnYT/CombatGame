using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPanel : Panel
{
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
        PlayerRoundInformation.Instance.AddCash(amount);
    }

    public void InstantHost()
    {
        NetworkManager.Instance.StartHost();
    }
}
