﻿using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomNetworkRoomPlayer : NetworkRoomPlayer
{
    public override void OnClientEnterRoom()
    {
        if (isLocalPlayer)
        {
            CmdAddConnectedPlayer(index, SteamClient.Name, SteamClient.SteamId.Value, VoiceCommsManager.Instance.ClientId);
            NetworkManager.Instance.RoomPlayer = this;

            if (SceneLoader.IsCharacterScreen && ServerManager.Instance.IsOnlineMatch && !SteamLobbyManager.Instance.IsPrivateMatch && !SteamLobbyManager.Instance.PublicHost) //get the timer in character select screen
                CmdRequestTimer();
        }
    }

    private void OnDestroy()
    {
        if (FightManager.Instance) //this MUST be called before remove player
            GameInterfaces.OnPlayerDisconnected(index);

        if (PanelManager.Instance && ServerManager.Instance && PanelManager.Instance.GetPanel<ChatPanel>() != null && ServerManager.Instance.GetPlayer(index) != null)
        {
            if (ServerManager.Instance.GetPlayerLocal().PlayerID != index)
                PanelManager.Instance.GetPanel<ChatPanel>().DisplayMessage(ServerManager.Instance.GetPlayer(index).Name, "<color=red>Disconnected</color>"); 
        }

        if (ServerManager.Instance)
            ServerManager.Instance.RemovePlayer(index);
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

    [Command]
    public void CmdAddConnectedPlayer(int netID, string steamName, ulong steamId, string voiceCommsId)
    {
        ServerManager.Instance.AddConnectedPlayer(netID, steamName, steamId, voiceCommsId);
        foreach (var connectPlayer in ServerManager.Instance.Players)
        {
            RpcAddConnectedPlayer(connectPlayer.PlayerID, connectPlayer.Name, connectPlayer.SteamId, connectPlayer.VoiceCommsId);
        }
    }

    [ClientRpc]
    private void RpcAddConnectedPlayer(int netID, string steamName, ulong steamId, string voiceCommsId)
    {
        ServerManager.Instance.AddConnectedPlayer(netID, steamName, steamId, voiceCommsId);
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

    [Command]
    public void CmdRequestTimer()
    {
        if (PanelManager.Instance.GetPanel<CharacterSelectScreen>())
            RpcReceiveTimer(PanelManager.Instance.GetPanel<CharacterSelectScreen>().SelectCharacterTimer);
    }

    [ClientRpc]
    private void RpcReceiveTimer(float time)
    {
        if (time == 0 && !isServer) //time hadn't been set yet, request again
            CmdRequestTimer();
        else if (PanelManager.Instance.GetPanel<CharacterSelectScreen>())
            PanelManager.Instance.GetPanel<CharacterSelectScreen>().ConfigureTimer(time);
    }

    [ClientRpc]
    public void RpcBeginPhase(int phase)
    {
        MatchManager.Instance.BeginPhaseClient((MatchManager.RoundPhase)phase);
    }

    [ClientRpc]
    public void RpcCountdownOver()
    {
        FightManager.Instance.CountdownOver();
    }

    [ClientRpc]
    public void RpcSetPlayerSpawnPosition(int playerId, int spawnPositionsIndex)
    {
        MatchManager.Instance.SetPlayerSpawn(ServerManager.Instance.GetPlayer(playerId).PlayerController, spawnPositionsIndex);
    }

    [Command]
    public void CmdRequestReadiedUpClients(int requesterId)
    {
        foreach (var player in ServerManager.Instance.Players)
            RpcReceivedReadiedUpClients(requesterId, player.PlayerID, player.Fighter);
    }

    [ClientRpc]
    private void RpcReceivedReadiedUpClients(int requesterId, int playerId, string characterName)
    {
        if (index == requesterId)
            RpcSelectCharacter(playerId, characterName);
    }
}
