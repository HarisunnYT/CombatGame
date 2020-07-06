﻿using System.Collections;
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
        PanelManager.Instance.GetPanel<AreYouSurePanel>().Configure(this, () =>
        {
            Time.timeScale = 1;

            //TODO ask user if they want to pull the party or leave by themselves
            if (SteamLobbyManager.Instance.PrivateHost && ServerManager.Instance.IsOnlineMatch)
                MatchManager.Instance.ExitMatchWithParty();
            else
                MatchManager.Instance.ExitMatch(false);
        });
    }

    private void Update()
    {
        if (interactingProfile.Menu.WasPressed)
        {
            Close();
        }
    }
}
