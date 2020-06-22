using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Text;
//using DG.Tweening;

public enum Axis
{
    Forward,
    NegativeForward,
    Left,
    Right,
    Up,
    Down
}

public static class Util
{
    public static bool CheckInsideLayer(LayerMask layerMask, int layer)
    {
        return layerMask == (layerMask | (1 << layer));
    }

    public static string FormatToCurrency(int value)
    {
        string result = value.ToString("C");
        result = result.Remove(result.Length - 3, 3);

        return result;
    }
}
