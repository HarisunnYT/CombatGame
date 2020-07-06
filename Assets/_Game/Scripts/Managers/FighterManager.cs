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

    public bool HasLocalPlayerReadiedUp { get; private set; }

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

    public FighterData GetFighter(string fighterName)
    {
        foreach (var fighter in fighters)
        {
            if (fighter.name == fighterName)
            {
                return fighter;
            }
        }

        return null;
    }

    public bool LocalPlayerSelectedCharacter(string characterName)
    {
        if (ServerManager.Instance.IsOnlineMatch)
        {
            NetworkManager.Instance.RoomPlayer.SelectCharacter(NetworkManager.Instance.RoomPlayer.index, characterName);
        }
        else
        {
            if (!LocalPlayersManager.Instance.HasLocalPlayerReadiedUp(CursorManager.Instance.GetLastInteractedPlayerIndex()))
            {
                string charName = characterName;
                if (charName == "random")
                    charName = ServerManager.Instance.GetRandomUnselectedCharacter();

                LocalPlayersManager.Instance.LocalPlayerReadiedUp(CursorManager.Instance.GetLastInteractedPlayerIndex());
                CursorManager.Instance.HideCursor(CursorManager.Instance.GetLastInteractedPlayerIndex());
                CharacterSelectManager.Instance.CharacterSelected(CursorManager.Instance.GetLastInteractedPlayerIndex(), charName);

                SetLocalPlayerReady(true);

                return true;
            }
        }

        return false;
    }

    public void LocalPlayerUnselectedCharacter(string characterName)
    {
        if (ServerManager.Instance.IsOnlineMatch)
        {
            NetworkManager.Instance.RoomPlayer.UnselectCharacter(NetworkManager.Instance.RoomPlayer.index, characterName);
        }
        else
        {
            if (LocalPlayersManager.Instance.HasLocalPlayerReadiedUp(CursorManager.Instance.GetLastInteractedPlayerIndex()))
            {
                LocalPlayersManager.Instance.LocalPlayerUnreadiedUp(CursorManager.Instance.GetLastInteractedPlayerIndex());
                CursorManager.Instance.ShowCursor(CursorManager.Instance.GetLastInteractedPlayerIndex());
                CharacterSelectManager.Instance.CharacterUnselected(CursorManager.Instance.GetLastInteractedPlayerIndex(), characterName);

                SetLocalPlayerReady(false);
            }
        }
    }

    public void SetLocalPlayerReady(bool ready)
    {
        HasLocalPlayerReadiedUp = ready;
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

    public FighterData GetRandomFighter()
    {
        return fighters[Random.Range(0, fighters.Length)];
    }
}
