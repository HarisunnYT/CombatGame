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
    private Image selectedCharacterImage;

    [SerializeField]
    private CharacterCell[] characterCells;

    private float selectCharacterTimer;
    private bool finished = false;

    private void Awake()
    {
        if (CharacterSelectManager.Instance)
            CharacterSelectManager.Instance.OnCharacterSelected += OnCharacterSelected;
    }

    private void OnDestroy()
    {
        if (CharacterSelectManager.Instance)
            CharacterSelectManager.Instance.OnCharacterSelected -= OnCharacterSelected;
    }

    protected override void OnShow()
    {
        selectCharacterTimer = Time.time + CharacterSelectManager.Instance.CharacterSelectTime;
        selectedCharacterImage.gameObject.SetActive(false);
        finished = false;

        //no countdown for local player
        if (ServerManager.Instance.IsOnlineMatch)
            countdownTimer.Configure(selectCharacterTimer);
        else
            countdownTimer.gameObject.SetActive(false);
    }

    private void Update()
    {
        //we don't countdown in local
        if (ServerManager.Instance.IsOnlineMatch)
        {
            int roundedTime = Mathf.Clamp(Mathf.RoundToInt(selectCharacterTimer - Time.time), 0, int.MaxValue);
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
        selectedCharacterImage.sprite = FighterManager.Instance.GetFighter(characterName).FigherIcon;
        selectedCharacterImage.gameObject.SetActive(true);

        foreach (var characterCell in characterCells)
        {
            if (characterCell.CharacterName == characterName)
            {
                characterCell.SetCharacterSelected(true);
                break;
            }
        }
    }

    public void Cancel()
    {
        CharacterSelectManager.Instance.ExitLobby(false);
    }
}
