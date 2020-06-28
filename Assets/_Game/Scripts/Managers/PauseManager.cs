using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : Singleton<PauseManager>
{

    private PausePanel pausePanel;

    private void Start()
    {
        pausePanel = PanelManager.Instance.GetPanel<PausePanel>();
    }

    private void Update()
    {
        foreach(var inputProfile in InputProfile.InputProfiles)
        {
            if (inputProfile.Menu.WasPressed && !pausePanel.gameObject.activeSelf)
            {
                PanelManager.Instance.GetPanel<PausePanel>().Show(inputProfile);
            }
        }    
    }
}
