using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorPanel : Panel
{
    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private TMP_Text descriptionText;

    public void ShowPanel(ErrorData errorData)
    {
        titleText.text = errorData.ErrorCode;
        descriptionText.text = errorData.ErrorMessages[Random.Range(0, errorData.ErrorMessages.Length)];

        ShowPanel();
    }

    public void Ok()
    {
        Close();
    }
}
