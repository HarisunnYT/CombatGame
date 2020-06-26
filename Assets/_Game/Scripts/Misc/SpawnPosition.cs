using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPosition : MonoBehaviour
{
    [SerializeField]
    private int spawnDirection;

    public void SetPlayerSpawn(PlayerController player)
    {
        player.transform.position = transform.position;
        player.gameObject.SetActive(true);

        player.ResetCharacter();

        player.SetDirection(spawnDirection);
        player.DisableInput();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(spawnDirection, 0, 0));
    }
}
