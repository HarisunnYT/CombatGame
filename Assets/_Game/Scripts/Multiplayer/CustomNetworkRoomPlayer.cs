using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplayerBasicExample;

public class CustomNetworkRoomPlayer : NetworkRoomPlayer
{
    public override void OnClientEnterRoom()
    {
        base.OnClientEnterRoom();

        if (isLocalPlayer)
        {
            NetworkManager.Instance.RoomPlayer = this;
            //AddConnectedPlayer(index, SteamClient.Name);
        }
    }

    private void OnDestroy()
    {
        if (ServerManager.Instance)
        {
            ServerManager.Instance.RemovePlayer(index);
        }
    }

    public void SelectCharacter(int playerID, string characterName)
    {
        if (isServer)
            RpcSelectCharacter(playerID, characterName);
        else
            CmdSelectCharacter(playerID, characterName);
    }

    [Command]
    private void CmdSelectCharacter(int playerID, string characterName)
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
        LobbyManager.Instance.CharacterSelected(playerID, characterName);
        ServerManager.Instance.SetCharacterSelected(characterName);

        NetworkManager.Instance.roomSlots[playerID].readyToBegin = true;
    }

    public void AddConnectedPlayer(int netID, string steamName)
    {
        if (isServer)
            RpcAddConnectedPlayer(netID, steamName);
        else
            CmdAddConnectedPlayer(netID, steamName);
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
    private void RpcAddConnectedPlayer(int netID, string steamName)
    {
        ServerManager.Instance.AddConnectedPlayer(netID, steamName);
    }

    public void CmdAssignPlayerID(int[] playerNetID, int[] playerID)
    {
        RpcAssignPlayerID(playerNetID, playerID);
    }

    [ClientRpc]
    private void RpcAssignPlayerID(int[] playerNetID, int[] playerID)
    {
        for (int i = 0; i < playerNetID.Length; i++)
        {
            ServerManager.Instance.GetPlayer(playerID[i]).NetID = playerNetID[i];
            foreach (var player in FindObjectsOfType<PlayerController>())
            {
                if (player.netId == playerNetID[i])
                {
                    player.OnAssignedID(playerID[i]);
                    break;
                }
            }
        }
    }

    [Command]
    public void CmdSpawnObject(int prefabID, NetworkConnectionToClient conn)
    {
        if (prefabID != -1)
        {
            GameObject instantiatedObject = Instantiate(NetworkManager.Instance.GetPrefabFromID(prefabID));
            NetworkServer.Spawn(instantiatedObject, conn);
        }
    }

    [Command]
    public void CmdUnspawnObject(GameObject obj)
    {
        obj.SetActive(false);
        NetworkServer.UnSpawn(obj);
    }

    [Command]
    public void CmdFightOver(int winnerPlayerID)
    {
        RpcFightOver(winnerPlayerID);
    }

    [ClientRpc]
    private void RpcFightOver(int winnerPlayerID)
    {
        FightManager.Instance.FightOver(winnerPlayerID);
    }
}
