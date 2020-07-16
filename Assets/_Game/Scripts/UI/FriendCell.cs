using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;

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

    private bool wasOnline = false;

    public void Configure(Friend friend)
    {
        Friend = friend;

        nameText.text = friend.Name;
        profilePictureTask = friend.GetMediumAvatarAsync();

        wasOnline = friend.IsOnline;
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

        joinButton.gameObject.SetActive(Friend.IsOnline && Friend.IsPlayingThisGame);
        inviteButton.gameObject.SetActive(Friend.IsOnline);

        if (Friend.IsOnline != wasOnline)
        {
            wasOnline = Friend.IsOnline;
            if (wasOnline)
                transform.SetAsFirstSibling();
        }

        try
        {
            joinButton.interactable = Friend.GameInfo.Value.Lobby != null;
        }
        catch { }
    }

    public void JoinFriend()
    {
        SteamLobbyManager.Instance.JoinFriendLobby(Friend.GameInfo.Value.Lobby.Value);
    }

    public void InviteFriend()
    {
        Friend.InviteToGame("");
    }
}
