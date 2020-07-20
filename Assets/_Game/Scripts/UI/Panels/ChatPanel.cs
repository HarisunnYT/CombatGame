using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;
using InControl;
using UnityEngine.UI;

public class ChatPanel : Panel
{
    [Tooltip("Time with no message to hide chat")]
    [SerializeField]
    private float noMessageTime = 5;

    [SerializeField]
    private ChatMessage chatMessagePrefab;

    [SerializeField]
    private Transform content;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private Image box;

    private bool chatOpen = false;
    private float hideTarget = -1;

    private StandaloneInputModule inputModule;

    private void Start()
    {
        inputModule = InControlManager.Instance.GetComponent<StandaloneInputModule>();

        VoiceCommsManager.Instance.SteamComms.TextPacketReceived += SteamComms_TextPacketReceived;

        ShowInput(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) //TODO make it work with controller
            ShowInput(true);
        else if (CursorManager.Instance && CursorManager.Instance.GetLastInteractedCursor().InputProfile.Back.WasPressed)
            ShowInput(false);

        if (hideTarget != -1 && Time.time > hideTarget && inputModule.enabled == false)
        {
            ShowChat(false, 0.5f);
            hideTarget = -1;
        }

        if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(inputField.text)) //TODO make it work with controller
        {
            SendMessage();
        }
    }

    public void ShowInput(bool show)
    {
        inputModule.enabled = show;
        inputField.gameObject.SetActive(show);
        box.enabled = show;

        if (show)
        {
            inputField.ActivateInputField();
            ShowChat(true, 0.5f);
        }
    }

    public void SendMessage()
    {
        string message = SteamClient.Name + ": " + inputField.text;
        DisplayMessage(message); 
        inputField.text = "";
        hideTarget = Time.time + noMessageTime;

        inputField.ActivateInputField();
        VoiceCommsManager.Instance.SendChatMessage(message);
    }

    private void DisplayMessage(string message)
    {
        ChatMessage messageObj = Instantiate(chatMessagePrefab, content);
        messageObj.Configure(message);

        ShowChat(true, 0.25f);
        hideTarget = Time.time + noMessageTime;
    }

    private void ShowChat(bool show, float duration)
    {
        canvasGroup.DOFade(show ? 1 : 0, duration);
        chatOpen = show;
    }

    private void SteamComms_TextPacketReceived(Dissonance.Networking.TextMessage obj)
    {
        DisplayMessage(obj.Message);
    }
}
