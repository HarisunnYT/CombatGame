﻿using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public List<PlayerController> Players { get; private set; } = new List<PlayerController>();

    private MatchManager currentMatch;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
#if UNITY_EDITOR //disable networking if player prefab is in scene
        if (Players.Count > 0)
        {
            NetworkManager.singleton.StartHost();
            NetworkManager.singleton.HUD.HideGUI();

            Invoke("AutoSetPlayerPosition", 0.01f);
        }
#endif
    }

    //TODO REMOVE
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            BeginMatch();
        }
    }

    public void BeginMatch()
    {
        if (isServer)
            RpcCreateMatchManager();
        else
            CmdCreateMatchManager();
    }

    [Command]
    private void CmdCreateMatchManager()
    {
        RpcCreateMatchManager();
    }

    [ClientRpc]
    private void RpcCreateMatchManager()
    {
        GameObject manager = new GameObject("Match Manager");
        currentMatch = manager.AddComponent<MatchManager>();
    }

    public void AddPlayer(PlayerController player)
    {
        Players.Add(player);
    }

    public void RemovePlayer(PlayerController player)
    {
        Players.Remove(player);
    }

    public bool IsPlayer(GameObject obj)
    {
        return obj.layer == LayerMask.NameToLayer("Player");
    }

    public PlayerController GetPlayerFromObject(GameObject obj)
    {
        foreach(var player in Players)
        {
            if (player.gameObject == obj)
            {
                return player;
            }
        }

        return null;
    }

#if UNITY_EDITOR
    //this is used for when the player prefab is in the main scene, 
    //it auto sets the hosts player position to player in scene position
    private void AutoSetPlayerPosition()
    {
        PlayerController scenePlayer = Players[0];
        PlayerController hostPlayer = Players[1];

        hostPlayer.transform.position = scenePlayer.transform.position;
        Players.Remove(scenePlayer);
        Destroy(scenePlayer.gameObject);
    }
#endif
}
