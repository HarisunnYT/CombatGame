using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorldSpaceText : WorldSpaceUI
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

    public void Display(string text, Color color, Vector2 startPosition, Vector2 direction, float duration = -1, System.Action completeCallback = null)
    {
        displayText.text = text;
        displayText.color = color;

        transform.position = startPosition;

        Display(duration, completeCallback);

        rigidbody.AddForce(direction, ForceMode2D.Impulse);
    }

    public void Display(string text, Color color, Vector3 startPosition, float force, float duration, System.Action completeCallback = null)
    {
        Vector2 direction = new Vector2(Random.Range(-1.0f, 1.0f), 1);
        Display(text, color, startPosition, direction * force, duration, completeCallback);
    }

    public void Display(string text, Vector3 localPosition, Color color, System.Action completeCallback = null)
    {
        rigidbody.simulated = false;
        Display(text, color, Vector2.zero, Vector2.zero, -1, completeCallback);
        transform.localPosition = localPosition;
    }
}
