using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using TMPro;
using UnityEngine;

public class HUDPanel : Panel
{
    [SerializeField]
    private TMP_Text countDownText;

    public void UpdateCountdownText(string text)
    {
        countDownText.gameObject.SetActive(true);
        countDownText.text = text;
    }

    public void HideCountdownText()
    {
        countDownText.gameObject.SetActive(false);
    }
}
