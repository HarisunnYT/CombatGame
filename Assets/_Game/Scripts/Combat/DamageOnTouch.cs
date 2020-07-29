using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOnTouch : MonoBehaviour
{
    [SerializeField]
    private int damage = 20;

    [SerializeField]
    private float knockback = 2;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Collided(collider);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collided(collision.collider);
    }

    private void Collided(Collider2D collider)
    {
        if ((SteamLobbyManager.Instance && SteamLobbyManager.Instance.PublicHost) || (ServerManager.Instance && !ServerManager.Instance.IsOnlineMatch))
        {
            IDamagable damagable = collider.GetComponent<IDamagable>();
            if (damagable != null)
                damagable.OnDamaged(damage, null);

            IKnockable knockable = collider.GetComponent<IKnockable>();
            if (knockable != null)
            {
                PlayerController target = collider.GetComponent<PlayerController>();

                Vector3 direction = (collider.transform.position - transform.position).normalized;
                direction.x = direction.x > 0 ? 1 : -1;
                knockable.OnKnockback(ServerManager.Instance.GetPlayer(target).PlayerID, knockback, direction + Vector3.up);
            }
        }
    }
}
