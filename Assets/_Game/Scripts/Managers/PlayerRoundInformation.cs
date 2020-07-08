using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRoundInformation : MonoBehaviour, IFightEvents
{
    public int Cash { get; private set; }
    public int Wins { get; private set; }

    private const int maxCash = 9999;

    public delegate void CashEvent(int newAmount);
    public event CashEvent OnCashUpdated;

    private void Awake()
    {
        AddListener();
    }

    private void OnDestroy()
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
        OnCashUpdated?.Invoke(Cash);
    }

    public bool Purchase(int price)
    {
        if (Cash >= price)
        {
            RemoveCash(price);
            return true;
        }

        return false;
    }

    public bool CanPurchase(int price)
    {
        if (Cash >= price)
        {
            return true;
        }

        return false;
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
        if (killer.PlayerRoundInfo == this)
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
