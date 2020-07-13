using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendsListButton : MonoBehaviour
{
    public void ShowFriendListPanel()
    {
        PanelManager.Instance.ShowPanel<FriendsListPanel>();
    }
}
