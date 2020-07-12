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
        if (SteamLobbyManager.Instance.PublicHost)
        {
            foreach (var member in SteamLobbyManager.Instance.PublicLobby.Value.Members)
            {
                if (member.Id != SteamClient.SteamId)
                {
                    commsNetwork.PeerConnected(member.Id);
                }
            }

            commsNetwork.InitializeAsServer();
        }
        else
            commsNetwork.InitializeAsClient(SteamLobbyManager.Instance.PublicLobby.Value.Owner.Id);

    }
}
