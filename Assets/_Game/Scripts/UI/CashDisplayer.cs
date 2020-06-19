using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CashDisplayer : MonoBehaviour
{
    private TMP_Text cashText;

    private void Awake()
    {
        cashText = GetComponent<TMP_Text>();

        PlayerRoundInformation.Instance.OnCashUpdated += OnCashUpdated;
    }

    private void OnEnable()
    {
        OnCashUpdated(PlayerRoundInformation.Instance.Cash);
    }

    private void OnDestroy()
    {
        if (PlayerRoundInformation.Instance)
        {
            PlayerRoundInformation.Instance.OnCashUpdated -= OnCashUpdated;
        }
    }

    private void OnCashUpdated(int newAmount)
    {
        cashText.text = "<color=#16FF00>$</color> " + Util.FormatToCurrency(newAmount);
    }
}
