using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommunicationSlice : MonoBehaviour
{
    [SerializeField]
    private MessageData message;
    public MessageData Message { get { return message; } }

    [SerializeField]
    private Image messageImage;

    [SerializeField]
    private GameObject selectedObj;

    private void Awake()
    {
        messageImage.sprite = message.Icon;
    }

    public void SetSelected(bool selected)
    {
        selectedObj.SetActive(selected);
    }
}
