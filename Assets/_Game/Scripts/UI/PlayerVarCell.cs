using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerVarCell : PlayerCell
{
    [SerializeField]
    private Image healthBar;

    [SerializeField]
    private MoveCooldownWheel[] moveCooldownWheels;

    protected override void OnPlayerCreated(int playerID, PlayerController playerController)
    {
        //only create this cell if it's the correct player id it's assigned to
        if (playerID == this.playerID)
        {
            base.OnPlayerCreated(playerID, playerController);

            playerController.OnHealthChanged += OnHealthChanged;
            playerController.PlayerRoundInfo.OnEquipedMove += OnEquipedMove;
        }
    }

    private void OnDestroy()
    {
        PlayerController.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int health)
    {
        healthBar.fillAmount = (float)health / 100;
    }

    private void OnEquipedMove(MoveData move, int position)
    {
        foreach(var moveCooldownWheel in moveCooldownWheels)
        {
            if (moveCooldownWheel.ButtonNumber == position)
                moveCooldownWheel.Configure(move);
        }
    }
}
