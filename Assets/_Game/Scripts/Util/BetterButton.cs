using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BetterButton : Button
{
    private Animator animator;

    [SerializeField]
    private UnityEvent onSelected;

    [SerializeField]
    private UnityEvent onUnselected;

    protected override void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void OnSelect(BaseEventData eventData)
    {
        if (animator)
            animator.SetBool("Selected", true);

        onSelected?.Invoke();
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        if (animator)
            animator?.SetBool("Selected", false);

        onUnselected?.Invoke();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (animator)
            animator.SetBool("Selected", true);

        onSelected?.Invoke();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (animator)
            animator?.SetBool("Selected", false);

        onUnselected?.Invoke();
    }
}
