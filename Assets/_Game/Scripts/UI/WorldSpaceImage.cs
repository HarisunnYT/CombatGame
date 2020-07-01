using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceImage : WorldSpaceUI
{
    [SerializeField]
    private Image image;

    public void Display(Sprite icon, Vector3 localPosition, float duration, System.Action completeCallback = null)
    {
        image.sprite = icon;
        Display(localPosition, duration, completeCallback);
    }
}
