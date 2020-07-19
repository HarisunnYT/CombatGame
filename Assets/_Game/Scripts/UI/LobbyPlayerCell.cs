using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayerCell : SelectedCharacterCell
{
    [SerializeField]
    private BetterToggle muteButton;

    [SerializeField]
    private BetterButton kickButton;

    public override void Configure(ServerManager.ConnectedPlayer connectedPlayer, FighterData fighter)
    {
        base.Configure(connectedPlayer, fighter);

        bool isLocalPlayer = connectedPlayer.PlayerID == ServerManager.Instance.GetPlayerLocal().PlayerID;
        muteButton.gameObject.SetActive(!isLocalPlayer);
        kickButton.gameObject.SetActive(!isLocalPlayer);
    }

    public void MutePlayer(bool mute)
    {
        VoiceCommsManager.Instance.MutePeer(mute, ConnectedPlayer.VoiceCommsId);
    }

    public void Kick()
    {
        SteamLobbyManager.Instance.KickPlayer(ConnectedPlayer.SteamId);
    }
}
