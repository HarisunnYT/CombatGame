using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchManager : NetworkBehaviour
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
    private int buyPhaseTimeInSeconds = 45;
    public int BuyPhaseTimeInSeconds { get { return buyPhaseTimeInSeconds; } }

    [SerializeField]
    private SpawnPosition[] spawnPositions;

    #endregion

    #region COMPONENTS

    #endregion

    #region RUNTIME_VARIABLES

    public static MatchManager Instance;

    public int WinsRequired { get; private set; } = 5;

    //int being the amount of wins the player has
    private Dictionary<PlayerController, int> wins = new Dictionary<PlayerController, int>();
    public Dictionary<PlayerController, int> MatchResults { get { return wins; } }

    [SyncVar]
    private float time;
    public float Time { get { return time; } }

    private FightManager currentFight;
    private RoundPhase currentPhase;

    private CharacterPurchasePanel purchasePanel;

    private float buyPhaseCountdownTimer = 0;

    private int spawnIndex = 0;

    #endregion

    #region CALLBACKS

    public delegate void PhaseEvent(RoundPhase phase);
    public event PhaseEvent OnPhaseChanged;

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        purchasePanel = PanelManager.Instance.GetPanel<CharacterPurchasePanel>();

        if (!ServerManager.Instance.IsOnlineMatch)
        {
            //we start at 1 as the main player has already spawned
            for (int i = 1; i < LocalPlayersManager.Instance.LocalPlayersCount; i++)
            {
                NetworkManager.Instance.OnServerAddPlayer(NetworkClient.connection);
            }
        }
    }

    public void BeginMatch()
    {
        BeginPhase(RoundPhase.Fight_Phase);
    }

    public void BeginPhase(RoundPhase phase)
    {
        if (phase == RoundPhase.Fight_Phase)
            BeginFightPhase();
        else
            BeginBuyPhase();

        currentPhase = phase;
        OnPhaseChanged?.Invoke(currentPhase);
    }

    private void BeginFightPhase()
    {
        CameraManager.Instance.CameraFollow.ResetCamera();
        LevelEditorManager.Instance.RevealRecentObjects();

        if (ServerManager.Instance.IsOnlineMatch)
            purchasePanel.Close();
        else
            LocalPlayerUIManager.Instance.DisplayLocalScreens(false);

        CursorManager.Instance.HideAllCursors();

        spawnIndex = 0;

        CreateFightManager();
    }

    private void BeginBuyPhase()
    {
        foreach(var player in ServerManager.Instance.Players)
        {
            player.PlayerController.ResetCharacter();
        }

        buyPhaseCountdownTimer = time + buyPhaseTimeInSeconds;

        if (ServerManager.Instance.IsOnlineMatch)
            purchasePanel.ShowPanel();
        else
            LocalPlayerUIManager.Instance.DisplayLocalScreens(true);

        currentFight.AlivePlayers.Clear();

        Destroy(currentFight.gameObject);
        currentFight = null;

        StartCoroutine(DisablePlayerObjects());
    }

    private IEnumerator DisablePlayerObjects()
    {
        yield return new WaitForSecondsRealtime(1);

        foreach(var player in ServerManager.Instance.Players)
        {
            player.PlayerController.gameObject.SetActive(false);
        }
    }

    private void CreateFightManager()
    {
        if (currentFight == null)
        {
            GameObject manager = new GameObject("Fight Manager");
            currentFight = manager.AddComponent<FightManager>();
        }
    }

    private void Update()
    {
        if (currentPhase == RoundPhase.Buy_Phase)
        {
            int roundedTime = Mathf.RoundToInt(buyPhaseCountdownTimer - time);
            if (roundedTime <= 0)
            {
                BeginPhase(RoundPhase.Fight_Phase);
            }
        }
    }

    public void AddWin(PlayerController player)
    {
        if (wins.ContainsKey(player))
            wins[player]++;
        else
            wins.Add(player, 1);
    }

    public int GetWins(PlayerController player)
    {
        if (player != null && wins.ContainsKey(player))
            return wins[player];
        else
            return 0;
    }

    public void SetPlayerSpawn(PlayerController player)
    {
        spawnPositions[spawnIndex].SetPlayerSpawn(player);
        spawnIndex++;
    }

    public bool HasPlayerWon()
    {
        foreach(var player in wins)
        {
            if (player.Value >= WinsRequired)
            {
                return true;
            }
        }

        return false;
    }

    private void FixedUpdate()
    {
        time += UnityEngine.Time.fixedDeltaTime;
    }

    public void ExitMatchWithParty()
    {
        ExitManager.Instance.ExitMatch(ExitType.HostLeftWithParty);
        SteamLobbyManager.Instance.ExitMatchWithParty();
    }

    #region PLAYER_ASSIGNMENTS

    public void AddPlayer(PlayerController player, int id)
    {
        ServerManager.Instance.GetPlayer(id).PlayerController = player;

        CreateFightManager(); //lazy initialise fight manager

        if (!currentFight.AlivePlayers.Contains(id))
            currentFight.AlivePlayers.Add(id);

        SetPlayerSpawn(player);
        wins.Add(player, 0);
    }

    public PlayerController GetPlayer(int playerID)
    {
        return ServerManager.Instance.GetPlayer(playerID).PlayerController;
    }

    public int GetPlayerID(PlayerController player)
    {
        return ServerManager.Instance.GetPlayer(player).PlayerID;
    }

    public PlayerController GetClientPlayer()
    {
        foreach (var player in ServerManager.Instance.Players)
        {
            if (player.PlayerController.isLocalPlayer)
            {
                return player.PlayerController;
            }
        }

        return null;
    }

    #endregion
}
