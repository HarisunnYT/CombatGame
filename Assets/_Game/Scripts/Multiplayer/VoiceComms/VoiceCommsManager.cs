using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dissonance.Integrations.SteamworksP2P;
using Steamworks;

public class VoiceCommsManager : Singleton<VoiceCommsManager>
{
    [SerializeField]
    private SteamworksP2PCommsNetwork commsNetwork; //TODO move this as it won't work in lobby 

    protected override void Initialize()
    {
        foreach (var member in SteamLobbyManager.Instance.PublicLobby.Value.Members)
        {
            commsNetwork.PeerConnected(member.Id);
        }

        if (SteamLobbyManager.Instance.PublicHost)
        {
            commsNetwork.InitializeAsServer();
        }
        else
            commsNetwork.InitializeAsClient(SteamLobbyManager.Instance.PublicLobby.Value.Owner.Id);

    }
}
