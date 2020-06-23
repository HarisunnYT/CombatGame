using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            player.OnDamaged(int.MaxValue, player); //we pass the player through as the killer as they killed themselves (may change this to last attack enemy)
        }
    }
}
