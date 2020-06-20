using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BetterButton : Button
{
    private Animator animator;

    protected override void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void OnSelect(BaseEventData eventData)
    {
        if (animator)
            animator.SetBool("Selected", true);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        if (animator)
            animator?.SetBool("Selected", false);
    }
}
