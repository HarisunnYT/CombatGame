﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dissonance.Integrations.SteamworksP2P;
using Steamworks;
using Dissonance;

public class VoiceCommsManager : PersistentSingleton<VoiceCommsManager>
{
    private DissonanceComms comms;
    private SteamworksP2PCommsNetwork steamComms;

    protected override void Initialize()
    {
        steamComms = GetComponent<SteamworksP2PCommsNetwork>();

        SteamMatchmaking.OnLobbyMemberJoined += PeerConnected;
        SteamMatchmaking.OnLobbyMemberDisconnected += PeerDisconnected;
    }

    public void StartServer()
    {
        steamComms.InitializeAsServer();
    }

    public void StartClient()
    {
        SteamId hostId = SteamLobbyManager.Instance.PublicLobby != null ? SteamLobbyManager.Instance.PublicLobby.Value.Owner.Id :
                                                                          SteamLobbyManager.Instance.PrivateLobby.Value.Owner.Id;
        steamComms.InitializeAsClient(hostId);
    }

    public void Stop()
    {
        steamComms.Stop();
    }

    public void MutePeer(bool mute, string steamId)
    {
        comms.FindPlayer(steamId).IsLocallyMuted = mute;
    }

    private void PeerConnected(Steamworks.Data.Lobby arg1, Friend friend)
    {
        steamComms.PeerConnected(friend.Id);
    }

    private void PeerDisconnected(Steamworks.Data.Lobby arg1, Friend friend)
    {
        steamComms.PeerDisconnected(friend.Id);
    }
}
