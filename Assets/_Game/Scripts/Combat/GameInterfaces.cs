using Mirror;
using System;
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

    void OnDamaged(float serverTime, int amount, PlayerController player);

    [ClientRpc]
    void RpcOnDamaged(float serverTime, int amount, int playerID);

    void OnHealed (int amount);

    [ClientRpc]
    void RpcOnHealed(int amount);
}

public interface IDamages
{
    int Damage { get; set; }
}

public interface IKnockable
{
    void OnKnockback(int playerId, float knockback, Vector2 direction);

    [ClientRpc]
    void RpcOnKnockback(int playerId, float knockback, Vector2 direction);
}

public interface IBase
{
    void AddListener();
    void RemoveListener();
}

public interface IFightEvents : IBase
{
    void OnPlayerDied(PlayerController killer, PlayerController victim);
}

public interface IServerEvents : IBase
{
    void OnPlayerDisconnected(int playerId);
}

public class GameInterfaces
{
    private static List<IBase> listeners = new List<IBase>();

    public static void AddListener(IBase obj)
    {
        listeners.Add(obj as IServerEvents);
    }

    public static void RemoveListener(IBase obj)
    {
        listeners.Remove(obj);
    }

    public static void OnPlayerDied(PlayerController killer, PlayerController victim)
    {
        foreach(var listener in listeners)
        {
            if (listener is IFightEvents)
                ((IFightEvents)listener).OnPlayerDied(killer, victim);
        }
    }

    public static void OnPlayerDisconnected(int playerId)
    {
        foreach (var listener in listeners)
        {
            if (listener is IServerEvents)
                ((IServerEvents)listener).OnPlayerDisconnected(playerId);
        }
    }
}