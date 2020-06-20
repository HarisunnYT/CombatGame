using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchMakingSearchPanel : Panel
{
    public void Cancel()
    {
        PlayFabMatchMaking.Instance.CancelMatchMaking();
        PanelManager.Instance.ShowPanel<PlayPanel>();
    }
}
