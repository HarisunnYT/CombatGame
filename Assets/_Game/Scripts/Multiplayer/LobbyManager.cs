using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : Singleton<LobbyManager>
{
    public delegate void CharacterEvent(uint playerID, int characterID);
    public event CharacterEvent OnCharacterSelected;

    private void Start()
    {
        if (!ServerManager.Instance.IsServer && PlayFabMatchMaking.Instance)
        {
            if (ServerManager.Instance.MatchOnline)
            {
                CreateClient();
            }
            else
            {
                NetworkManager.Instance.StartHost();
            }
        }
    }

    private void CreateClient()
    {
        NetworkManager.Instance.networkAddress = "49.178.32.53";
        NetworkManager.Instance.StartClient();
    }

    public void CharacterSelected(uint connectionID, int characterIndex)
    {
        if (connectionID == NetworkClient.connection.identity.netId)
        {
            NetworkManager.Instance.RoomPlayer.CmdChangeReadyState(true);
        }

        OnCharacterSelected?.Invoke(connectionID, characterIndex);
    }
}
