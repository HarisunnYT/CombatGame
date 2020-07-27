using MultiplayerBasicExample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickmanCombatController : CombatController
{
    private enum AttackType
    {
        None,
        BodySlam
    }

    private AttackType currentAttackType;

    [Header("Body Slam")]
    [SerializeField]
    private float bodySlamForce = 2;

    [SerializeField]
    private float bodySlamDelay = 0.5f;

    [SerializeField]
    private float getUpDelay = 0.2f;

    protected override bool Attack(MoveData moveData)
    {
        if (moveData.name == "body_slam")
            return StartBodySlam();

        return false;
    }

    protected override void Update()
    {
        base.Update();

        if (!isLocalPlayer)
            return;

        if (playerController.Grounded && currentAttackType == AttackType.BodySlam)
            EndBodySlam();
    }

    private IEnumerator DelayedCallback(float delay, System.Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback.Invoke();
    }

    public override void AttackComplete()
    {
        base.AttackComplete();
        currentAttackType = AttackType.None;
    }

    #region BODY_SLAM

    private bool StartBodySlam()
    {
        if (playerController.Grounded)
            return false;

        currentAttackType = AttackType.BodySlam;
        rigidbody.velocity = Vector2.zero;
        rigidbody.isKinematic = true;
        playerController.DisableInput();
        playerController.DisablePlayerToPlayerCollisions(true);

        StartCoroutine(DelayedSlam());

        return true;
    }

    private IEnumerator DelayedSlam()
    {
        yield return new WaitForSeconds(bodySlamDelay);

        rigidbody.isKinematic = false; 
        rigidbody.AddForce(Vector2.down * bodySlamForce, ForceMode2D.Impulse);
    }

    private void EndBodySlam()
    {
        StartCoroutine(DelayedCallback(getUpDelay, () =>
        {
            AttackComplete();
            playerController.EnableInput();
            playerController.DisablePlayerToPlayerCollisions(false);
        }));
    }

    #endregion
}
