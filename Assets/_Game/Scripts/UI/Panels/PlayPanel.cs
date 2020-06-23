using InControl;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPanel : Panel
{
    private void Awake()
    {
    }


    public void Online()
    {
        if (PlayFabLogin.Instance.LoggedIn)
        {
            PlayFabMatchMaking.Instance.SearchForMatch();
            PanelManager.Instance.ShowPanel<MatchMakingSearchPanel>();

            ServerManager.Instance.IsOnlineMatch = true;
        }
    }

    public void Local()
    {
        ServerManager.Instance.IsOnlineMatch = false;

        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
    }

    
}
