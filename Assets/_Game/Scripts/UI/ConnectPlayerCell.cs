using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectPlayerCell : MonoBehaviour
{
    [SerializeField]
    private TMP_Text playerNameText;

    public int PlayerID { get; private set; } = -1;
    public bool Assigned { get; private set; } = false;

    public void Configure(int playerID, string playerName)
    {
        Configure(playerName);
        PlayerID = playerID;
    }

    public void Configure(string playerName)
    {
        playerNameText.text = playerName;
        gameObject.SetActive(true);

        Assigned = true;
    }

    public void DisableCell()
    {
        Assigned = false;
        gameObject.SetActive(false);
    }
}
