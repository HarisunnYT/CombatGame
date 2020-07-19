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

        if (ServerManager.Instance.GetPlayerLocal() != null)
        {
            bool isLocalPlayer = playerName == ServerManager.Instance.GetPlayerLocal().Name;
            muteButton.gameObject.SetActive(!isLocalPlayer);
            kickButton.gameObject.SetActive(SteamLobbyManager.Instance.PrivateHost);
        }
    }

    public void MutePlayer(bool mute)
    {
        if (ConnectedPlayer == null)
            ConnectedPlayer = ServerManager.Instance.GetPlayer(playerName); //lazy init

        VoiceCommsManager.Instance.MutePeer(mute, ConnectedPlayer.VoiceCommsId);
    }

    public void Kick()
    {
        if (ConnectedPlayer == null)
            ConnectedPlayer = ServerManager.Instance.GetPlayer(playerName); //lazy init

        SteamLobbyManager.Instance.KickPlayer(ConnectedPlayer.SteamId);
    }
}
