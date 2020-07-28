using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayerCell : SelectedCharacterCell
{
    [SerializeField]
    private BetterToggle muteButton;

    [SerializeField]
    private BetterButton kickButton;

    public override void Configure(string playerName, FighterData fighter)
    {
        base.Configure(playerName, fighter);

        ConnectedPlayer = null;

        if (ServerManager.Instance.GetPlayerLocal() != null)
        {
            bool isLocalPlayer = playerName == ServerManager.Instance.GetPlayerLocal().Name;
            muteButton.gameObject.SetActive(!isLocalPlayer);
            kickButton.gameObject.SetActive(SteamLobbyManager.Instance.PrivateHost && !isLocalPlayer);

            if (ConnectedPlayer == null)
                ConnectedPlayer = ServerManager.Instance.GetPlayer(playerName); //lazy init

            MutePlayer(VoiceCommsManager.Instance.IsPeerMuted(ConnectedPlayer.VoiceCommsId));
        }
    }

    public void MutePlayer(bool mute)
    {
        muteButton.ForceSetToggleState(mute);
        VoiceCommsManager.Instance.MutePeer(mute, ConnectedPlayer.VoiceCommsId);
    }

    public void Kick()
    {
        if (ConnectedPlayer == null)
            ConnectedPlayer = ServerManager.Instance.GetPlayer(playerName); //lazy init

        SteamLobbyManager.Instance.KickPlayer(ConnectedPlayer.SteamId);
    }
}
