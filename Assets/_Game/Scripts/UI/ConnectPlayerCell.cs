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

    [SerializeField]
    private GameObject isReadyIcon;

    public NetworkConnection Connection { get; private set; } = null;
    public bool Assigned { get; private set; } = false;

    public void Configure(NetworkConnection conn, string playerName)
    {
        playerNameText.text = playerName;
        gameObject.SetActive(true);

        Connection = conn;
        Assigned = true;

        //if the connection is null, it's the client
        if (Connection == null)
        {
            Connection = NetworkClient.connection;
        }
    }

    public void SetReady(bool ready)
    {
        isReadyIcon.SetActive(ready);
    }

    public void DisableCell()
    {
        Assigned = false;
        gameObject.SetActive(false);
    }
}
