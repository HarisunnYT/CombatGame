using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : NetworkBehaviour, IHealth, IDamagable
{
    [SerializeField]
    protected int maxHealth = 100;

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

        Health = maxHealth;
    }

    public void OnDamaged(int amount, PlayerController player)
    {
        if (!MatchManager.Instance.MatchStarted || Invincible)
            return;

        if (ServerManager.Instance.IsOnlineMatch)
            RpcOnDamaged(ServerManager.Time, amount, MatchManager.Instance.GetPlayerID(player), false);
        else
            OnDamagedClient(ServerManager.Time, amount, MatchManager.Instance.GetPlayerID(player), false);
    }

    /// <summary>
    /// should only be called if you need to guarantee kill the player
    /// </summary>
    /// <param name="player"></param>
    public void ForceKill()
    {
        if (!MatchManager.Instance.MatchStarted)
            return;

        if (ServerManager.Instance.IsOnlineMatch)
            RpcOnDamaged(ServerManager.Time, maxHealth, MatchManager.Instance.GetPlayerID(this as PlayerController), true);
        else
            OnDamagedClient(ServerManager.Time, maxHealth, MatchManager.Instance.GetPlayerID(this as PlayerController), true);
    }

    private void OnDamagedClient(float serverTime, int amount, int playerID, bool ignoreInvincibility = false)
    {
        if (damageTimes.Contains(serverTime))
            return;

        if (Alive && (ignoreInvincibility || !Invincible))
        {
            damageTimes.Add(serverTime);
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
    public virtual void RpcOnDamaged(float serverTime, int amount, int playerID, bool ignoreInvincibility)
    {
        OnDamagedClient(serverTime, amount, playerID, ignoreInvincibility);
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
        Health = Mathf.Clamp(Health + amount, 0, maxHealth);
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
        Health = maxHealth;
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

    //it's int so it works with animation events
    public void SetInvincible(int invincible)
    {
        if (!ServerManager.Instance.IsOnlineMatch)
            Invincible = invincible == 1;
        else
            CmdSetInvincible(invincible);
    }

    [Command]
    private void CmdSetInvincible(int invincible)
    {
        SetInvincible(invincible);
        RpcSetInvincible(invincible);
    }

    [ClientRpc]
    private void RpcSetInvincible(int invincible)
    {
        Invincible = invincible == 1;
    }

    private IEnumerator InvincibleCoroutine()
    {
        SetInvincible(1);

        yield return new WaitForSeconds(CharacterStats.InvincibleTime);

        SetInvincible(0);
        invincibleRoutine = null;
    }
}
