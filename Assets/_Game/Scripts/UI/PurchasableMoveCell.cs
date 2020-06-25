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

    public bool Equiped { get; private set; }

    public override void Configure(MoveData moveData)
    {
        base.Configure(moveData);

        priceText.text = Util.FormatToCurrency(moveData.Price);
    }
}
