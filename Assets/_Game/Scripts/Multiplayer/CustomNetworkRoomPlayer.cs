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

        if (index == NetworkManager.Instance.RoomPlayer.index)
        {
            if (!SteamLobbyManager.Instance.PrivateHost || !SteamLobbyManager.Instance.PublicHost)
                VoiceCommsManager.Instance.StartClient();
            else if (SteamLobbyManager.Instance.PrivateHost)
                VoiceCommsManager.Instance.StartServer();
        }
    }

    private void OnDestroy()
    {
        if (ServerManager.Instance)
            ServerManager.Instance.RemovePlayer(index);

        if (index == NetworkManager.Instance.RoomPlayer.index)
            VoiceCommsManager.Instance.Stop();
    }

    [Command]
    public void CmdSelectCharacter(int playerID, string characterName)
    {
        //why do it here you ask? We need to confirm that a client has already selected it
        string charName = characterName;
        if (charName == "random")
            charName = ServerManager.Instance.GetRandomUnselectedCharacter();

        if (!ServerManager.Instance.IsCharacterSelected(charName))
        {
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
            FighterManager.Instance.SetLocalPlayerReady(true);
    }

    [Command]
    public void CmdUnselectCharacter(int playerID, string characterName)
    {
        RpcUnselectCharacter(playerID, characterName);
    }

    [ClientRpc]
    private void RpcUnselectCharacter(int playerID, string characterName)
    {
        CharacterSelectManager.Instance.CharacterUnselected(playerID, characterName);
        ServerManager.Instance.SetCharacterUnselected(characterName);

        NetworkManager.Instance.roomSlots[playerID].readyToBegin = false;
        CursorManager.Instance.ShowCursor(0);

        if (playerID == ServerManager.Instance.GetPlayerLocal().PlayerID)
            FighterManager.Instance.SetLocalPlayerReady(false);
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
            RpcAddConnectedPlayer(connectPlayer.PlayerID, connectPlayer.Name);
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
