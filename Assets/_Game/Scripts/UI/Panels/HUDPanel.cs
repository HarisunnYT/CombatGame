using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using TMPro;
using UnityEngine;

public class HUDPanel : Panel
{
    [SerializeField]
    private TMP_Text countDownText;

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

    public void UpdateCountdownText(string text)
    {
        countDownText.gameObject.SetActive(true);
        countDownText.text = text;
    }

    public void HideCountdownText()
    {
        countDownText.gameObject.SetActive(false);
    }

    public void HidePlayerCells(bool hide)
    {
        playerCellsParent.SetActive(!hide);
    }
}
