﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Linq;

public class FriendCell : MonoBehaviour
{
    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private RawImage profileImage;

    [SerializeField]
    private BetterButton joinButton;

    [SerializeField]
    private BetterButton inviteButton;

    [Space()]
    [SerializeField]
    private Image background;

    [SerializeField]
    private Color offlineColor;

    public Friend Friend { get; private set; }

    private Task<Steamworks.Data.Image?> profilePictureTask;

    private bool inviteSent = false;

    public void Configure(Friend friend)
    {
        Friend = friend;

        nameText.text = friend.Name;
        profilePictureTask = friend.GetMediumAvatarAsync();

        UpdateCell();
    }

    private void OnEnable()
    {
        inviteSent = false;
    }

    private void Update()
    {
        if (profilePictureTask != null && profilePictureTask.IsCompleted)
        {
            SetFriendAvatar();
        }

        UpdateCell();
    }

    private void SetFriendAvatar()
    {
        Steamworks.Data.Image img = profilePictureTask.Result.Value;
        Texture2D texture = new Texture2D((int)img.Width, (int)img.Height, TextureFormat.RGBA32, false, true);

        texture.LoadRawTextureData(img.Data);
        texture.Apply();

        profileImage.texture = texture;
        profileImage.color = Color.white;

        profilePictureTask = null;
    }

    public void UpdateCell()
    {
        background.color = Friend.IsOnline && Friend.IsPlayingThisGame ? Color.white : offlineColor;

        bool isPlayingThisGame = Friend.IsPlayingThisGame && Friend.GameInfo.HasValue && Friend.GameInfo.Value.Lobby != null;
        bool alreadyInTheirLobby = SteamLobbyManager.Instance.PrivateLobby.HasValue && SteamLobbyManager.Instance.PrivateLobby.Value.Owner.Id == Friend.Id;
        bool alreadyInMyLobby = isPlayingThisGame && Friend.GameInfo.Value.Lobby.Value.Owner.Id == SteamClient.SteamId;
        bool friendInPrivateLobby = isPlayingThisGame && string.IsNullOrEmpty(Friend.GameInfo.Value.Lobby.Value.GetData(SteamLobbyManager.PublicLobbyKey));
        bool searching = SteamLobbyManager.Instance.Searching;

        if (!inviteSent)
        {
            bool canInvite = SteamLobbyManager.Instance.PrivateLobby != null && SteamLobbyManager.Instance.PublicLobby == null;
            if (canInvite)
                inviteButton.SetInteractable(true);
            else
            {
                if (SteamLobbyManager.Instance.PublicLobby != null)
                    inviteButton.SetInteractable(false, 2);
                else if (alreadyInTheirLobby || alreadyInMyLobby)
                    inviteButton.SetInteractable(false, 3);
                else
                    inviteButton.SetInteractable(false, 0);
            }
        }

        if (Friend.IsOnline && Friend.IsPlayingThisGame)
            transform.SetAsFirstSibling();

        //set join button message index
        bool result = isPlayingThisGame && !alreadyInTheirLobby && !alreadyInMyLobby && friendInPrivateLobby && !searching;
        if (result)
            joinButton.SetInteractable(true);
        else
        {
            if (!isPlayingThisGame || !friendInPrivateLobby)
                joinButton.SetInteractable(false, 0);
            else if (alreadyInTheirLobby || alreadyInMyLobby)
                joinButton.SetInteractable(false, 1);
            else if (searching)
                joinButton.SetInteractable(false, 2);
        }
    }

    public void JoinFriend()
    {
        SteamLobbyManager.Instance.JoinFriendLobby(Friend.GameInfo.Value.Lobby.Value);
        PanelManager.Instance.ClosePanel<FriendsListPanel>();
    }

    public void InviteFriend()
    {
        SteamLobbyManager.Instance.PrivateLobby.Value.InviteFriend(Friend.Id);
        inviteSent = true;
        inviteButton.SetInteractable(false, 1);
    }
}
