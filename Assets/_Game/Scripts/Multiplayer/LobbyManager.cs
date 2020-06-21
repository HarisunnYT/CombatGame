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
            CreateClient();
        }
    }

    private void CreateClient()
    {
        NetworkManager.singleton.networkAddress = "172.197.128.73";
        NetworkManager.singleton.StartClient();
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
