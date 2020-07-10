using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaveWithPartyPanel : Panel
{
    public void LeaveWithParty()
    {
        ExitManager.Instance.ExitMatchWithParty();
    }

    public void LeaveWithoutParty()
    {
        ExitManager.Instance.ExitMatch(ExitType.Leave);
    }
}
