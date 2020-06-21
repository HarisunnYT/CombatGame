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

    public void SelectCharacter(uint connectionID, int characterIndex)
    {
        if (isServer)
            RpcSelectCharacter(connectionID, characterIndex);
        else 
            CmdSelectCharacter(connectionID, characterIndex);
    }

    [Command]
    private void CmdSelectCharacter(uint connectionID, int characterIndex)
    {
        if (!ServerManager.Instance.IsCharacterSelected(characterIndex))
        {
            RpcSelectCharacter(connectionID, characterIndex);
        }
    }
    
    [ClientRpc]
    private void RpcSelectCharacter(uint connectionID, int characterIndex)
    {
        LobbyManager.Instance.CharacterSelected(connectionID, characterIndex);
        ServerManager.Instance.SetCharacterSelected(characterIndex);
    }
}
