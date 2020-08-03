using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LobbyPrivacyButton : BetterButton
{
    [SerializeField]
    private TMP_Text privacyText;

    protected override void OnEnable()
    {
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
