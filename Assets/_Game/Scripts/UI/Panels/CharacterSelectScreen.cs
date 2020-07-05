using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectScreen : Panel
{
    [SerializeField]
    private TMP_Text countdownText;

    [SerializeField]
    private Image selectedCharacterImage;

    [SerializeField]
    private CharacterCell[] characterCells;

    private float selectCharacterTimer;

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
    }

    private void Update()
    {
        int roundedTime = Mathf.Clamp(Mathf.RoundToInt(selectCharacterTimer - Time.time), 0, int.MaxValue);
        countdownText.text = roundedTime.ToString();
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
