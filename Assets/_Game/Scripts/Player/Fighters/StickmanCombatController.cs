using Mirror;
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

    [Header("Axe Throw")]
    [SerializeField]
    private Projectile axePrefab;

    [SerializeField]
    private float axeForce = 10;

    protected override bool Attack(MoveData moveData)
    {
        if (moveData.name == "body_slam")
            return StartBodySlam();
        else if (moveData.name == "axe_throw")
            ThrowAxe();

        return true;
    }

    protected override void Update()
    {
        base.Update();

        if (!isLocalPlayer && ServerManager.Instance && ServerManager.Instance.IsOnlineMatch)
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

    #region AXE_THROW

    private void ThrowAxe()
    {
        if (ServerManager.Instance.IsOnlineMatch)
            CmdThrowAxe(playerController.PlayerID, playerController.Direction, (int)(axeForce * 100), (int)ForceMode2D.Impulse, NetworkClient.connection as NetworkConnectionToClient);
        else
        {
            Projectile axe = ObjectPooler.GetPooledObject(axePrefab.gameObject).GetComponent<Projectile>();
            axe.AddForce(playerController, new Vector3(playerController.Direction, 0, 0), axeForce, ForceMode2D.Impulse);
        }
    }

    [Command]
    private void CmdThrowAxe(int playerId, int direction, int force, int forceMode2D, NetworkConnectionToClient conn)
    {
        PlayerController playerController = ServerManager.Instance.GetPlayer(playerId).PlayerController;
        Projectile axe = Instantiate(axePrefab);
        axe.AddForce(playerController, new Vector3(direction, 0, 0), (float)force / 100, (ForceMode2D)forceMode2D);
        NetworkServer.Spawn(axe.gameObject, conn);
    }

    #endregion
}
