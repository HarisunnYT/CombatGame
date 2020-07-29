using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : Projectile
{
    [SerializeField]
    private float torque;

    public override void AddForce(Vector3 spawnPosition, Vector3 direction, float force, ForceMode2D forceMode)
    {
        base.AddForce(spawnPosition, direction, force, forceMode);

        rigidbody.AddTorque(torque, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Despawn();
    }
}
