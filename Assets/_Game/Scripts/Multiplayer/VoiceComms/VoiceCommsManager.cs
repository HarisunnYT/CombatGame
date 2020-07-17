using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dissonance.Integrations.SteamworksP2P;
using Steamworks;

public class VoiceCommsManager : PersistentSingleton<VoiceCommsManager>
{
    private SteamworksP2PCommsNetwork commsNetwork;

    protected override void Initialize()
    {
        commsNetwork = GetComponent<SteamworksP2PCommsNetwork>();

        SteamMatchmaking.OnLobbyMemberJoined += PeerConnected;
        SteamMatchmaking.OnLobbyMemberDisconnected += PeerDisconnected;
    }

    public void StartServer()
    {
        commsNetwork.InitializeAsServer();
    }

    public void StartClient()
    {
        SteamId hostId = SteamLobbyManager.Instance.PublicLobby != null ? SteamLobbyManager.Instance.PublicLobby.Value.Owner.Id :
                                                                          SteamLobbyManager.Instance.PrivateLobby.Value.Owner.Id;
        commsNetwork.InitializeAsClient(hostId);
    }

    public void Stop()
    {
        commsNetwork.Stop();
    }

    private void PeerConnected(Steamworks.Data.Lobby arg1, Friend friend)
    {
        commsNetwork.PeerConnected(friend.Id);
    }

    private void PeerDisconnected(Steamworks.Data.Lobby arg1, Friend friend)
    {
        commsNetwork.PeerDisconnected(friend.Id);
    }
}
