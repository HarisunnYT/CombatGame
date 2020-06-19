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

    public void AutoWin()
    {
        FightManager.Instance.FightOver(FightManager.Instance.AlivePlayers[0]);
        Close();
    }

    public void AddCash(int amount)
    {
        PlayerRoundInformation.Instance.AddCash(amount);
    }
}
