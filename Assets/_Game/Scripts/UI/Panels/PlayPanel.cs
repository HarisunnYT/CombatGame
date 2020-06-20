using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPanel : Panel
{
    public void Online()
    {
        if (PlayFabLogin.Instance.LoggedIn)
        {
            PlayFabMatchMaking.Instance.SearchForMatch();
            PanelManager.Instance.ShowPanel<MatchMakingSearchPanel>();
        }
    }

    public void Local()
    {

    }
}
