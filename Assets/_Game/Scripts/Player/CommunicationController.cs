using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CommunicationController : NetworkBehaviour
{
    [Header("Wheel")]
    [SerializeField]
    private CommunicationWheel communicationWheelPrefab;

    [SerializeField]
    private Vector3 wheelOffset;

    [Header("Bubble")]
    [SerializeField]
    private WorldSpaceImage speechBubblePrefab;

    [SerializeField]
    private Vector3 speechBubbleOffset;

    private PlayerController playerController;

    private WorldSpaceImage speechBubble;
    private CommunicationWheel commWheel;

    private bool commWheelOpen = false;

    private float inputDelayTimer = 0;
    private const float inputDelay = 0.2f;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

        speechBubble = Instantiate(speechBubblePrefab, transform);
        speechBubble.gameObject.SetActive(false);

        commWheel = Instantiate(communicationWheelPrefab, transform);
        commWheel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isLocalPlayer && ServerManager.Instance && ServerManager.Instance.IsOnlineMatch)
            return;

        //we use this to make sure the user doesn't open the comm wheel and send message in the same few frames
        if (Time.time < inputDelayTimer)
            return;

        if (!commWheelOpen && playerController.InputProfile != null && playerController.InputProfile.CommunicationWheelOpen.WasPressed)
        {
            ShowCommWheel();
            return;
        }

        if (commWheelOpen)
        {
            if (playerController.InputProfile.CommunicationWheelUp)
                ShowSpeechBubble(commWheel.Slices[0].Message);
            if (playerController.InputProfile.CommunicationWheelRight)
                ShowSpeechBubble(commWheel.Slices[1].Message);
            if (playerController.InputProfile.CommunicationWheelDown)
                ShowSpeechBubble(commWheel.Slices[2].Message);
            if (playerController.InputProfile.CommunicationWheelLeft)
                ShowSpeechBubble(commWheel.Slices[3].Message);
        }
    }

    private void ShowCommWheel()
    {
        inputDelayTimer = Time.time + inputDelay;

        commWheelOpen = true;
        commWheel.Display(wheelOffset, 2, () =>
        {
            commWheelOpen = false;
        });
    }

    private void ShowSpeechBubble(MessageData messagedata)
    {
        inputDelayTimer = Time.time + inputDelay;
        commWheel.HideObject();

        //TODO send correct index
        if (ServerManager.Instance.IsOnlineMatch)
            CmdShowSpeechBubble(GetMessageIndex(messagedata));
        else
            ShowBubble(GetMessageIndex(messagedata));
    }

    private int GetMessageIndex(MessageData messagedata)
    {
        for (int i = 0; i < commWheel.Slices.Length; i++)
        {
            if (messagedata == commWheel.Slices[i].Message)
            {
                return i;
            }
        }

        return 0;
    }

    private void ShowBubble(int messageIndex)
    {
        //show and hide is displayed throw animation
        speechBubble.Display(commWheel.Slices[messageIndex].Message.Icon, speechBubbleOffset, 2);
    }

    [Command]
    private void CmdShowSpeechBubble(int messageIndex)
    {
        RpcShowSpeechBubble(messageIndex);
    }

    [ClientRpc]
    private void RpcShowSpeechBubble(int messageIndex)
    {
        ShowBubble(messageIndex);
    }
}
