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
    private Animator characterIcon;
    public Animator Animator { get { return characterIcon; } }

    public ServerManager.ConnectedPlayer ConnectedPlayer { get; private set; }
    public bool Occuipied { get; private set; } = false;

    public void Configure(ServerManager.ConnectedPlayer connectedPlayer, FighterData fighter)
    {
        ConnectedPlayer = connectedPlayer;
        Configure(connectedPlayer.Name, fighter);
    }

    public void Configure(string playerName, FighterData fighter)
    {
        playerNameText.text = playerName;
        characterIcon.runtimeAnimatorController = fighter.UIAnimatorController;

        Occuipied = true;
        gameObject.SetActive(true);
    }

    public void Unconfigure(bool hide)
    {
        gameObject.SetActive(!hide);
        Occuipied = false;
    }
}
