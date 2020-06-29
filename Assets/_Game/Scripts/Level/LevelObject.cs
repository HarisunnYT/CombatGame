using InControl.NativeProfile;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObject : NetworkBehaviour
{
    [SerializeField]
    private string objectName;

    private LevelObjectData levelObject;
    private LevelEditorPanel editorPanel;
    private Cursor cursor;

    private const float placeDelay = 0.25f;
    private float placeTimer;

    public void Configure(LevelObjectData levelObject, Cursor cursor, LevelEditorPanel editorPanel)
    {
        this.levelObject = levelObject;
        this.cursor = cursor;
        this.editorPanel = editorPanel;

        placeTimer = Time.time + placeDelay;
        MatchManager.Instance.OnPhaseChanged += OnPhaseChanged;
    }

    private void ServerConfigure()
    {
        levelObject = LevelEditorManager.Instance.GetDataFromPrefab(gameObject);
        cursor = CursorManager.Instance.GetLastInteractedCursor();
        editorPanel = PanelManager.Instance.GetPanel<LevelEditorPanel>();

        placeTimer = Time.time + placeDelay;
        MatchManager.Instance.OnPhaseChanged += OnPhaseChanged;
    }

    private void Update()
    {
        if (!hasAuthority && ServerManager.Instance.IsOnlineMatch)
            return;
        else if (cursor == null) //if this hit, it's an object spawned from the server
            ServerConfigure();

        Vector3 target = cursor.AssignedCamera.ScreenToWorldPoint(cursor.transform.position);
        transform.position = new Vector3(target.x, target.y, target.z + 1);

        if (cursor.InputProfile.Select && Time.time > placeTimer)
        {
            if (PlayerRoundInformation.Instance.Purchase(levelObject.Price))
            {
                Purchased();
            }
        }
    }

    private void Purchased()
    {
        transform.parent = null;
        editorPanel.ShowPurchasableBar(true);
        MatchManager.Instance.OnPhaseChanged -= OnPhaseChanged;

        Destroy(this);
    }

    private void OnPhaseChanged(MatchManager.RoundPhase phase)
    {
        //the buy phase is over and the item wasn't purchased, destroy it
        if (hasAuthority)
            NetworkManager.Instance.RoomPlayer.CmdUnspawnObject(gameObject);

        MatchManager.Instance.OnPhaseChanged -= OnPhaseChanged;
    }
}
