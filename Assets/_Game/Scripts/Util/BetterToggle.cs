using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BetterToggle : MonoBehaviour, ISubmitHandler, IInteractableMessage
{
    [SerializeField]
    private Image target;

    [SerializeField]
    private Sprite selectedSprite;

    [SerializeField]
    private Sprite unselectedSprite;

    [Space()]
    [SerializeField]
    private bool startSelected = true;

    [Space()]
    [Multiline]
    [SerializeField]
    private string interactableMessage;

    [Multiline]
    [SerializeField]
    private string[] nonInteractableMessage;

    [Space()]
    [SerializeField]
    private UnityEvent onPressed;

    [SerializeField]
    private UnityEvent onSelected;

    [SerializeField]
    private UnityEvent onDeselected;

    public int NonInteractableIndex { get; set; }
    public bool Interactable { get; set; } = true; //think of this as selected rather than interactable

    private void Awake()
    {
        Interactable = startSelected;
        SetState(Interactable);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        Interactable = !Interactable;
        SetState(Interactable);

        onPressed?.Invoke();
        if (Interactable)
            onSelected?.Invoke();
        else
            onDeselected?.Invoke();
    }

    public void SetState(bool selected)
    {
        target.sprite = selected ? selectedSprite : unselectedSprite;
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
