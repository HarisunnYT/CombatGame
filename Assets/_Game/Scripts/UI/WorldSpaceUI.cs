using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceUI : MonoBehaviour
{
    private System.Action completeCallback;

    private float duration;
    private float timer = -1;

    public void Display(float duration, System.Action completeCallback = null)
    {
        this.duration = duration;
        this.completeCallback = completeCallback;

        if (duration != -1)
            timer = 0;

        //trick to reset anim
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public void Display(Vector3 localPosition, float duration, System.Action completeCallback = null)
    {
        transform.localPosition = new Vector3(localPosition.x, localPosition.y, transform.localPosition.z);
        Display(duration, completeCallback);
    }

    private void Update()
    {
        if (timer == -1)
            return;

        timer += Time.deltaTime;
        float normTiem = timer / duration;

        if (normTiem > 1.0f)
        {
            HideObject();
        }
    }

    public void HideObject()
    {
        timer = -1;
        completeCallback?.Invoke();
        gameObject.SetActive(false);
    }
}
