using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using TMPro;
using UnityEngine;

public class HUDPanel : Panel
{
    [SerializeField]
    private Timer countDownTimer;

    [SerializeField]
    private GameObject playerCellsParent;

    [SerializeField]
    private PlayerVarCell[] playerCells;

    private void Start()
    {
        for (int i = 0; i < ServerManager.Instance.Players.Count; i++)
        {
            playerCells[i].Configure(ServerManager.Instance.Players[i].PlayerID);
        }
    }

    public void BeginFightCountdown(float targetTime)
    {
        countDownTimer.Configure(targetTime, false, true);
    }

    public void HideCountdownText()
    {
        countDownTimer.gameObject.SetActive(false);
    }

    public void HidePlayerCells(bool hide)
    {
        playerCellsParent.SetActive(!hide);
    }
}
