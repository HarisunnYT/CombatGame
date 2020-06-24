using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterManager : PersistentSingleton<FighterManager>
{
    [SerializeField]
    private FighterData[] fighters;

    //uint is the id of the player, string is the fighter name
    private Dictionary<uint, string> playerFighters = new Dictionary<uint, string>();

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
}
