using Mirror;
using MultiplayerBasicExample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Character
{
    #region PLAYER_EXTENSIONS

    public enum MovementType
    {
        Normal
    }

    [System.Serializable]
    struct MovementTypeData
    {
        public MovementType MovementType;
        public CharacterData MovementData;
    }

    #endregion

    #region EXPOSED_VARIABLES

    [Space()]
    [SerializeField]
    private MovementTypeData[] movementTypeDataItems;

    [SerializeField]
    private CharacterData technicalData;
    public CharacterData TechnicalData { get { return technicalData; } }

    #endregion

    #region COMPONENTS

    public CharacterData CurrentMovementData { get; private set; }
    public MovementType CurrentMovementType { get; private set; } = MovementType.Normal;

    private BaseMovement baseMovement;
    private Animator animator;

    private InputProfile inputProfile;

    #endregion

    #region RUNTIME_VARIABLES

    public bool InputEnabled { get; private set; }

    public Vector2 InputAxis { get; private set; }

    public bool HoldingJump { get; private set; } = false;
    public bool HorizontalMovementEnabled { get; private set; } = true;
    public bool VerticalMovementEnabled { get; private set; } = true;

    public FighterData Fighter { get; private set; }

    private Vector3 originalScale;

    private float previousScaleSwappedTimer = 0;
    private float attackButtonTimer = 0;
    private float timeBetweenJumpTimer = 0;

    private bool attacking = false;

    private Coroutine horizontalMovementCoroutine;
    private Coroutine verticalMovementCoroutine;

    private int playerID = -1;

    #endregion

    #region CALLBACKS

    public delegate void NormalEvent();
    public event NormalEvent OnPlayerDisconnected;

    #endregion

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
        originalScale = transform.localScale;

        SetMovementType(MovementType.Normal, false);
    }

    private void Start()
    {
        //we don't want physics on network players as their positions are set over the server
        Rigidbody.isKinematic = !isLocalPlayer && ServerManager.Instance.IsOnlineMatch;

        //id assigning
        if (playerID == -1)
        {
            if (ServerManager.Instance.IsServer || ServerManager.Instance.IsOnlineMatch)
                NetworkManager.Instance.OnPlayerCreated(connectionToServer, this, false);
            else
                AssignID(playerID);
        }

        Fighter = FighterManager.Instance.GetFighterForPlayer(playerID);
        LobbyManager.Instance.PlayerCreated(playerID, this);
        MatchManager.Instance.AddPlayer(this, playerID);

        inputProfile = new InputProfile(ServerManager.Instance.GetPlayer(playerID).ControllerGUID, ServerManager.Instance.IsOnlineMatch);
    }

    private void OnDestroy()
    {
        OnPlayerDisconnected?.Invoke();

        if (GameManager.Instance)
            ServerManager.Instance.RemovePlayer(playerID);
    }

    protected override void Update()
    {
        if (!isLocalPlayer && ServerManager.Instance.IsOnlineMatch)
            return;

        if (!InputEnabled)
            return;

        base.Update();

        InputAxis = inputProfile.Move;
        int roundedXAxis = InputAxis.x > 0 ? 1 : -1;

        animator.SetBool("Running", InputAxis.x != 0);

        //flip scale
        if (InputAxis.x != 0 && Time.time > previousScaleSwappedTimer)
        {
            SetDirection(roundedXAxis);
            previousScaleSwappedTimer = Time.time + technicalData.GetValue(DataKeys.VariableKeys.FlipScaleDamper);
        }

        if (inputProfile.Jump.WasPressed && Time.time > timeBetweenJumpTimer)
        {
            baseMovement.Jump();
            timeBetweenJumpTimer = Time.time + CurrentMovementData.GetValue(DataKeys.VariableKeys.TimeBetweenJump);
        }

        HoldingJump = inputProfile.Jump;

        //attacking
        if (inputProfile.Attack1.WasPressed)
        {
            baseMovement.Attack();
            attacking = true;
            attackButtonTimer = Time.time + TechnicalData.GetValue(DataKeys.VariableKeys.AttackingButtonResetDelay);
        }

        if (Time.time > attackButtonTimer)
        {
            attacking = false;
        }

        animator.SetBool("Attacking", attacking);
        animator.SetBool("Grounded", Grounded);
        animator.SetBool("HoldingJump", HoldingJump);
        animator.SetBool("Falling", !Grounded && Rigidbody.velocity.y < 0);
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer && ServerManager.Instance.IsOnlineMatch)
            return;

        if (!InputEnabled)
            return;

        baseMovement.Update(Time.time);

        if (HorizontalMovementEnabled)
            baseMovement.MoveHorizontal(Time.deltaTime);

        if (VerticalMovementEnabled)
            baseMovement.MoveVertical(Time.deltaTime);
    }

    public void AssignID(int id)
    {
        playerID = id;

        if (!ServerManager.Instance.IsOnlineMatch && !ServerManager.Instance.IsServer)
            playerID = LocalPlayersManager.Instance.PlayerIndexForAssigning++;
    }

    public void SetFighter(FighterData fighter)
    {
        Fighter = fighter;
    }

    #region MOVEMENT

    public void SetMovementType(MovementType movementType, bool displayPuffParticle = true)
    {
        for (int i = 0; i < System.Enum.GetNames(typeof(MovementType)).Length; i++)
        {
            animator.SetLayerWeight(animator.GetLayerIndex(((MovementType)i).ToString()), 0);
        }

        animator.SetLayerWeight(animator.GetLayerIndex((movementType).ToString()), 1);
        CurrentMovementType = movementType;

        foreach (var movementTypeDataItem in movementTypeDataItems)
        {
            if (movementTypeDataItem.MovementType == movementType)
            {
                CurrentMovementData = movementTypeDataItem.MovementData;
            }
        }

        Rigidbody.gravityScale = CurrentMovementData.GetValue(DataKeys.VariableKeys.GravityScale);

        if (baseMovement != null)
            baseMovement.Deconfigure();

        baseMovement = new BaseMovement().Configure(this, Rigidbody, animator);
    }

    private void SetMovementType(int movementType)
    {
        if (isServer)
            RpcSetMovementType(movementType);
        else
            CmdSetMovementType(movementType);
    }

    [Command]
    public void CmdSetMovementType(int movementType)
    {
        RpcSetMovementType(movementType);
    }

    [ClientRpc]
    public void RpcSetMovementType(int movementType)
    {
        SetMovementType((MovementType)movementType);
    }

    public float GetMaxHorizontalSpeed()
    {
        return attacking ? CurrentMovementData.GetValue(DataKeys.VariableKeys.MaxHorizontalSpeed) / CurrentMovementData.GetValue(DataKeys.VariableKeys.AttackSpeedDamper, 1) :
                           CurrentMovementData.GetValue(DataKeys.VariableKeys.MaxHorizontalSpeed);
    }

    public float GetMaxVerticalSpeed()
    {
        return CurrentMovementData.GetValue(DataKeys.VariableKeys.MaxVerticalSpeed);
    }

    public void Knockback(Vector3 direction, float force)
    {
        DisableHorizontalMovement(0.5f);

        Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y);
        Rigidbody.AddForce(direction * force, ForceMode2D.Impulse);
    }

    public void DisableInput()
    {
        InputEnabled = false;
    }

    public void EnableInput()
    {
        InputEnabled = true;
    }

