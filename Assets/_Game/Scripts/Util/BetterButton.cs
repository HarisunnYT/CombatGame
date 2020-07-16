using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BetterButton : Button
{
    private Animator animator;

    [Multiline]
    [SerializeField]
    private string interactableMessage;

    [Multiline]
    [SerializeField]
    private string[] nonInteractableMessage;

    [SerializeField]
    private UnityEvent onSelected;

    [SerializeField]
    private UnityEvent onUnselected;

    private int nonInteractableIndex = 0;

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

    public string GetInteractableMessage()
    {
        return interactableMessage;
    }

    public string GetNonInteractableMessage()
    {
        return nonInteractableMessage[nonInteractableIndex];
    }

    public void SetInteractable(bool interactable, int nonInteractableIndex = 0)
    {
        this.nonInteractableIndex = nonInteractableIndex;
        this.interactable = interactable;
    }
}
