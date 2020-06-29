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

    public override void Configure(MoveData moveData)
    {
        base.Configure(moveData);

        priceText.text = Util.FormatToCurrency(moveData.Price);
        FighterManager.Instance.OnEquipedMove += OnEquipedMove;
    }

    private void OnEquipedMove(MoveData move)
    {
        //dont pass through move, we need to check if the move this cell is associated with is equiped, 
        //thus why we pass through MoveData
        equipedObject.SetActive(FighterManager.Instance.IsMoveEquiped(MoveData));
    }

    public void OnPressed()
    {
        if (PlayerRoundInformation.Instance.CanPurchase(MoveData.Price))
        {
            purchasePanel.PurchasingMove(this);
        }
    }
}
