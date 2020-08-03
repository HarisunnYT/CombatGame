using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BetterButton : Button, IInteractableMessage
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

    public int NonInteractableIndex { get; set; }
    public bool Interactable { get; set; } = true;

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

    public override void OnSubmit(BaseEventData eventData)
    {
        base.OnSubmit(eventData);

        OnClicked();
    }

    protected virtual void OnClicked() { }

    public void SetInteractable(bool interactable, int nonInteractableIndex = 0)
    {
        NonInteractableIndex = nonInteractableIndex;
        this.interactable = interactable;
        Interactable = interactable;
    }

    public void SetInteractableMessage(string message)
    {
        interactableMessage = message;
    }

    public string GetInteractableMessage()
    {
        return interactableMessage;
    }

    public string GetNonInteractableMessage()
    {
        return nonInteractableMessage[NonInteractableIndex];
    }
}
