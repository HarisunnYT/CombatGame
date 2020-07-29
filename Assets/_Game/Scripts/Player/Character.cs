using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : NetworkBehaviour, IHealth, IDamagable
{
    [SerializeField]
    protected int startingHealth = 100;

    [SerializeField]
    protected LayerMask invertedCharacterMask;

    [SerializeField]
    private float groundedRaySize = 1.75f;

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

    private List<float> damageTimes = new List<float>(); //a list of times of when damage was taken, 
                                                         //used to stop taken damage multiple times for the same attack

    public delegate void HealthEvent(int health);
    public event HealthEvent OnHealthChanged;

    protected virtual void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        CharacterStats = GetComponent<CharacterStats>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageFlash = GetComponent<ColorFlash>();

        Health = startingHealth;
    }

    public void OnDamaged(float serverTime, int amount, PlayerController player)
    {
        if (!MatchManager.Instance.MatchStarted)
            return;

        if (ServerManager.Instance.IsOnlineMatch)
            RpcOnDamaged(serverTime, amount, MatchManager.Instance.GetPlayerID(player));
        else
            OnDamagedClient(serverTime, amount, MatchManager.Instance.GetPlayerID(player));
    }

    private void OnDamagedClient(float serverTime, int amount, int playerID)
    {
        if (Alive && !Invincible)
        {
            Debug.Log(serverTime);

            Health -= amount;

            //damage text
            GameObject obj = ObjectPooler.GetPooledObject(SpawnObjectsManager.Instance.GetPrefab(DataKeys.SpawnableKeys.WorldSpaceText));
            obj.GetComponent<WorldSpaceText>().Display("-" + amount, Color.red, transform.position + transform.up, 3, 1);

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

            OnHealthChanged?.Invoke(Health);
        }
    }

    [ClientRpc]
    public virtual void RpcOnDamaged(float serverTime, int amount, int playerID)
    {
        OnDamagedClient(serverTime, amount, playerID);
    }

    public void OnHealed(int amount)
    {
        if (ServerManager.Instance.IsOnlineMatch)
            RpcOnHealed(amount);
        else
            OnHealedClient(amount);
    }

    private void OnHealedClient(int amount)
    {
        Health = Mathf.Clamp(Health + amount, 0, startingHealth);
        Alive = Health > 0;

        OnHealthChanged?.Invoke(Health);
    }

    [ClientRpc]
    public void RpcOnHealed(int amount)
    {
        OnHealedClient(amount);
    }

    protected virtual void Update()
    {
        Vector3 pivot = transform.position + Vector3.up;
        Grounded = Physics2D.Raycast(pivot, Vector2.down, groundedRaySize, invertedCharacterMask);
        Debug.DrawLine(pivot, new Vector3(pivot.x, pivot.y - groundedRaySize, 0), Color.green);
    }

    protected virtual void OnDeath(int playerID)
    {
        Alive = false;
    }

    public virtual void ResetCharacter()
    {
        Health = startingHealth;
        OnHealthChanged?.Invoke(Health);
        SetAlive();

        gameObject.SetActive(true);
    }

    public void SetAlive()
    {
        Alive = true;
    }

    public void SetDirection(int direction)
    {
        if (!ServerManager.Instance.IsOnlineMatch)
            SetDirectionClient(direction);
        else
            CmdSetDirection(direction);
    }

    private void SetDirectionClient(int direction)
    {
        spriteRenderer.flipX = direction == 1 ? false : true;
        scaleFlipper.transform.localScale = new Vector3(direction, 1, 1);

        Direction = direction;
    }

    [Command]
    private void CmdSetDirection(int direction)
    {
        SetDirectionClient(direction); //set direction on server
        RpcSetDirection(direction); //set direction on client
    }

    [ClientRpc]
    private void RpcSetDirection(int direction)
    {
        SetDirectionClient(direction);
    }

    private IEnumerator InvincibleCoroutine()
    {
        Invincible = true;

        yield return new WaitForSeconds(CharacterStats.InvincibleTime);

        Invincible = false;
        invincibleRoutine = null;
    }
}
