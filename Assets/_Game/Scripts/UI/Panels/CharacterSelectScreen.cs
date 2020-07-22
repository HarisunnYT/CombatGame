using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectScreen : Panel
{
    [SerializeField]
    private Timer countdownTimer;

    [SerializeField]
    private CharacterCell[] characterCells;

    [SerializeField]
    private SelectedCharacterCell[] selectedCharacterCells;

    public float SelectCharacterTimer { get; private set; } //we send this time to the other clients when they connect

    private bool finished = false;

    private void Awake()
    {
        if (CharacterSelectManager.Instance)
        {
            CharacterSelectManager.Instance.OnCharacterSelected += OnCharacterSelected;
            CharacterSelectManager.Instance.OnCharacterUnselected += OnCharacterUnselected;
        }
    }

    private void OnDestroy()
    {
        if (CharacterSelectManager.Instance)
        {
            CharacterSelectManager.Instance.OnCharacterSelected -= OnCharacterSelected;
            CharacterSelectManager.Instance.OnCharacterUnselected -= OnCharacterUnselected;
        }
    }

    protected override void OnShow()
    {
        SelectCharacterTimer = (float)NetworkTime.time + CharacterSelectManager.Instance.CharacterSelectTime;
        finished = false;

        //no countdown for local player
        if (ServerManager.Instance.IsOnlineMatch && !SteamLobbyManager.Instance.IsPrivateMatch)
            countdownTimer.Configure(SelectCharacterTimer);
        else
            countdownTimer.transform.parent.gameObject.SetActive(false);

        foreach(var selectedCharacter in selectedCharacterCells)
        {
            selectedCharacter.gameObject.SetActive(false);
        }

        if (ServerManager.Instance.IsOnlineMatch)
            SteamLobbyManager.Instance.StartVoiceComms();
    }

    public void ConfigureTimer(float targetTime)
    {
        countdownTimer.OverrideTargetTime(targetTime);
    }

    private void Update()
    {
        //we don't countdown in local
        if (ServerManager.Instance && ServerManager.Instance.IsOnlineMatch && !SteamLobbyManager.Instance.IsPrivateMatch)
        {
            int roundedTime = Mathf.Clamp(Mathf.RoundToInt(SelectCharacterTimer - (float)NetworkTime.time), 0, int.MaxValue);
            if (roundedTime <= 0 && !finished)
            {
                //times up, force random character for local player
                if (!FighterManager.Instance.HasLocalPlayerReadiedUp)
                    FighterManager.Instance.LocalPlayerSelectedCharacter("random");

                finished = true;
            }
        }
    }

    private void OnCharacterSelected(int playerID, string characterName)
    {
        foreach(var selectedCharacter in selectedCharacterCells)
        {
            if (!selectedCharacter.Occuipied)
            {
                selectedCharacter.Configure(ServerManager.Instance.GetPlayer(playerID), FighterManager.Instance.GetFighter(characterName));
                break;
            }
        }

        foreach (var characterCell in characterCells)
        {
            if (characterCell.CharacterName == characterName)
            {
                characterCell.SetCharacterSelected(true, playerID);
                break;
            }
        }
    }

    private void OnCharacterUnselected(int playerID, string characterName)
    {
        foreach (var selectedCharacter in selectedCharacterCells)
        {
            if (selectedCharacter.Occuipied && selectedCharacter.ConnectedPlayer.PlayerID == playerID)
            {
                selectedCharacter.Unconfigure(true);
                break;
            }
        }

        foreach (var characterCell in characterCells)
        {
            if (characterCell.CharacterName == characterName)
            {
                characterCell.SetCharacterSelected(false, playerID);
                break;
            }
        }
    }

    public void Cancel()
    {
        ExitManager.Instance.ExitMatch(SteamLobbyManager.Instance.IsPrivateMatch ? ExitType.HostLeftWithParty : ExitType.Leave);
    }
}
