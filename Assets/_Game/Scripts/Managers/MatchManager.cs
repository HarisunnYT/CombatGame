using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchManager : Singleton<MatchManager>
{
    public enum RoundPhase
    {
        Fight_Phase,
        Buy_Phase
    }

    public int WinsRequired { get; private set; } = 5;

    public Dictionary<uint, PlayerController> Players = new Dictionary<uint, PlayerController>();

    private FightManager currentFight;

    private RoundPhase currentPhase;

    public void BeginMatch()
    {
        if (!ServerManager.Instance.IsOnlineMatch)
        {
            //we start at 1 as the main player has already spawned
            for (int i = 1; i < LocalPlayersManager.Instance.LocalPlayers; i++)
            {
                NetworkManager.Instance.OnServerAddPlayer(NetworkClient.connection);
            }
        }

        BeginPhase(RoundPhase.Fight_Phase);
    }

    public void BeginPhase(RoundPhase phase)
    {
        if (phase == RoundPhase.Fight_Phase)
            BeginFightPhase();
        else
            BeginBuyPhase();

        currentPhase = phase;
    }

    private void BeginFightPhase()
    {
        CreateFightManager();
    }

    private void BeginBuyPhase()
    {
        PanelManager.Instance.ShowPanel<PurchasePhasePanel>();
    }

    private void CreateFightManager()
    {
        if (currentFight == null)
        {
            GameObject manager = new GameObject("Fight Manager");
            currentFight = manager.AddComponent<FightManager>();
        }
    }

    #region PLAYER_ASSIGNMENTS

    public void AddPlayer(PlayerController player, uint id)
    {
        Players.Add(id, player);

        CreateFightManager(); //lazy initialise fight manager

        currentFight.AlivePlayers.Add(player);
    }

    public void RemovePlayer(PlayerController player)
    {
        RemovePlayer(GetPlayerID(player));
    }

    public void RemovePlayer(uint playerID)
    {
        Players.Remove(playerID);
    }

    public PlayerController GetPlayer(uint playerID)
    {
        return Players[playerID];
    }

    public PlayerController GetPlayer(GameObject obj)
    {
        foreach (var player in Players)
        {
            if (player.Value.gameObject == obj)
            {
                return player.Value;
            }
        }

        return null;
    }

    public uint GetPlayerID(PlayerController player)
    {
        foreach(var p in Players)
        {
            if (p.Value == player)
            {
                return p.Key;
            }
        }

        return 0;
    }

    #endregion
}
