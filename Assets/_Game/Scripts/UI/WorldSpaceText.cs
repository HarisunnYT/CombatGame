using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorldSpaceText : MonoBehaviour
{
    [SerializeField]
    private TMP_Text displayText;

    private Rigidbody2D rb;
    private Rigidbody2D rigidbody
    {
        get
        {
            if (rb == null) //lazy initialise
                rb = GetComponent<Rigidbody2D>();
            return rb;
        }
    }

    private Vector3 direction;

    private float duration;
    private float timer = -1;

    public void DisplayText(string text, Color color, Vector2 startPosition, Vector2 direction, float duration)
    {
        this.direction = direction;
        this.duration = duration;

        displayText.text = text;
        displayText.color = color;

        transform.position = startPosition;

        //trick to reset anim
        gameObject.SetActive(false);
        gameObject.SetActive(true);

        rigidbody.AddForce(direction, ForceMode2D.Impulse);

        if (duration != -1)
            timer = 0;
    }

    public void DisplayText(string text, Color color, Vector3 startPosition, float force, float duration)
    {
        Vector2 direction = new Vector2(Random.Range(-1.0f, 1.0f), 1);
        DisplayText(text, color, startPosition, direction * force, duration);
    }

    public void DisplayText(string text, Vector3 localPosition, Color color)
    {
        rigidbody.simulated = false;
        DisplayText(text, color, Vector2.zero, Vector2.zero, -1);
        transform.localPosition = localPosition;
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
        gameObject.SetActive(false);
    }
}
