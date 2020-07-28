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

    private PlayerRoundInformation playerRoundInfo;

    public void Configure(LevelObjectData objectData, LevelEditorPanel levelEditorPanel)
    {
        icon.sprite = objectData.Icon;
        priceText.text = Util.FormatToCurrency(objectData.Price);

        if (ServerManager.Instance.IsOnlineMatch)
            playerRoundInfo = ServerManager.Instance.GetPlayerLocal().PlayerController.PlayerRoundInfo;
        else
        {
            LevelEditorCamera cam = GetComponentInParent<LevelEditorCamera>();
            if (cam)
                playerRoundInfo = ServerManager.Instance.GetPlayer(cam.LocalPlayerIndex).PlayerController.PlayerRoundInfo;
        }

        GetComponent<BetterButton>().SetInteractableMessage("<color=green>" + objectData.ObjectName + "</color>: " + objectData.Description);

        this.objectData = objectData;
        this.levelEditorPanel = levelEditorPanel;
    }

    public void OnPressed()
    {
        if (playerRoundInfo.CanPurchase(objectData.Price))
        {
            levelEditorPanel.ShowPurchasableBar(false);
            LevelEditorManager.Instance.CreateLevelObject(objectData, CursorManager.Instance.GetLastInteractedCursor(), levelEditorPanel);
        }
    }
}
