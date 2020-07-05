using InControl;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private string characterName = ""; 
    public string CharacterName { get { return characterName; } }

    [SerializeField]
    private GameObject selectedIcon;

    public bool Selected { get; private set; } = false;

    private bool cursorOverButton = false;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(CharacterSelected);

        selectedIcon.SetActive(false);
    }

    public void CharacterSelected()
    {
        if (!Selected)
        {
            bool selectedCharacter = FighterManager.Instance.LocalPlayerSelectedCharacter(characterName);
            if (selectedCharacter)
                SetCharacterSelected(true);
        }
    }

    public void SetCharacterSelected(bool selected)
    {
        if (characterName != "random")
        {
            selectedIcon.SetActive(selected);
            Selected = selected;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        cursorOverButton = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cursorOverButton = false;
    }
}
