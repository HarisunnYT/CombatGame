using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dissonance.Integrations.SteamworksP2P;
using Steamworks;
using Dissonance;

public class VoiceCommsManager : PersistentSingleton<VoiceCommsManager>
{
    public string ClientId { get { return comms.LocalPlayerName; } }
    public SteamworksP2PCommsNetwork SteamComms { get; private set; }

    private DissonanceComms comms;

    private bool clientStarted = false;
    private bool serverStarted = false;

    protected override void Initialize()
    {
        SteamComms = GetComponent<SteamworksP2PCommsNetwork>();
        comms = GetComponent<DissonanceComms>();

        SteamMatchmaking.OnLobbyMemberDisconnected += PeerDisconnected;
        ServerManager.Instance.OnPlayerAdded += OnPlayerAdded;
    }

    public void StartServer()
    {
        if (!serverStarted)
        {
            SteamComms.InitializeAsServer();
            serverStarted = true;
        }
    }

    public void StartClient()
    {
        if (!clientStarted)
        {
            SteamId hostId = SteamLobbyManager.Instance.PublicLobby != null ? SteamLobbyManager.Instance.PublicLobby.Value.Owner.Id :
                                                                              SteamLobbyManager.Instance.PrivateLobby.Value.Owner.Id;
            SteamComms.InitializeAsClient(hostId);
            clientStarted = true;
        }
    }

    public void Stop()
    {
        SteamComms.Stop();
        clientStarted = false;
        serverStarted = false;
    }

    public void MutePeer(bool mute, string voiceCommsId)
    {
        comms.FindPlayer(voiceCommsId).IsLocallyMuted = mute;
    }

    public bool IsPeerMuted(string voiceCommsId)
    {
        return comms.FindPlayer(voiceCommsId).IsLocallyMuted;
    }

    public void PeerConnected(Steamworks.Data.Lobby arg1, Friend friend)
    {
        SteamComms.PeerConnected(friend.Id);

        if (friend.Id != SteamClient.SteamId)
            MutePeer(false, ServerManager.Instance.GetPlayer(friend.Id.Value).VoiceCommsId); //unmute player when they enter the room
    }

    private void PeerDisconnected(Steamworks.Data.Lobby arg1, Friend friend)
    {
        SteamComms.PeerDisconnected(friend.Id);
    }

    public void SendChatMessage(string message)
    {
        SteamComms.SendText(message, ChannelType.Room, comms.FindPlayer(ClientId).Rooms[0]);
    }

    private void OnPlayerAdded(ServerManager.ConnectedPlayer connectedPlayer)
    {
        if (serverStarted)
            NetworkManager.Instance.RoomPlayer.VoiceCommsServerCreated();
    }
}
