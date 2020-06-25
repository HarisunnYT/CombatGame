using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentMoveCell : MoveCell
{
    [SerializeField]
    private int movePosition;

    public void OnPressed()
    {
        if (purchasePanel.CurrentPurchasingMove != null)
        {
            Configure(purchasePanel.CurrentPurchasingMove.MoveData);
            purchasePanel.PurchasedMove(MoveData, movePosition);
        }
    }
}