#endregion

    #region COMBAT

    protected override void OnDeath(int killedFromPlayerID)
    {
        base.OnDeath(killedFromPlayerID);

        if (ServerManager.Instance.IsOnlineMatch)
        {
            if (isServer)
                RpcOnDeath(killedFromPlayerID, MatchManager.Instance.GetPlayerID(this));
            else
                CmdOnDeath(killedFromPlayerID, MatchManager.Instance.GetPlayerID(this));
        }
        else
        {
            OnDeathClient(killedFromPlayerID, playerID);
        }
    }

    private void OnDeathClient(int killerID, int victimID)
    {
        CombatInterfaces.OnPlayerDied(MatchManager.Instance.GetPlayer(killerID), MatchManager.Instance.GetPlayer(victimID));

        //TODO death anim and shiz
        gameObject.SetActive(false);
    }

    [Command]
    private void CmdOnDeath(int killerID, int victimID)
    {
        RpcOnDeath(killerID, victimID);
    }

    [ClientRpc]
    private void RpcOnDeath(int killerID, int victimID)
    {
        OnDeathClient(killerID, victimID);
    }

#endregion

    #region DISABLE_INPUT

    public void DisableHorizontalMovement(float duration)
    {
        if (horizontalMovementCoroutine != null)
            StopCoroutine(horizontalMovementCoroutine);

        HorizontalMovementEnabled = false;
        horizontalMovementCoroutine = StartCoroutine(DisableHorizontalMovementCoroutine(duration));
    }

    private IEnumerator DisableHorizontalMovementCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        HorizontalMovementEnabled = true;
        horizontalMovementCoroutine = null;
    }

    public void DisableVerticalMovement(float duration)
    {
        if (verticalMovementCoroutine != null)
            StopCoroutine(verticalMovementCoroutine);

        VerticalMovementEnabled = false;
        verticalMovementCoroutine = StartCoroutine(DisableVerticalMovementCoroutine(duration));
    }

    private IEnumerator DisableVerticalMovementCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        VerticalMovementEnabled = true;
        verticalMovementCoroutine = null;
    }

#endregion

}
