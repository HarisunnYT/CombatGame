using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputBasedButton : MonoBehaviour
{
    [SerializeField]
    private TMP_Text keyText;

    [SerializeField]
    private Image buttonImage;

    private void Awake()
    {
        PlayerController playerController = GetComponentInParent<PlayerController>();
        playerController.OnInputProfileSet += () =>
        {
            keyText.gameObject.SetActive(playerController.InputProfile.GUID == default);
            buttonImage.gameObject.SetActive(playerController.InputProfile.GUID != default);
        };
    }
}
