using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampoline : LevelObject
{
    [SerializeField]
    private float force;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rigidbody = collision.gameObject.GetComponent<Rigidbody2D>();
        if (rigidbody && rigidbody.velocity.y <= 0 && collision.gameObject.GetComponent<NetworkBehaviour>().isClient)
        {
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0);
            rigidbody.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        }
    }
}
