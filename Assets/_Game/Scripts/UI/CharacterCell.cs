using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCell : MonoBehaviour
{
    [SerializeField]
    private int characterIndex = 0; 
    public int CharacterIndex { get { return characterIndex; } }

    [SerializeField]
    private GameObject selectedIcon;

    public bool Selected { get; private set; } = false;

    private void Awake()
    {
        selectedIcon.SetActive(false);
    }

    public void CharacterSelected()
    {
        if (!Selected)
        {
            NetworkManager.Instance.RoomPlayer.SelectCharacter(NetworkClient.connection.identity.netId, characterIndex);
        }
    }

    public void SetCharacterSelected(bool selected)
    {
        selectedIcon.SetActive(selected);
        Selected = selected;
    }
}
