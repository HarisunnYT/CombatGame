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

        muteButtonImage.transform.parent.gameObject.SetActive(playerID != ServerManager.Instance.GetPlayerLocal().PlayerID);
    }

    public void Mute()
    {
        isMuted = !isMuted;
        muteButtonImage.sprite = isMuted ? mutedIcon : unmutedIcon;

        VoiceCommsManager.Instance.MutePeer(isMuted, ServerManager.Instance.GetPlayer(playerController).VoiceCommsId);
    }
}
