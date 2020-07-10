using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PausePanel : Panel
{
    [SerializeField]
    private TMP_Text exitText;

    private InputProfile interactingProfile;

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

        exitText.text = ServerManager.Instance.IsOnlineMatch ? "Leave" : "Exit";
        CursorManager.Instance.ShowCursor(interactingProfile.GUID);
    }

    protected override void OnClose()
    {
        if (CursorManager.Instance && interactingProfile != null)
            CursorManager.Instance.HideCursor(interactingProfile.GUID);

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
