using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchasableLevelObjectCell : MonoBehaviour
{
    [SerializeField]
    private Image icon;

    [SerializeField]
    private TMP_Text priceText;

    private LevelEditorPanel levelEditorPanel;
    private LevelObjectData objectData;

    public void Configure(LevelObjectData objectData, LevelEditorPanel levelEditorPanel)
    {
        icon.sprite = objectData.Icon;
        priceText.text = Util.FormatToCurrency(objectData.Price);

        this.objectData = objectData;
        this.levelEditorPanel = levelEditorPanel;
    }

    public void OnPressed()
    {
        if (PlayerRoundInformation.Instance.CanPurchase(objectData.Price))
        {
            levelEditorPanel.ShowPurchasableBar(false);
            LevelEditorManager.Instance.CreateLevelObject(objectData, CursorManager.Instance.GetLastInteractedCursor(), levelEditorPanel);
        }
    }
}
