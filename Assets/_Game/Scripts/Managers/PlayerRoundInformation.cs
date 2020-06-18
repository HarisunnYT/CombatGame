using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRoundInformation : Singleton<PlayerRoundInformation>, IFightEvents
{
    public int Cash { get; private set; }
    public int Wins { get; private set; }

    private const int maxCash = 9999;

    protected override void Initialize()
    {
        AddListener();
    }

    protected override void Deinitialize()
    {
        RemoveListener();
    }

    public void AddCash(int amount)
    {
        Cash += amount;
    }

    public void RemoveCash(int amount)
    {
        Cash = Mathf.Clamp(Cash - amount, 0, maxCash);
    }

    public void AddWin()
    {
        Wins++;
    }

    public void AddListener()
    {
        CombatInterfaces.AddListener(this);
    }

    public void RemoveListener()
    {
        CombatInterfaces.RemoveListener(this);
    }

    public void OnPlayerDied(PlayerController killer, PlayerController victim)
    {
        if (killer.isLocalPlayer)
        {
            AddCash((int)GameManager.Instance.GameData.GetValue(DataKeys.VariableKeys.CashPerKill));
        }
    }
}
