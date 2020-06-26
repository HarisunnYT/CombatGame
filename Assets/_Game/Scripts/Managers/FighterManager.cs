using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterManager : PersistentSingleton<FighterManager>
{
    public MoveData AttackOne { get; private set; }
    public MoveData AttackTwo { get; private set; }
    public MoveData AttackThree { get; private set; }

    [SerializeField]
    private FighterData[] fighters;

    public delegate void MoveEvent(MoveData move);
    public event MoveEvent OnEquipedMove;

    public FighterData GetFighterForPlayer(int playerID)
    {
        string fighterName = GetFighterNameFromPlayerID(playerID);

        foreach(var fighter in fighters)
        {
            if (fighter.name == fighterName)
            {
                return fighter;
            }
        }

        return null;
    }

    public string GetFighterNameFromPlayerID(int playerID)
    {
        return ServerManager.Instance.GetPlayer(playerID).Figher;
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

        OnEquipedMove?.Invoke(moveData);
    }

    public bool IsMoveEquiped(MoveData move)
    {
        return AttackOne == move || AttackTwo == move || AttackThree == move;
    }
}
