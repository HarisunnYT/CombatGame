using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRoundInformation : MonoBehaviour
{
    public int Cash { get; private set; }
    public int Wins { get; private set; }

    private const int maxCash = 9999;

    public void AddCash(int amount)
    {
        Cash += amount;
    }

    public void RemoveCash(int amount)
    {
        Cash = Mathf.Clamp(Cash - amount, 0, maxCash);
    }

    public void AddWin()
    {
        Wins++;
    }
}
