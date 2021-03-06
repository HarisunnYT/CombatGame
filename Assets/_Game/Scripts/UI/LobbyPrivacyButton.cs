﻿using TMPro;
using UnityEngine;

public class LobbyPrivacyButton : BetterButton
{
    [SerializeField]
    private TMP_Text privacyText;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (Application.isPlaying)
            UpdateText();
    }

    protected override void OnClicked()
    {
        SteamLobbyManager.Instance.SetPrivateLobbyJoinable(!SteamLobbyManager.Instance.PrivateLobbyJoinable);
        UpdateText();
    }

    private void UpdateText()
    {
        privacyText.text = SteamLobbyManager.Instance.PrivateLobbyJoinable ? "Open" : "Invite Only";
    }
}
