using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRoundInformation : MonoBehaviour, IFightEvents
{
    public MoveData AttackOne { get; private set; }
    public MoveData AttackTwo { get; private set; }
    public MoveData AttackThree { get; private set; }

    public int Cash { get; private set; }
    public int Wins { get; private set; }

    private const int maxCash = 9999;

    public delegate void CashEvent(int newAmount);
    public event CashEvent OnCashUpdated;

    /// <param name="position">button position eg button 1 is X / Mouse 1</param>
    public delegate void MoveEvent(MoveData move, int position);
    public event MoveEvent OnEquipedMove;

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
        if (killer != victim && killer.PlayerRoundInfo == this)
        {
            AddCash((int)GameManager.Instance.GameData.GetValue(DataKeys.VariableKeys.CashPerKill));
        }
    }

    /// <param name="position">1, 2 or 3</param>
    public void EquipedMove(MoveData moveData, int position)
    {
        if (position == 1)
            AttackOne = moveData;
        else if (position == 2)
            AttackTwo = moveData;
        else if (position == 3)
            AttackThree = moveData;

        OnEquipedMove?.Invoke(moveData, position);
    }

    public bool IsMoveEquiped(MoveData move)
    {
        return AttackOne == move || AttackTwo == move || AttackThree == move;
    }

    #region LISTENERS

    public void AddListener()
    {
        GameInterfaces.AddListener(this);
    }

    public void RemoveListener()
    {
        GameInterfaces.RemoveListener(this);
    }

    #endregion
}
