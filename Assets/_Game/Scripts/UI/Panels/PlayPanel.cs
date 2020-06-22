using InControl;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPanel : Panel
{
    [System.Serializable]
    struct LocalPlayerCells
    {
        public NotConnectedCell NotConnectedCell;
        public ConnectPlayerCell ConnectedPlayerCell;
    }

    [SerializeField]
    private LocalPlayerCells[] localPlayerCells;

    private int currentPlayerIndex = 1;

    private void Awake()
    {
        LocalPlayersManager.Instance.OnLocalPlayerConnected += OnLocalPlayerConnected;
    }

    private void Update()
    {
        foreach (var device in InputManager.ActiveDevices)
        {
            if (device.CommandWasPressed && !LocalPlayersManager.Instance.HasLocalPlayerJoinedAlready(device.GUID))
            {
                LocalPlayersManager.Instance.LocalPlayerJoined(currentPlayerIndex, device.GUID);
                currentPlayerIndex++;

                break;
            }
        }
    }

    private void OnLocalPlayerConnected(int playerIndex, System.Guid controllerGUID)
    {
        //we minus 1 because there is always 1 local player connected
        localPlayerCells[playerIndex - 1].ConnectedPlayerCell.gameObject.SetActive(true);
        localPlayerCells[playerIndex - 1].NotConnectedCell.gameObject.SetActive(false);

        localPlayerCells[playerIndex - 1].ConnectedPlayerCell.Configure(null, "Player " + (playerIndex + 1)); //TODO show proper name (probably steam name + (1))
    }

    public void Online()
    {
        if (PlayFabLogin.Instance.LoggedIn)
        {
            PlayFabMatchMaking.Instance.SearchForMatch();
            PanelManager.Instance.ShowPanel<MatchMakingSearchPanel>();

            ServerManager.Instance.IsOnlineMatch = true;
        }
    }

    public void Local()
    {
        ServerManager.Instance.IsOnlineMatch = false;

        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
    }

    
}
