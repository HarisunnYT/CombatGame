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
    private int characterIndex = 0; 
    public int CharacterIndex { get { return characterIndex; } }

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
            if (ServerManager.Instance.IsOnlineMatch)
            {
                NetworkManager.Instance.RoomPlayer.SelectCharacter(NetworkClient.connection.identity.netId, characterIndex);
            }
            else
            {
                if (!LocalPlayersManager.Instance.HasLocalPlayerReadiedUp(CursorManager.Instance.GetLastInteractedPlayerIndex()))
                {
                    LocalPlayersManager.Instance.LocalPlayerReadiedUp(CursorManager.Instance.GetLastInteractedPlayerIndex());
                    SetCharacterSelected(true);
                }
            }
        }
    }

    public void SetCharacterSelected(bool selected)
    {
        selectedIcon.SetActive(selected);
        Selected = selected;
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
