using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CashDisplayer : MonoBehaviour
{
    private TMP_Text cashText;
    private PlayerRoundInformation playerRoundInformation;

    private void Awake()
    {
        if (ServerManager.Instance.IsOnlineMatch)
            playerRoundInformation = ServerManager.Instance.GetPlayerLocal().PlayerController.PlayerRoundInfo;
        else
        {
            LevelEditorCamera cam = GetComponentInParent<LevelEditorCamera>();
            if (cam)
            playerRoundInformation = ServerManager.Instance.GetPlayer(cam.LocalPlayerIndex).PlayerController.PlayerRoundInfo;
        }

        cashText = GetComponent<TMP_Text>();
        playerRoundInformation.OnCashUpdated += OnCashUpdated;
    }

    private void OnEnable()
    {
        if (playerRoundInformation)
            OnCashUpdated(playerRoundInformation.Cash);
    }

    private void OnDestroy()
    {
        if (playerRoundInformation)
            playerRoundInformation.OnCashUpdated -= OnCashUpdated;
    }

    private void OnCashUpdated(int newAmount)
    {
        cashText.text = "<color=#16FF00>$</color> " + Util.FormatToCurrency(newAmount);
    }
}
