using Mirror;
using Steamworks;
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
            CmdAddConnectedPlayer(index, SteamClient.Name);
        }
    }

    private void SelectCharacter(int playerID, string characterName)
    {
        LobbyManager.Instance.CharacterSelected(playerID, characterName);
        ServerManager.Instance.SetCharacterSelected(characterName);
    }

    [Command]
    public void CmdSelectCharacter(int playerID, string characterName)
    {
        if (!ServerManager.Instance.IsCharacterSelected(characterName))
        {
            SelectCharacter(playerID, characterName);
            RpcSelectCharacter(playerID, characterName);
        }
    }
    
    [ClientRpc]
    private void RpcSelectCharacter(int playerID, string characterName)
    {
        SelectCharacter(playerID, characterName);
    }

    [Command]
    private void CmdAddConnectedPlayer(int netID, string steamName)
    {
        ServerManager.Instance.AddConnectedPlayer(netID, steamName);
        foreach (var connectPlayer in ServerManager.Instance.Players)
        {
            RpcAddConnectedPlayer(connectPlayer.PlayerID, connectPlayer.SteamName);
        }
    }

    [ClientRpc]
    public void RpcAddConnectedPlayer(int netID, string steamName)
    {
        ServerManager.Instance.AddConnectedPlayer(netID, steamName);
    }

    public void CmdAssignPlayerID(int playerNetID, int playerID)
    {
        AssignPlayerID(playerNetID, playerID);
        RpcAssignPlayerID(playerNetID, playerID);
    }

    [ClientRpc]
    private void RpcAssignPlayerID(int playerNetID, int playerID)
    {
        AssignPlayerID(playerNetID, playerID);
    }

    private void AssignPlayerID(int playerNetID, int playerID)
    {
        ServerManager.Instance.GetPlayer(playerID).NetID = playerNetID;
        foreach(var player in FindObjectsOfType<PlayerController>())
        {
            if (player.netId == playerNetID)
            {
                player.OnAssignedID(playerID);
            }
        }
    }
}
