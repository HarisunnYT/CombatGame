using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatMessage : MonoBehaviour
{
    [SerializeField]
    private float timeTillHide = 5;

    [SerializeField]
    private TMP_Text chatMessageText;

    private CanvasGroup canvasGroup;

    private float targetTime = -1;

    public bool IsShowing { get; private set; } = true;

    public void Configure(string message)
    {
        chatMessageText.text = message;
        targetTime = Time.time + timeTillHide;

        canvasGroup = GetComponent<CanvasGroup>();
        Show();
    }

    public void Show()
    {
        canvasGroup.DOFade(1, 0.25f);
    }

    public void Hide()
    {
        targetTime = -1;
        IsShowing = false;
        canvasGroup.DOFade(0, 0.25f);
    }

    private void Update()
    {
        if (targetTime != -1 && Time.time > targetTime)
        {
            targetTime = -1;
            IsShowing = false;

            if (!PanelManager.Instance.GetPanel<ChatPanel>().InputOpen)
                canvasGroup.DOFade(0, 0.25f);
        }
    }
}
