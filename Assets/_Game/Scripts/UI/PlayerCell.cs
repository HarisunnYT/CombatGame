﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCell : MonoBehaviour
{
    [SerializeField]
    private Image playerIcon;

    [SerializeField]
    private TMP_Text playerNameText;

    protected uint playerID;
    protected PlayerController playerController;

    public virtual void Configure(uint playerID)
    {
        this.playerID = playerID;
        LobbyManager.Instance.OnPlayerCreated += OnPlayerCreated;
    }

    protected virtual void OnPlayerCreated(uint playerID, PlayerController playerController)
    {
        if (playerID == this.playerID)
        {
            this.playerController = playerController;

            gameObject.SetActive(true);

            playerNameText.text = ServerManager.Instance.GetPlayerName(playerController.connectionToServer);
            playerIcon.sprite = playerController.Fighter.FigherIcon;

            playerController.OnPlayerDisconnected += OnPlayerDisconnected;
            LobbyManager.Instance.OnPlayerCreated -= OnPlayerCreated;
        }
    }

    private void OnDestroy()
    {
        playerController.OnPlayerDisconnected -= OnPlayerDisconnected;
    }

    protected void OnPlayerDisconnected()
    {
        try
        {
            gameObject.SetActive(false);
        }
        catch { }
    }
}
