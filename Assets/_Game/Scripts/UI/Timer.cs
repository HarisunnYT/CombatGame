using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using Mirror;
using UnityEngine.Networking;
using Mirror.FizzySteam;

public class Timer : NetworkBehaviour
{
    private float targetTime;
    private int previousRoundedTime;

    private TMP_Text text;
    private Color originalColor;

    private bool flashRedNearFinish;
    private bool pulseNearFinish;

    public void Configure(float targetTime, bool flashRedNearFinish = true, bool pulseNearFinish = true)
    {
        if (text == null)
        {
            text = GetComponent<TMP_Text>();
            originalColor = text.color;
        }

        gameObject.SetActive(true);

        if (SteamLobbyManager.Instance.PublicHost)
        {
            this.targetTime = targetTime;
            RpcSetTargetTime(targetTime);
        }

        this.flashRedNearFinish = flashRedNearFinish;
        this.pulseNearFinish = pulseNearFinish;
        text.color = originalColor;
    }

    public void OverrideTargetTime(float time)
    {
        if (targetTime == 0)
            targetTime = time;
    }

    private void Update()
    {
        float time = (float)NetworkTime.time;
        int roundedTime = Mathf.Clamp(Mathf.RoundToInt(targetTime - time), 0, int.MaxValue);
        if (roundedTime != previousRoundedTime)
        {
            UpdateText(roundedTime.ToString());

            if (roundedTime <= 5)
            {
                if (flashRedNearFinish)
                    text.color = new Color(1, 0.3f, 0.3f, 1); //not so harsh red

                if (pulseNearFinish)
                    text.transform.DOPunchScale(Vector3.one * 0.5f, 0.2f);
            }
        }

        previousRoundedTime = roundedTime;
    }

    private void UpdateText(string str)
    {
        text.text = str;
    }

    [ClientRpc]
    private void RpcSetTargetTime(float time)
    {
        targetTime = time;
    }
}
