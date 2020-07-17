using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FriendsListPanel : Panel
{
    [SerializeField]
    private Transform content;

    [SerializeField]
    private FriendCell friendCellPrefab;

    [Space()]
    [SerializeField]
    private TMP_Text messageText;

    [SerializeField]
    private string offlineString;

    [SerializeField]
    private string noFriendsOnlineString;

    private List<FriendCell> friends = new List<FriendCell>();

    protected override void OnShow()
    {
        bool hasFriendOnline = false;
        bool isClientOffline = SteamClient.State == FriendState.Offline;

        //show online friends first
        foreach (var friend in SteamFriends.GetFriends())
        {
            if (friend.IsOnline)
            {
                FriendCell cell = FriendCellCreated(friend);
                if (!cell)
                    ConfigureFriendCell(friend);

                hasFriendOnline = true;
            }
        }

        messageText.gameObject.SetActive(!hasFriendOnline || isClientOffline);
        messageText.text = isClientOffline  ? offlineString : noFriendsOnlineString;
    }

    private void Update()
    {
        if (CursorManager.Instance.GetLastInteractedCursor().InputProfile.Back.WasPressed)
            Close();
    }

    private void ConfigureFriendCell(Friend friend)
    {
        FriendCell cell = Instantiate(friendCellPrefab, content);
        cell.Configure(friend);
        cell.transform.SetAsLastSibling();

        friends.Add(cell);
    }

    private FriendCell FriendCellCreated(Friend friend)
    {
        foreach(var friendCell in friends)
        {
            if (friend.Id == friendCell.Friend.Id)
            {
                return friendCell;
            }
        }

        return null;
    }
}
