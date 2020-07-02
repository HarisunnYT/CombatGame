using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchMakingSearchPanel : Panel
{
    protected override void OnShow()
    {
        //SteamMatchMakingManager.Instance.SearchForMatch();
    }

    public void Cancel()
    {
        PanelManager.Instance.ShowPanel<PlayPanel>();
    }
}
