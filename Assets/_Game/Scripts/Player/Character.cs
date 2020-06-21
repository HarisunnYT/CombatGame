﻿using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : NetworkBehaviour, IHealth, IDamagable, IKnockable
{
    [SerializeField]
    private int startingHealth = 100;

    [SerializeField]
    protected LayerMask invertedCharacterMask;

    [Space()]
    [SerializeField]
    private GameObject scaleFlipper;

    public int Health { get; set; }
    public bool Alive { get; set; } = true;
    public bool Invincible { get; set; }
    public int Direction { get; protected set; } = 1;
    public bool Grounded { get; protected set; } = false;

    public Rigidbody2D Rigidbody { get; private set; }
    public CharacterStats CharacterStats { get; private set; }

    protected SpriteRenderer spriteRenderer { get; private set; }

    private ColorFlash damageFlash;
    private ColorFlash currentFlash;

    private Coroutine invincibleRoutine;

    protected virtual void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        CharacterStats = GetComponent<CharacterStats>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageFlash = GetComponent<ColorFlash>();

        Health = startingHealth;
    }
    
    [ClientRpc]
    public virtual void RpcOnDamaged(int amount, uint playerID)
    {
        if (Alive && !Invincible)
        {
            Health -= amount;

            //damage text
            GameObject obj = ObjectPooler.GetPooledObject(SpawnObjectsManager.Instance.GetPrefab(DataKeys.SpawnableKeys.WorldSpaceText));
            obj.GetComponent<WorldSpaceText>().DisplayText("-" + amount, Color.red, transform.position + transform.up, 3, 1);

            //character flash
            currentFlash?.CancelFlash();
            currentFlash = damageFlash?.Flash(CharacterStats.InvincibleTime);

            //character invincible time
            if (CharacterStats.InvincibleTime > 0)
            {
                if (invincibleRoutine != null)
                    StopCoroutine(invincibleRoutine);

                invincibleRoutine = StartCoroutine(InvincibleCoroutine());
            }

            if (Health <= 0)
            {
                OnDeath(playerID);
            }
        }
    }

    protected virtual void Update()
    {
        Grounded = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + 0.5f), Vector2.down, 0.5f, invertedCharacterMask);
    }

    protected virtual void OnDeath(uint playerID)
    {
        Alive = false;
    }

    public virtual void OnKnockback(float knockback, Vector2 direction)
    {
        if (Alive)
        {
            Rigidbody.AddForce(direction * knockback, ForceMode2D.Impulse);
        }
    }

    protected void SetDirection(int direction)
    {
        if (isServer)
            RpcSetDirection(direction);
        else
            CmdSetDirection(direction);
    }

    [Command]
    protected void CmdSetDirection(int direction)
    {
        RpcSetDirection(direction);
    }

    [ClientRpc]
    protected void RpcSetDirection(int direction)
    {
        spriteRenderer.flipX = direction == 1 ? false : true;
        scaleFlipper.transform.localScale = new Vector3(direction, 1, 1);
    }

    private IEnumerator InvincibleCoroutine()
    {
        Invincible = true;

        yield return new WaitForSeconds(CharacterStats.InvincibleTime);

        Invincible = false;
        invincibleRoutine = null;
    }
}
