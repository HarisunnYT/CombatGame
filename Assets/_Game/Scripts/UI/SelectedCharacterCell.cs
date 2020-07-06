using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectedCharacterCell : MonoBehaviour
{
    [SerializeField]
    private TMP_Text playerNameText;

    [SerializeField]
    private Image characterIcon;

    public ServerManager.ConnectedPlayer ConnectedPlayer { get; private set; }
    public bool Occuipied { get; private set; } = false;

    public void Configure(ServerManager.ConnectedPlayer connectedPlayer, FighterData fighter)
    {
        ConnectedPlayer = connectedPlayer;

        playerNameText.text = connectedPlayer.Name;
        characterIcon.sprite = fighter.FigherIcon;

        Occuipied = true;
        gameObject.SetActive(true);
    }

    public void Unconfigure()
    {
        Occuipied = false;
        gameObject.SetActive(false);
    }
}
