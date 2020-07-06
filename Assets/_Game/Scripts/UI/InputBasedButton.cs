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
        if (playerController)
        {
            playerController.OnInputProfileSet += () =>
            {
                Configure(playerController.InputProfile);
            };
        }
    }

    public void Configure(InputProfile inputProfile)
    {
        keyText.gameObject.SetActive(inputProfile.GUID == default);
        buttonImage.gameObject.SetActive(inputProfile.GUID != default);

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
