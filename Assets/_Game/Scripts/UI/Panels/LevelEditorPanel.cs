using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelEditorPanel : Panel
{
    [SerializeField]
    private PurchasableLevelObjectCell levelObjectCellPrefab;

    [SerializeField]
    private Timer countdownTimer;

    [SerializeField]
    private Transform levelObjectsContent;

    [SerializeField]
    private GameObject purchasableBar;

    private LevelEditorCamera levelEditorCamera;

    public override void Initialise()
    {
        levelEditorCamera = GetComponentInParent<LevelEditorCamera>();
        MatchManager.Instance.OnPhaseChanged += OnPhaseChanged;
    }

    private void OnDestroy()
    {
        if (MatchManager.Instance)
            MatchManager.Instance.OnPhaseChanged -= OnPhaseChanged;
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

        if (levelEditorCamera)
            levelEditorCamera.SetCameraToMainPosition();

        ShowPurchasableBar(true);

    }

    private void OnPhaseChanged(MatchManager.RoundPhase phase)
    {
        if (phase == MatchManager.RoundPhase.Buy_Phase)
        {
            countdownTimer.Configure((float)NetworkTime.time + MatchManager.Instance.BuyPhaseTimeInSeconds);
        }
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
