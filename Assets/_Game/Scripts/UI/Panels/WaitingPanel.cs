﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingPanel : Panel
{
    public void Cancel()
    {
        CharacterSelectManager.Instance.ExitLobby(false);
    }
}
