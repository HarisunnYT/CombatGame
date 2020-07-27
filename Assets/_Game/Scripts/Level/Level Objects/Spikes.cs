using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : LevelObject
{
    [SerializeField]
    private int damage = 20;

    protected override void OnTriggerEntered(Collider2D collider)
    {
        if ((SteamLobbyManager.Instance && SteamLobbyManager.Instance.PublicHost) || (ServerManager.Instance && !ServerManager.Instance.IsOnlineMatch))
        {
            IDamagable damagable = collider.GetComponent<IDamagable>();
            if (damagable != null)
                damagable.OnDamaged(damage, null);
        }
    }
}
