using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CommunicationController : NetworkBehaviour
{
    [SerializeField]
    private Vector3 speechBubbleOffset;

    [SerializeField]
    private WorldSpaceText speechBubblePrefab;

    [SerializeField]
    private MessageData[] messages;

    private PlayerController playerController;
    private WorldSpaceText speechBubble;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

        speechBubble = Instantiate(speechBubblePrefab, transform);
        speechBubble.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isLocalPlayer && ServerManager.Instance.IsOnlineMatch)
            return;

        //TODO show options for all types of text
        if (playerController.InputProfile != null && playerController.InputProfile.CommunicationWheel.WasPressed)
        {
            ShowSpeechBubble();
        }
    }

    private void ShowSpeechBubble()
    {
        //TODO send correct index
        if (ServerManager.Instance.IsOnlineMatch)
            CmdShowSpeechBubble(0);
        else
            ShowBubble(0);
    }

    private void ShowBubble(int messageIndex)
    {
        //show and hide is displayed throw animation
        speechBubble.DisplayText(messages[messageIndex].Message, speechBubbleOffset, messages[messageIndex].Color);
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
