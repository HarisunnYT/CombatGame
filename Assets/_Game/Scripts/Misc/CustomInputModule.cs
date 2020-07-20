using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomInputModule : StandaloneInputModule
{
    protected override void Start()
    {
        Invoke("DelayedDisable", 0.1f);
    }

    private void DelayedDisable()
    {
        enabled = false;
    }
}
