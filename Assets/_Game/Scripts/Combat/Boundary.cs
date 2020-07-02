using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (SteamLobbyManager.Instance.PublicHost || !ServerManager.Instance.IsOnlineMatch)
        {
            IDamagable damagable = collision.GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.OnDamaged(int.MaxValue, collision.GetComponent<PlayerController>());
            }
        }
    }
}
