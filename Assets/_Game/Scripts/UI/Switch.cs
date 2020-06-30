using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Switch : MonoBehaviour, ISubmitHandler
{
    [SerializeField]
    private Image target;

    [SerializeField]
    private Sprite onSprite;

    [SerializeField]
    private Sprite offSprite;

    [Space()]
    [SerializeField]
    private bool startOn;

    [Space()]
    [SerializeField]
    private UnityEvent onPressed;

    [SerializeField]
    private UnityEvent onPressedOn;

    [SerializeField]
    private UnityEvent onPressedOff;

    private bool isOn;

    private void Awake()
    {
        isOn = startOn;
        SetState(isOn);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        SetState(!isOn);
        onPressed?.Invoke();

        if (isOn)
            onPressedOn?.Invoke();
        else
            onPressedOff?.Invoke();
    }

    private void SetState(bool state)
    {
        isOn = state;
        target.sprite = isOn ? onSprite : offSprite;
    }
}
