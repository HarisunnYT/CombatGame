using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealth
{
    int Health { get; set; }
    bool Invincible { get; set; }
    bool Alive { get; set; }
}

public interface IDamagable
{
    int Health { get; set; }

    void OnDamaged(int amount, Character damagedFrom);
}

public interface IDamages
{
    int Damage { get; set; }
}

public interface IKnockable
{
    void OnKnockback(float knockback, Vector2 direction);
}

public interface IFightEvents
{
    void AddListener();
    void RemoveListener();
    void OnPlayerDied(PlayerController killer, PlayerController victim);
}

public class CombatInterfaces
{
    private static List<IFightEvents> fightEvents = new List<IFightEvents>();

    public static void AddListener(IFightEvents listener)
    {
        fightEvents.Add(listener);
    }

    public static void RemoveListener(IFightEvents listener)
    {
        fightEvents.Remove(listener);
    }

    public static void OnPlayerDied(PlayerController killer, PlayerController victim)
    {
        foreach(var listener in fightEvents)
        {
            listener.OnPlayerDied(killer, victim);
        }
    }
}