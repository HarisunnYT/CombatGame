using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchManager : Singleton<MatchManager>
{
    #region EXTENSIONS
    public enum RoundPhase
    {
        Fight_Phase,
        Buy_Phase
    }

    #endregion

    #region EXPOSED_VARIABLES

    [SerializeField]
    private SpawnPosition[] spawnPositions;

    #endregion

    #region COMPONENTS

    #endregion

    #region RUNTIME_VARIABLES

    public int WinsRequired { get; private set; } = 5;

    public Dictionary<uint, PlayerController> Players = new Dictionary<uint, PlayerController>();

    private FightManager currentFight;
    private RoundPhase currentPhase;

    #endregion

    public void BeginMatch()
    {
        if (!ServerManager.Instance.IsOnlineMatch)
        {
            //we start at 1 as the main player has already spawned
            for (int i = 1; i < LocalPlayersManager.Instance.LocalPlayersCount; i++)
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
        CursorManager.Instance.HideAllCursors();

        //place players in spawns
        for (int i = 0; i < Players.Count; i++)
        {
            spawnPositions[i].SetPlayerSpawn(Players.ElementAt(i).Value);
        }

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

        spawnPositions[Players.Count - 1].SetPlayerSpawn(Players.ElementAt(Players.Count - 1).Value);
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
