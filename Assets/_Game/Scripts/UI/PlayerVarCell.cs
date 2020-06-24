using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerVarCell : MonoBehaviour
{
    [SerializeField]
    private Image playerIcon;

    [SerializeField]
    private TMP_Text playerNameText;

    [SerializeField]
    private Image healthBar;

    private uint playerID;
    private PlayerController playerController;

    public void Configure(uint playerID)
    {
        this.playerID = playerID;

        LobbyManager.Instance.OnPlayerCreated += OnPlayerCreated;
    }

    private void OnPlayerCreated(uint playerID, PlayerController playerController)
    {
        //only create this cell if it's the correct player id it's assigned to
        if (playerID == this.playerID)
        {
            this.playerController = playerController;

            playerNameText.text = ServerManager.Instance.GetPlayerName(playerController.connectionToServer);
            playerIcon.sprite = playerController.Fighter.FigherIcon;

            gameObject.SetActive(true);

            playerController.OnPlayerDisconnected += OnPlayerDisconnected;
            playerController.OnHealthChanged += OnHealthChanged;

            LobbyManager.Instance.OnPlayerCreated -= OnPlayerCreated;
        }
    }

    private void OnHealthChanged(int health)
    {
        healthBar.fillAmount = (float)health / 100;
    }

    private void OnPlayerDisconnected()
    {
        if (gameObject)
            gameObject.SetActive(false);
    }
}
