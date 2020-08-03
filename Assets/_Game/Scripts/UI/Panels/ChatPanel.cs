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
    private Color nameColor;

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

    private List<ChatMessage> messages = new List<ChatMessage>();

    public bool ChatOpen { get; private set; } = false;

    public bool InputOpen { get; private set; } = false;

    private InControlInputModule inputModule;

    private float hideTarget = -1;

    private void Start()
    {
        inputModule = InControlManager.Instance.GetComponent<InControlInputModule>();

        VoiceCommsManager.Instance.SteamComms.TextPacketReceived += SteamComms_TextPacketReceived;

        inputField.gameObject.SetActive(false);
        box.enabled = false;

        if (GameManager.Instance)
            GameManager.Instance.CanPause = true;

        if (!ServerManager.Instance.IsOnlineMatch)
            Close();

        SteamLobbyManager.Instance.OnPrivateLobbyLeft += ClearMessages;
        ServerManager.Instance.OnPlayerAdded += OnPlayerAdded;
    }

    private void OnDestroy()
    {
        if (SteamLobbyManager.Instance)
            SteamLobbyManager.Instance.OnPrivateLobbyLeft -= ClearMessages;
        if (ServerManager.Instance)
            ServerManager.Instance.OnPlayerAdded -= OnPlayerAdded;

        VoiceCommsManager.Instance.SteamComms.TextPacketReceived -= SteamComms_TextPacketReceived;
    }

    private void Update()
    {
        if (CursorManager.Instance == null)
            return;

        if (!InputOpen && CursorManager.Instance.GetLastInteractedProfile().Chat.WasPressed)
            ShowInput(true);
        else if (InputOpen && CursorManager.Instance.GetLastInteractedProfile().Back.WasPressed)
            ShowInput(false);

        if (hideTarget != -1 && Time.time > hideTarget && InputOpen == false)
        {
            ShowChat(false, 0.5f);
            hideTarget = -1;
        }

        if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(inputField.text)) //TODO make it work with controller
            SendMessage();
    }

    public void ShowInput(bool show)
    {
        inputField.gameObject.SetActive(show);
        box.enabled = show;
        InputOpen = show;

        if (show)
            StartCoroutine(FrameDelayEnableInput());
        else
        {
            inputField.DeactivateInputField();
            inputModule.DeactivateModule();
        }

        //show all previous messages
        for (int i = 0; i < messages.Count; i++)
        {
            if (show)
                messages[i].Show();
            else if (!messages[i].IsShowing)
                messages[i].Hide();
        }

        //disable player movement
        if (ServerManager.Instance.GetPlayerLocal() != null && ServerManager.Instance.GetPlayerLocal().PlayerController != null)
        {
            if (show)
                ServerManager.Instance.GetPlayerLocal().PlayerController.DisableInput();
            else
                ServerManager.Instance.GetPlayerLocal().PlayerController.EnableInput();
        }

        if (show)
            CursorManager.Instance.HideAllCursors();
        else if (SceneLoader.IsMainMenu)
            CursorManager.Instance.ShowAllCursors();

        if (GameManager.Instance)
            GameManager.Instance.CanPause = !show;
    }

    private IEnumerator FrameDelayEnableInput()
    {
        yield return new WaitForEndOfFrame(); //we wait a frame to stop the input for showing dialogue show in dialogue

        inputField.ActivateInputField();
        inputModule.ActivateModule();
        ShowChat(true, 0.5f);
    }

    public void SendMessage()
    {
        string message = DisplayMessage(SteamClient.Name, inputField.text); 
        inputField.text = "";
        hideTarget = Time.time + noMessageTime;

        inputField.ActivateInputField();
        VoiceCommsManager.Instance.SendChatMessage(message);
    }

    public void DisplayMessage(string message)
    {
        ChatMessage messageObj = Instantiate(chatMessagePrefab, content);
        message = ProfanityFilter.ReplaceProfanity(message); //replace profanity
        messageObj.Configure(message);
        messages.Add(messageObj);

        ShowChat(true, 0.25f);
        hideTarget = Time.time + noMessageTime;
    }

    public string DisplayMessage(string playerName, string message)
    {
        playerName = string.Format("<color=#{0}>{1}: </color>", ColorUtility.ToHtmlStringRGB(nameColor), playerName);
        string result = playerName + message;
        DisplayMessage(result);

        return result;
    }

    private void ShowChat(bool show, float duration)
    {
        canvasGroup.DOFade(show ? 1 : 0, duration);
        ChatOpen = show;
    }

    private void SteamComms_TextPacketReceived(Dissonance.Networking.TextMessage obj)
    {
        DisplayMessage(obj.Message);
    }

    public void ClearMessages()
    {
        for (int i = 0; i < messages.Count; i++)
        {
            Destroy(messages[i].gameObject);
        }

        messages.Clear();
    }

    private void OnPlayerAdded(ServerManager.ConnectedPlayer connectedPlayer)
    {
        if (ServerManager.Instance.GetPlayerLocal() != connectedPlayer)
            DisplayMessage(connectedPlayer.Name, "<color=green>Connected</color>");
    }
}
