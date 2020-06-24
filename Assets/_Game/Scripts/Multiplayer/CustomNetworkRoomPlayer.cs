using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomNetworkRoomPlayer : NetworkRoomPlayer
{
    public override void OnClientEnterRoom()
    {
        base.OnClientEnterRoom();

        if (isLocalPlayer)
        {
            NetworkManager.Instance.RoomPlayer = this;
        }
    }

    public void SelectCharacter(uint connectionID, string characterName)
    {
        if (isServer)
            RpcSelectCharacter(connectionID, characterName);
        else 
            CmdSelectCharacter(connectionID, characterName);
    }

    [Command]
    private void CmdSelectCharacter(uint connectionID, string characterName)
    {
        if (!ServerManager.Instance.IsCharacterSelected(characterName))
        {
            RpcSelectCharacter(connectionID, characterName);
        }
    }
    
    [ClientRpc]
    private void RpcSelectCharacter(uint connectionID, string characterName)
    {
        LobbyManager.Instance.CharacterSelected(connectionID, characterName);
        ServerManager.Instance.SetCharacterSelected(characterName);
    }
}
