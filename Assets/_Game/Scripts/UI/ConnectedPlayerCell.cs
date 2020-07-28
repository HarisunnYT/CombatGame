using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectedPlayerCell : PlayerCell
{
    [SerializeField]
    private Sprite mutedIcon;

    [SerializeField]
    private Sprite unmutedIcon;

    [SerializeField]
    private Image muteButtonImage;

    private bool isMuted = false;

    public override void Configure(int playerID)
    {
        base.Configure(playerID);

        bool isLocalPlayer = playerID == ServerManager.Instance.GetPlayerLocal().PlayerID;
        muteButtonImage.transform.parent.gameObject.SetActive(!isLocalPlayer);

        isMuted = VoiceCommsManager.Instance.IsPeerMuted(ServerManager.Instance.GetPlayer(playerID).VoiceCommsId);
        SetMuted(isMuted);
    }

    public void Mute()
    {
        Debug.Log("mute");
        isMuted = !isMuted;
        SetMuted(isMuted);
    }

    private void SetMuted(bool muted)
    {
        muteButtonImage.sprite = muted ? mutedIcon : unmutedIcon;
        VoiceCommsManager.Instance.MutePeer(muted, ServerManager.Instance.GetPlayer(playerController).VoiceCommsId);
    }
}
