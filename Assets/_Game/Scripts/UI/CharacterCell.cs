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
    private InputBasedButton inputBasedButton;

    public bool Selected { get; private set; } = false;

    private bool cursorOverButton = false;

    private Button button;
    private Image[] images;

    private Cursor lastInteractedCursor;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(CharacterSelected);

        images = GetComponentsInChildren<Image>();
    }

    public void CharacterSelected()
    {
        lastInteractedCursor = CursorManager.Instance.GetLastInteractedCursor();

        if (!Selected)
        {
            bool selectedCharacter = FighterManager.Instance.LocalPlayerSelectedCharacter(characterName);
            if (selectedCharacter)
                SetCharacterSelected(true);
        }
    }

    private void Update()
    {
        if (lastInteractedCursor != null && lastInteractedCursor.InputProfile.Back.WasPressed)
        {
            FighterManager.Instance.LocalPlayerUnselectedCharacter(characterName);
            SetCharacterSelected(false);
        }
    }

    public void SetCharacterSelected(bool selected)
    {
        if (characterName != "random")
        {
            foreach(var img in images)
                img.color = selected ? Color.gray : Color.white;

            if (selected)
            {
                inputBasedButton.Configure(lastInteractedCursor.InputProfile);
            }
            else
            {
                inputBasedButton.Hide();
                lastInteractedCursor = null;
            }

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
