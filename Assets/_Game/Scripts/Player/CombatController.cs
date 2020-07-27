using Mirror;
using MultiplayerBasicExample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : NetworkBehaviour
{
    private float attackOneCooldownTimer = 0;
    private float attackTwoCooldownTimer = 0;
    private float attackThreeCooldownTimer = 0;

    protected PlayerController playerController;
    protected PlayerRoundInformation playerRoundInfo;
    protected Animator animator;
    protected Rigidbody2D rigidbody;

    public bool Attacking { get { return basicAttacking || specialAttacking; } }
    private bool basicAttacking = false;
    private bool specialAttacking = false;

    private float attackButtonTimer = 0;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerRoundInfo = playerController.PlayerRoundInfo;
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();

        MatchManager.Instance.OnPhaseChanged += OnPhaseChanged;
    }

    protected virtual void Update()
    {
        if (!isLocalPlayer || playerController.InputProfile == null || !playerController.InputEnabled)
            return;

        //attacking
        if (playerController.InputProfile.Attack.WasPressed)
        {
            //reset animator value before determining what attack was pressed
            DetermineAttack(-1);

            if (playerController.InputProfile.Attack1.WasPressed && ServerManager.Time > attackOneCooldownTimer)
                DetermineAttack(0);
            else if (playerController.InputProfile.Attack2.WasPressed && ServerManager.Time > attackTwoCooldownTimer)
                DetermineAttack(1);
            else if (playerController.InputProfile.Attack3.WasPressed && ServerManager.Time > attackThreeCooldownTimer)
                DetermineAttack(2);

            basicAttacking = true;
            attackButtonTimer = Time.time + playerController.TechnicalData.GetValue(DataKeys.VariableKeys.AttackingButtonResetDelay);
        }

        if (Time.time > attackButtonTimer)
            basicAttacking = false;

        animator.SetBool("BasicAttacking", basicAttacking);
        animator.SetBool("SpecialAttacking", specialAttacking);
    }

    /// <param name="attackIndex">0, 1 or 2</param>
    private void DetermineAttack(int attackIndex)
    {
        //TODO account for cooldowns
        int moveNumber = 0;
        if (attackIndex == 0 && playerRoundInfo.AttackOne != null)
        {
            if (Attack(playerRoundInfo.AttackOne))
            {
                moveNumber = playerRoundInfo.AttackOne.MoveId;
                attackOneCooldownTimer = ServerManager.Time + playerRoundInfo.AttackOne.Cooldown;
            }
        }
        else if (attackIndex == 1 && playerRoundInfo.AttackTwo != null)
        {
            if (Attack(playerRoundInfo.AttackTwo))
            {
                moveNumber = playerRoundInfo.AttackTwo.MoveId;
                attackTwoCooldownTimer = ServerManager.Time + playerRoundInfo.AttackTwo.Cooldown;
            }
        }
        else if (attackIndex == 2 && playerRoundInfo.AttackThree != null)
        {
            if (Attack(playerRoundInfo.AttackThree))
            {
                moveNumber = playerRoundInfo.AttackThree.MoveId;
                attackThreeCooldownTimer = ServerManager.Time + playerRoundInfo.AttackThree.Cooldown;
            }
        }

        if (attackIndex != -1)
        {
            specialAttacking = true;
            animator.SetInteger("MoveNumber", moveNumber);
        }
    }

    public virtual void AttackComplete()
    {
        specialAttacking = false;
        playerController.DisableAllCombatColliders();
    }

    private void OnPhaseChanged(MatchManager.RoundPhase phase)
    {
        ResetController();
    }

    private void ResetController()
    {
        attackOneCooldownTimer = 0;
        attackTwoCooldownTimer = 0;
        attackThreeCooldownTimer = 0;
    }

    protected virtual bool Attack(MoveData moveData) 
    {
        return true;
    }
}
