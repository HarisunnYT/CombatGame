﻿using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendsListPanel : Panel
{
    [SerializeField]
    private Transform content;

    [SerializeField]
    private FriendCell friendCellPrefab;

    private List<FriendCell> friends = new List<FriendCell>();

    protected override void OnShow()
    {
        //show online friends first
        foreach (var friend in SteamFriends.GetFriends())
        {
            if (friend.IsOnline)
            {
                FriendCell cell = FriendCellCreated(friend);
                if (cell)
                    cell.UpdateCell();
                else
                    ConfigureFriendCell(friend);
            }
        }

        //show offline friends
        foreach (var friend in SteamFriends.GetFriends())
        {
            if (!friend.IsOnline)
            {
                FriendCell cell = FriendCellCreated(friend);
                if (cell)
                    cell.UpdateCell();
                else
                    ConfigureFriendCell(friend);
            }
        }
    }

    private void ConfigureFriendCell(Friend friend)
    {
        FriendCell cell = Instantiate(friendCellPrefab, content);
        cell.Configure(friend);

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