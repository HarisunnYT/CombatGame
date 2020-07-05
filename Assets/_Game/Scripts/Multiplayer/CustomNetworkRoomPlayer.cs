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
            AddConnectedPlayer(index, SteamClient.Name);
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
        {
            //if the server chose random, they can get a random character easily
            string charName = characterName;
            if (charName == "random")
                charName = ServerManager.Instance.GetRandomUnselectedCharacter();

            RpcSelectCharacter(playerID, charName);
        }
        else
            CmdSelectCharacter(playerID, characterName);
    }

    [Command]
    private void CmdSelectCharacter(int playerID, string characterName)
    {
        //why do it here you ask? We need to confirm that a client has already selected it
        string charName = characterName;
        if (charName == "random")
            charName = ServerManager.Instance.GetRandomUnselectedCharacter();

        if (!ServerManager.Instance.IsCharacterSelected(charName))
        {
            SelectCharacter(playerID, charName);
            RpcSelectCharacter(playerID, charName);
        }
    }
    
    [ClientRpc]
    private void RpcSelectCharacter(int playerID, string characterName)
    {
        CharacterSelectManager.Instance.CharacterSelected(playerID, characterName);
        ServerManager.Instance.SetCharacterSelected(characterName);

        NetworkManager.Instance.roomSlots[playerID].readyToBegin = true;

        if (playerID == ServerManager.Instance.GetPlayerLocal().PlayerID)
            FighterManager.Instance.SetLocalPlayerReady();
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
