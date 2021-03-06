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

    public PlayerController PlayerController { get; private set; } 

    protected int playerID;

    public virtual void Configure(int playerID)
    {
        this.playerID = playerID;

        Invoke("TryAssignPlayerController", 0.1f);
    }

    private void TryAssignPlayerController()
    {
        if (ServerManager.Instance == null)
            return;

        if (PlayerController == null && ServerManager.Instance.GetPlayer(playerID).PlayerController != null)
            OnPlayerCreated(playerID, ServerManager.Instance.GetPlayer(playerID).PlayerController);
        else
            Invoke("TryAssignPlayerController", 0.1f);
    }

    protected virtual void OnPlayerCreated(int playerID, PlayerController playerController)
    {
        if (playerID == this.playerID)
        {
            this.PlayerController = playerController;

            gameObject.SetActive(true);

            playerNameText.text = ServerManager.Instance.GetPlayerName(playerID);
            playerIcon.sprite = playerController.Fighter.FigherIcon;

            playerController.OnPlayerDisconnected += OnPlayerDisconnected;
        }
    }

    private void OnDestroy()
    {
        PlayerController.OnPlayerDisconnected -= OnPlayerDisconnected;
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
