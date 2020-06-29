using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelEditorPanel : Panel
{
    [SerializeField]
    private PurchasableLevelObjectCell levelObjectCellPrefab;

    [SerializeField]
    private TMP_Text countdownTimer;

    [SerializeField]
    private Transform levelObjectsContent;

    [SerializeField]
    private GameObject purchasableBar;

    private LevelEditorCamera levelEditorCamera;

    public override void Initialise()
    {
        levelEditorCamera = GetComponentInParent<LevelEditorCamera>();
    }

    private void Awake()
    {
        foreach(var obj in LevelEditorManager.Instance.LevelObjectsList.Items)
        {
            CreateObjectCell(obj as LevelObjectData);
        }
    }

    protected override void OnShow()
    {
        CameraManager.Instance.CameraFollow.ResetCamera();
        CursorManager.Instance.ShowAllCursors();

        if (levelEditorCamera)
            levelEditorCamera.SetCameraToMainPosition();

        ShowPurchasableBar(true);

        MatchManager.Instance.OnBuyPhaseTimePassed += UpdateCountdownTimer;
    }

    protected override void OnClose()
    {
        MatchManager.Instance.OnBuyPhaseTimePassed -= UpdateCountdownTimer;
    }

    public void UpdateCountdownTimer(int time)
    {
        countdownTimer.text = time.ToString();
    }

    public void OpenCharacterPanel()
    {
        if (ServerManager.Instance.IsOnlineMatch)
            PanelManager.Instance.ShowPanel<CharacterPurchasePanel>();
        else
        {
            transform.parent.GetComponentInChildren<CharacterPurchasePanel>(true).ShowPanel(); //if it's local, get the character panel that's on the same parent as this
            Close();
        }
    }

    private void CreateObjectCell(LevelObjectData objectData)
    {
        PurchasableLevelObjectCell cell = Instantiate(levelObjectCellPrefab, levelObjectsContent);
        cell.Configure(objectData, this);
    }

    public void ShowPurchasableBar(bool show)
    {
        purchasableBar.SetActive(show);
    }
}
