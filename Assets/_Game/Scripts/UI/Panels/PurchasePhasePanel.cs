using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PurchasePhasePanel : Panel
{
    [SerializeField]
    private TMP_Text countdownTimer;

    public void UpdateCountdownTimer(string time)
    {
        countdownTimer.text = time;
    }
}
