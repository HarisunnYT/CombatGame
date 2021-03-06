﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalPlayerUIManager : Singleton<LocalPlayerUIManager>
{
    [SerializeField]
    private Camera[] localCameras;

    [SerializeField]
    private CharacterPurchasePanel[] purchasePanels;

    [SerializeField]
    private LevelEditorPanel[] levelEditorPanels;

    [SerializeField]
    private GraphicRaycaster[] raycasters;

    protected override void Initialize()
    {
        foreach(var panel in GetComponentsInChildren<Panel>(true))
        {
            panel.Initialise();
        }
    }

    public void DisplayLocalScreens(bool showScreens)
    {
        CameraManager.Instance.gameObject.SetActive(!showScreens);

        for (int i = 0; i < LocalPlayersManager.Instance.LocalPlayersCount; i++)
        {
            localCameras[i].gameObject.SetActive(showScreens);

            if (showScreens)
                purchasePanels[i].ShowPanel();
            else
            {
                purchasePanels[i].Close();
                levelEditorPanels[i].Close();
            }

            Cursor cursor = CursorManager.Instance.GetCursor(i);
            if (showScreens)
            {
                cursor.AssignRaycaster(raycasters[i]);
                cursor.AssignCamera(localCameras[i]);
            }
            else
            {
                cursor.ResetRaycaster();
                cursor.ResetCamera();
            }
        }
    }
}
