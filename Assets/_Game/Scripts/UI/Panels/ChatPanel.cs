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
    private bool inputOpen = false;

    private InControlInputModule inputModule;

    private float hideTarget = -1;

    private void Start()
    {
        inputModule = InControlManager.Instance.GetComponent<InControlInputModule>();

        VoiceCommsManager.Instance.SteamComms.TextPacketReceived += SteamComms_TextPacketReceived;
        ShowInput(false);

        if (!ServerManager.Instance.IsOnlineMatch)
            Close();
    }

    private void Update()
    {
        if (CursorManager.Instance == null)
            return;

        if (!inputOpen && CursorManager.Instance.GetLastInteractedProfile().Chat.WasPressed)
            ShowInput(true);
        else if (inputOpen && CursorManager.Instance.GetLastInteractedProfile().Back.WasPressed)
            ShowInput(false);

        if (hideTarget != -1 && Time.time > hideTarget && inputOpen == false)
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
        inputField.gameObject.SetActive(show);
        box.enabled = show;
        inputOpen = show;

        if (show)
        {
            inputField.ActivateInputField();
            inputModule.ActivateModule();
            ShowChat(true, 0.5f);
        }
        else
        {
            inputField.DeactivateInputField();
            inputModule.DeactivateModule();
        }

        if (ServerManager.Instance.GetPlayerLocal().PlayerController != null)
        {
            if (show)
                ServerManager.Instance.GetPlayerLocal().PlayerController.DisableInput();
            else
                ServerManager.Instance.GetPlayerLocal().PlayerController.EnableInput();
        }

        GameManager.Instance.CanPause = !show;
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
