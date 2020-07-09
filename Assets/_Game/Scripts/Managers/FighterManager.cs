using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterManager : PersistentSingleton<FighterManager>
{
    [SerializeField]
    private FighterData[] fighters;

    public string LastPlayedFighterName { get; private set; }
    public bool HasLocalPlayerReadiedUp { get; private set; }

    public const string LastPlayerFighterKey = "last_player_fighter";

    protected override void Initialize()
    {
        LastPlayedFighterName = PlayerPrefs.GetString(LastPlayerFighterKey, fighters[0].FighterName);
    }

    public FighterData GetFighterForPlayer(int playerID)
    {
        string fighterName = GetFighterNameFromPlayerID(playerID);

        foreach(var fighter in fighters)
        {
            if (fighter.FighterName == fighterName)
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
        if (fighterName == "")
            return fighters[0];

        foreach (var fighter in fighters)
        {
            if (fighter.FighterName == fighterName)
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

    public FighterData GetRandomFighter()
    {
        return fighters[Random.Range(0, fighters.Length)];
    }

    public void SetLastPlayedFighter(string fighterName)
    {
        LastPlayedFighterName = fighterName;
        PlayerPrefs.SetString(LastPlayerFighterKey, fighterName);
    }
}
