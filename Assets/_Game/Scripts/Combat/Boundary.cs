using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (MatchManager.Instance.FightStarted && (SteamLobbyManager.Instance.PublicHost || !ServerManager.Instance.IsOnlineMatch))
        {
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController)
                playerController.ForceKill();
        }
    }
}
