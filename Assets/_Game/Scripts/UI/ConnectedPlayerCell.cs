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

    public void Mute()
    {
        isMuted = !isMuted;
        muteButtonImage.sprite = isMuted ? mutedIcon : unmutedIcon;

        VoiceCommsManager.Instance.MutePeer(isMuted, ServerManager.Instance.GetPlayer(playerController).VoiceCommsId);
    }
}
