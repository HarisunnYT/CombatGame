using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRoundInformation : Singleton<PlayerRoundInformation>, IFightEvents
{
    public int Cash { get; private set; }
    public int Wins { get; private set; }

    private const int maxCash = 9999;

    public delegate void CashEvent(int newAmount);
    public event CashEvent OnCashUpdated;

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
        OnCashUpdated?.Invoke(Cash);
    }

    public void RemoveCash(int amount)
    {
        Cash = Mathf.Clamp(Cash - amount, 0, maxCash);
    }

    public void AddWin()
    {
        Wins++;
    }

    public void AddPlacementCash(int placement)
    {
        CharacterData gameData = GameManager.Instance.GameData;
        switch (placement)
        {
            case 1:
                AddCash((int)gameData.GetValue(DataKeys.VariableKeys.FirstPlaceCash));
                break;
            case 2:
                AddCash((int)gameData.GetValue(DataKeys.VariableKeys.SecondPlaceCash));
                break;
            case 3:
                AddCash((int)gameData.GetValue(DataKeys.VariableKeys.ThirdPlaceCash));
                break;
            case 4:
                AddCash((int)gameData.GetValue(DataKeys.VariableKeys.FourthPlaceCash));
                break;
        }
    }

    public void OnPlayerDied(PlayerController killer, PlayerController victim)
    {
        if (killer.isLocalPlayer)
        {
            AddCash((int)GameManager.Instance.GameData.GetValue(DataKeys.VariableKeys.CashPerKill));
        }
    }

    #region LISTENERS

    public void AddListener()
    {
        CombatInterfaces.AddListener(this);
    }

    public void RemoveListener()
    {
        CombatInterfaces.RemoveListener(this);
    }

    #endregion
}
