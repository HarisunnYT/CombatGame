﻿using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampoline : LevelObject
{
    [SerializeField]
    private float force;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    protected override void OnTriggerEntered(Collider2D collider)
    {
        Rigidbody2D rigidbody = collider.gameObject.GetComponent<Rigidbody2D>();
        if (rigidbody && rigidbody.velocity.y <= 0 && (!ServerManager.Instance.IsOnlineMatch || collider.gameObject.GetComponent<NetworkBehaviour>().isLocalPlayer))
        {
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0);
            rigidbody.AddForce(Vector2.up * force, ForceMode2D.Impulse);

            animator.SetTrigger("Bounce");
        }
    }
}
