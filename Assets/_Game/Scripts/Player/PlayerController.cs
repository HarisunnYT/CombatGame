﻿using Mirror;
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
    public InputProfile InputProfile { get; private set; }
    public PlayerRoundInformation PlayerRoundInfo { get; private set; }

    private BaseMovement baseMovement;
    private Animator animator;
    private CommunicationController communicationController;
    private CombatCollider combatCollider;

    private Collider2D collider;

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
    public event NormalEvent OnInputProfileSet;

    #endregion

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
        communicationController = GetComponent<CommunicationController>();
        collider = GetComponent<Collider2D>();
        PlayerRoundInfo = GetComponent<PlayerRoundInformation>();
        combatCollider = GetComponentInChildren<CombatCollider>(true);

        originalScale = transform.localScale;
        combatCollider.Collider.enabled = false;

        SetMovementType(MovementType.Normal, false);
    }

    private void Start()
    {
        //we don't want physics on network players as their positions are set over the server
        Rigidbody.isKinematic = !isLocalPlayer && ServerManager.Instance.IsOnlineMatch;

        //server match id assigning
        if (ServerManager.Instance.IsOnlineMatch)
        {
            if (ServerManager.Instance.GetPlayer(netId) != null)
            {
                OnAssignedID(ServerManager.Instance.GetPlayer(netId).PlayerID);
            }
        }
        else //local match id assigning
        {
            OnAssignedID(LocalPlayersManager.Instance.PlayerIndexForAssigning);
            LocalPlayersManager.Instance.PlayerIndexForAssigning++;
        }
    }

    private void OnDestroy()
    {
        OnPlayerDisconnected?.Invoke();
        InputProfile.Deinitialise();

        if (ServerManager.Instance)
            ServerManager.Instance.RemovePlayer(playerID);
    }

    protected override void Update()
    {
        if (!isLocalPlayer && ServerManager.Instance && ServerManager.Instance.IsOnlineMatch)
            return;

        if (!InputEnabled)
            return;

        base.Update();

        InputAxis = InputProfile.Move;
        int roundedXAxis = InputAxis.x > 0 ? 1 : -1;

        animator.SetBool("Running", InputAxis.x != 0);

        //flip scale
        if (InputAxis.x != 0 && Time.time > previousScaleSwappedTimer)
        {
            SetDirection(roundedXAxis);
            previousScaleSwappedTimer = Time.time + technicalData.GetValue(DataKeys.VariableKeys.FlipScaleDamper);
        }

        if (InputProfile.Jump.WasPressed && Time.time > timeBetweenJumpTimer)
        {
            baseMovement.Jump();
            timeBetweenJumpTimer = Time.time + CurrentMovementData.GetValue(DataKeys.VariableKeys.TimeBetweenJump);
        }

        HoldingJump = InputProfile.Jump;

        //attacking
        if (InputProfile.Attack1.WasPressed)
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
        if (!isLocalPlayer && ServerManager.Instance && ServerManager.Instance.IsOnlineMatch)
            return;

        if (!InputEnabled)
            return;

        baseMovement.Update(Time.time);

        if (HorizontalMovementEnabled)
            baseMovement.MoveHorizontal(Time.deltaTime);

        if (VerticalMovementEnabled)
            baseMovement.MoveVertical(Time.deltaTime);
    }

    public void SetFighter(FighterData fighter)
    {
        Fighter = fighter;
    }

    public void OnAssignedID(int id)
    {
        //player id already assigned
        if (playerID != -1)
            return;

        playerID = id;

        Fighter = FighterManager.Instance.GetFighterForPlayer(playerID);
        MatchManager.Instance.AddPlayer(this, playerID);

        InputProfile = new InputProfile(ServerManager.Instance.GetPlayer(playerID).ControllerGUID, ServerManager.Instance.IsOnlineMatch || playerID == 0);
        OnInputProfileSet?.Invoke();

        //remove already used controllers
        if (playerID == 0 && !ServerManager.Instance.IsOnlineMatch)
        {
            foreach(var player in ServerManager.Instance.Players)
            {
                if (player.PlayerID != 0)
                    InputProfile.RemoveController(player.ControllerGUID);
            }
        }

        //set the last player fighter, only if it's an online match or the players index is 0
        if ((ServerManager.Instance.IsOnlineMatch && isLocalPlayer) || (!ServerManager.Instance.IsOnlineMatch && playerID == 0))
            FighterManager.Instance.SetLastPlayedFighter(Fighter.FighterName);
    }

    public override void ResetCharacter()
    {
        base.ResetCharacter();

        animator.SetBool("Dead", false);
        gameObject.layer = LayerMask.NameToLayer("Player");
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

        animator.SetBool("Dead", true);

        gameObject.layer = LayerMask.NameToLayer("NonPlayerInteractable");
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
