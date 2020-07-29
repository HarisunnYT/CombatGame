using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField]
    private float secondsTillDestroy = 5;

    protected Rigidbody2D rigidbody;
    protected Collider2D collider;

    private ForceMode2D forceMode;
    private float force = -1;
    private Vector3 direction;

    private float targetDestroyTime = -1;

    protected bool isHost { get { return (SteamLobbyManager.Instance && SteamLobbyManager.Instance.PublicHost) || (ServerManager.Instance && !ServerManager.Instance.IsOnlineMatch);  } }

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        targetDestroyTime = ServerManager.Time + secondsTillDestroy;
    }

    public virtual void AddForce(Vector3 spawnPosition, Vector3 direction, float force, ForceMode2D forceMode)
    {
        this.forceMode = forceMode;
        this.force = force;
        this.direction = direction;

        transform.position = spawnPosition;

        if (forceMode == ForceMode2D.Impulse)
            AddForce();
    }

    public void AddForce(PlayerController player, Vector3 direction, float force, ForceMode2D forceMode)
    {
        Physics2D.IgnoreCollision(player.Collider, collider);
        AddForce(player.Collider.bounds.center, direction, force, forceMode);
    }

    private void AddForce()
    {
        if (isHost)
            rigidbody.AddForce(direction.normalized * force, forceMode);
    }

    protected virtual void FixedUpdate()
    {
        if (forceMode == ForceMode2D.Force)
            AddForce();

        if (ServerManager.Time > targetDestroyTime)
            Despawn();
    }

    protected void Despawn()
    {
        if (isHost)
        {
            gameObject.SetActive(false);
            RpcHide();
        }
    }

    [ClientRpc]
    private void RpcHide()
    {
        gameObject.SetActive(false);
    }
}
