using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchasableMoveCell : MoveCell
{
    [Space()]
    [SerializeField]
    private TMP_Text priceText;

    [SerializeField]
    private GameObject equipedObject;

    public bool Equiped { get; private set; }

    private PlayerRoundInformation playerRoundInfo;

    public override void Configure(MoveData moveData)
    {
        base.Configure(moveData);

        if (ServerManager.Instance.IsOnlineMatch)
            playerRoundInfo = ServerManager.Instance.GetPlayerLocal().PlayerController.PlayerRoundInfo;
        else
        {
            LevelEditorCamera cam = GetComponentInParent<LevelEditorCamera>();
            if (cam)
                playerRoundInfo = ServerManager.Instance.GetPlayer(cam.LocalPlayerIndex).PlayerController.PlayerRoundInfo;
        }

        priceText.text = Util.FormatToCurrency(moveData.Price);
        playerRoundInfo.OnEquipedMove += OnEquipedMove;
    }

    private void OnEquipedMove(MoveData move)
    {
        //dont pass through move, we need to check if the move this cell is associated with is equiped, 
        //thus why we pass through MoveData
        equipedObject.SetActive(playerRoundInfo.IsMoveEquiped(MoveData));
    }

    public void OnPressed()
    {
        if (playerRoundInfo.CanPurchase(MoveData.Price))
        {
            purchasePanel.PurchasingMove(this);
        }
    }
}
