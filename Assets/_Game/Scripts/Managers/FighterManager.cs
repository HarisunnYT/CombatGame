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

    //uint is the id of the player, string is the fighter name
    private Dictionary<uint, string> playerFighters = new Dictionary<uint, string>();

    public delegate void MoveEvent(MoveData move);
    public event MoveEvent OnEquipedMove;

    public FighterData GetFighterForPlayer(uint playerID)
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

    public string GetFighterNameFromPlayerID(uint playerID)
    {
        foreach (var fighter in playerFighters)
        {
            if (fighter.Key == playerID)
            {
                return fighter.Value;
            }
        }

        return "";
    }

    public void FighterSelected(uint connectionID, string characterName)
    {
        playerFighters.Add(connectionID, characterName);
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
