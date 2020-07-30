using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveCooldownWheel : MonoBehaviour
{
    [SerializeField]
    private int buttonNumber;
    public int ButtonNumber { get { return buttonNumber; } }

    [SerializeField]
    private Image wheel;

    [SerializeField]
    private Color inactiveColor;

    [SerializeField]
    private Color activeColor;

    [SerializeField]
    private float pulseScale = 1.5f;

    [SerializeField]
    private float pulseSpeed = 0.2f;

    private InputBasedButton inputBasedButton;
    private PlayerVarCell playerCell;
    private MoveData move;

    private void Awake()
    {
        playerCell = GetComponentInParent<PlayerVarCell>();
        inputBasedButton = GetComponent<InputBasedButton>();
        inputBasedButton.Configure(playerCell.PlayerController.InputProfile);
    }

    public void Configure(MoveData move)
    {
        this.move = move;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (move != null && playerCell.PlayerController != null)
        {
            CombatController combatController = playerCell.PlayerController.CombatController;
            if (combatController.CanDoMove(buttonNumber))
            {
                float pulse = Mathf.PingPong(Time.time * pulseSpeed, pulseScale) + 1;
                transform.localScale = new Vector3(pulse, pulse, 1);
                wheel.color = activeColor;
                wheel.fillAmount = 1;
            }
            else
            {
                wheel.fillAmount = combatController.GetMoveCooldownState(buttonNumber);
                transform.localScale = Vector3.one;
                wheel.color = inactiveColor;
            }
        }
    }
}
