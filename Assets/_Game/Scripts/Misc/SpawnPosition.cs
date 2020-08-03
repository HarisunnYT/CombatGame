using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPosition : MonoBehaviour
{
    [SerializeField]
    private int spawnDirection;

    public bool Occupied { get; private set; }

    public void SetPlayerSpawn(PlayerController player)
    {
        if (player.Spawned)
            return;

        player.transform.position = transform.position;
        player.gameObject.SetActive(true);

        player.ResetCharacter();
        player.SetSpawned(true);

        player.SetDirection(spawnDirection);
        player.DisableInput();

        Occupied = true;
    }

    public void ResetData()
    {
        Occupied = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(spawnDirection, 0, 0));
    }
}
