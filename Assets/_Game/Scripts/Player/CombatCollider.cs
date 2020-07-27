using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatCollider : MonoBehaviour
{
    //TODO make weapon data
    public int Damage;
    public float Knockback = 5;

    [Space()]
    [SerializeField]
    private Collider2D collider;

    private PlayerController playerController;

    public Collider2D Collider { get { return collider; } }

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((SteamLobbyManager.Instance && SteamLobbyManager.Instance.PublicHost) || (ServerManager.Instance && !ServerManager.Instance.IsOnlineMatch))
        {
            IDamagable damagable = other.GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.OnDamaged(Damage, playerController);
                playerController.Knockback((transform.position - other.transform.position).normalized, playerController.CharacterStats.Weight);
            }

            IKnockable knockable = other.GetComponent<IKnockable>();
            if (knockable != null)
            {
                Vector3 direction = (other.transform.position - transform.position).normalized;
                direction.x = direction.x > 0 ? 1 : -1;
                knockable.OnKnockback(Knockback, direction + Vector3.up);
            }
        }
    }
}
