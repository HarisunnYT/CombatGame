using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PausePanel : Panel
{
    [SerializeField]
    private TMP_Text exitText;

    [SerializeField]
    private ConnectedPlayerCell[] connectPlayerCells;

    private InputProfile interactingProfile;

    public override void Initialise()
    {
        base.Initialise();

        if (ServerManager.Instance.IsOnlineMatch)
        {
            for (int i = 0; i < ServerManager.Instance.Players.Count; i++)
                connectPlayerCells[i].Configure(ServerManager.Instance.Players[i].PlayerID);
        }
    }

    public void Show(InputProfile inputProfile)
    {
        interactingProfile = inputProfile;
        ShowPanel();
    }

    protected override void OnShow()
    {
        if (!ServerManager.Instance.IsOnlineMatch)
        {
            Time.timeScale = 0;
        }

        GameManager.Instance.PauseMenuOpened = true;

        exitText.text = ServerManager.Instance.IsOnlineMatch ? "Leave" : "Exit";
        CursorManager.Instance.ShowCursor(interactingProfile.GUID);

        if (ServerManager.Instance.IsOnlineMatch)
            ServerManager.Instance.GetPlayerLocal().PlayerController.DisableInput();
        else
            ServerManager.Instance.GetPlayer(CursorManager.Instance.GetCursor(interactingProfile.GUID).PlayerIndex).PlayerController.DisableInput();
    }

    protected override void OnClose()
    {
        if (CursorManager.Instance && interactingProfile != null)
        {
            CursorManager.Instance.HideCursor(interactingProfile.GUID);

            if (ServerManager.Instance.IsOnlineMatch)
                ServerManager.Instance.GetPlayerLocal().PlayerController.EnableInput();
            else
                ServerManager.Instance.GetPlayer(CursorManager.Instance.GetCursor(interactingProfile.GUID).PlayerIndex).PlayerController.EnableInput();
        }

        GameManager.Instance.PauseMenuOpened = false;

        Time.timeScale = 1;
    }

    public void Quit()
    {
        //show the leave with party panel if it's the party host, there's more than just them in the private lobby and it isn't a private match
        if (SteamLobbyManager.Instance.PrivateLobby.HasValue && SteamLobbyManager.Instance.PrivateHost && 
            !SteamLobbyManager.Instance.IsPrivateMatch && SteamLobbyManager.Instance.PrivateLobby.Value.MemberCount > 1)
        {
            PanelManager.Instance.ShowPanel<LeaveWithPartyPanel>();
        }
        else
        {
            PanelManager.Instance.GetPanel<AreYouSurePanel>().Configure(this, () =>
            {
                Time.timeScale = 1;

                ExitType exitType;
                if (SteamLobbyManager.Instance.IsPrivateMatch)
                    exitType = ExitType.HostLeftWithParty;
                else if (!SteamLobbyManager.Instance.PrivateLobby.HasValue)
                    exitType = ExitType.LeftLocal;
                else
                    exitType = ExitType.Leave;

                if (exitType == ExitType.HostLeftWithParty)
                    ExitManager.Instance.ExitMatchWithParty();
                else
                    ExitManager.Instance.ExitMatch(exitType);
            });
        }
    }

    private void Update()
    {
        if (interactingProfile.Menu.WasPressed)
        {
            Close();
        }
    }
}
