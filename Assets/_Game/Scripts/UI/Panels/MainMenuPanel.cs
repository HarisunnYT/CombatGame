using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPanel : Panel
{
    public void Play()
    {
        PanelManager.Instance.ShowPanel<PlayPanel>();
    }
}
