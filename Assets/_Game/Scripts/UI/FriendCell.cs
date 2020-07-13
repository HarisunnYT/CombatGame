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

    public Friend Friend { get; private set; }

    private Task<Steamworks.Data.Image?> profilePictureTask;

    public void Configure(Friend friend)
    {
        this.Friend = friend;

        nameText.text = friend.Name;

        profilePictureTask = friend.GetMediumAvatarAsync();
    }

    private void OnEnable()
    {
        UpdateCell();
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
        joinButton.interactable = Friend.GameInfo.HasValue && Friend.GameInfo.Value.Lobby.HasValue;
    }

    public void JoinFriend()
    {

    }

    public void InviteFriend()
    {

    }
}
