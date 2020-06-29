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

    private SpriteRenderer spriteRenderer;
    private Collider2D collider;

    private const float placeDelay = 0.25f;
    private float placeTimer;

    private int insideObjectsCount = 0;

    public void Configure(LevelObjectData levelObject, Cursor cursor, LevelEditorPanel editorPanel)
    {
        this.levelObject = levelObject;
        this.cursor = cursor;
        this.editorPanel = editorPanel;

        Setup();
    }

    private void ServerConfigure()
    {
        levelObject = LevelEditorManager.Instance.GetDataFromPrefab(gameObject);
        cursor = CursorManager.Instance.GetLastInteractedCursor();
        editorPanel = PanelManager.Instance.GetPanel<LevelEditorPanel>();

        Setup();
    }

    private void Setup()
    {
        placeTimer = Time.time + placeDelay;
        MatchManager.Instance.OnPhaseChanged += OnPhaseChanged;

        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();

        collider.isTrigger = hasAuthority;
    }

    private void Update()
    {
        if (!hasAuthority && ServerManager.Instance.IsOnlineMatch)
            return;
        else if (cursor == null) //if this hit, it's an object spawned from the server
            ServerConfigure();

        int roundedSize = LevelEditorManager.Instance.RoundedGridSize;
        Vector3 target = cursor.AssignedCamera.ScreenToWorldPoint(cursor.transform.position);
        target.x = Mathf.Round(target.x / roundedSize) * roundedSize;
        target.y = Mathf.Round(target.y / roundedSize) * roundedSize;

        transform.position = new Vector3(target.x, target.y, target.z + 1);

        if (cursor.InputProfile.Select && Time.time > placeTimer && insideObjectsCount <= 0)
        {
            if (PlayerRoundInformation.Instance.Purchase(levelObject.Price))
            {
                Purchased();
            }
        }

        spriteRenderer.color = insideObjectsCount > 0 ? Color.red : Color.white;
    }

    private void Purchased()
    {
        transform.parent = null;
        collider.isTrigger = false;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if it's a level object, that means it hasn't been placed yet so we don't care
        if (!collision.GetComponent<LevelObject>())
        {
            insideObjectsCount++;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        insideObjectsCount = Mathf.Clamp(insideObjectsCount - 1, 0, int.MaxValue);
    }
}
