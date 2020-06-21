using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPanel : Panel
{
    [System.Serializable]
    struct LocalPlayerCells
    {
        public NotConnectedCell NotConnectedCell;
        public ConnectPlayerCell ConnectedPlayerCell;
    }

    [SerializeField]
    private LocalPlayerCells[] localPlayerCells; 

    public void Online()
    {
        if (PlayFabLogin.Instance.LoggedIn)
        {
            PlayFabMatchMaking.Instance.SearchForMatch();
            PanelManager.Instance.ShowPanel<MatchMakingSearchPanel>();

            ServerManager.Instance.MatchOnline = true;
        }
    }

    public void Local()
    {
        ServerManager.Instance.MatchOnline = false;

        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
    }
}
