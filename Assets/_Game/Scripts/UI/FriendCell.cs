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

    public void Configure(Friend friend)
    {
        Friend = friend;
        UpdateCell();

        nameText.text = friend.Name;
        profilePictureTask = friend.GetMediumAvatarAsync();
    }

    private void Update()
    {
        if (profilePictureTask != null && profilePictureTask.IsCompleted)
        {
            SetFriendAvatar();
        }
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
        background.color = Friend.IsOnline ? Color.white : offlineColor;

        joinButton.gameObject.SetActive(Friend.IsOnline);
        inviteButton.gameObject.SetActive(Friend.IsOnline);

        joinButton.interactable = Friend.GameInfo.HasValue && Friend.GameInfo.Value.Lobby.HasValue;
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
