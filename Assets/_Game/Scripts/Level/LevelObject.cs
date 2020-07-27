using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObject : NetworkBehaviour
{
    [SerializeField]
    private Vector2 offset;

    [SerializeField]
    private bool isTrigger;

    private bool placed;

    private LevelObjectData levelObject;
    private LevelEditorPanel editorPanel;
    private Cursor cursor;

    private PlayerController playerController;

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

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        collider = GetComponent<Collider2D>();

        if (ServerManager.Instance.IsOnlineMatch)
            playerController = ServerManager.Instance.GetPlayerLocal().PlayerController;
        else
            playerController = ServerManager.Instance.GetPlayer(cursor.ControllerID).PlayerController;

        collider.isTrigger = hasAuthority || !ServerManager.Instance.IsOnlineMatch;
    }

    private void Update()
    {
        if (cursor == null) //if this hit, it's an object spawned from the server
            ServerConfigure();

        //set opacity of object
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, hasAuthority || placed ? 1 : 0.5f);

        if (!placed && (hasAuthority || !ServerManager.Instance.IsOnlineMatch))
        {
            int roundedSize = LevelEditorManager.Instance.RoundedGridSize;
            Vector3 target = cursor.AssignedCamera.ScreenToWorldPoint(cursor.transform.position);
            target.x = Mathf.Round(target.x / roundedSize) * roundedSize;
            target.y = Mathf.Round(target.y / roundedSize) * roundedSize;

            Vector3 result = new Vector3(target.x + offset.x, target.y + offset.y, transform.position.z);
            transform.position = result;

            if (cursor.InputProfile.Select && Time.time > placeTimer && insideObjectsCount <= 0)
            {
                if (!placed && playerController.PlayerRoundInfo.Purchase(levelObject.Price))
                {
                    Purchased();
                }
            }

            spriteRenderer.color = insideObjectsCount > 0 ? Color.red : Color.white;
        }
    }

    private void Purchased()
    {
        editorPanel.ShowPurchasableBar(true);
        MatchManager.Instance.OnPhaseChanged -= OnPhaseChanged;

        PlaceObject(transform.position);
        CmdPlaceObject(transform.position);
    }

    [Command]
    private void CmdPlaceObject(Vector3 position)
    {
        RpcPlaceObject(position);
    }

    [ClientRpc]
    private void RpcPlaceObject(Vector3 position)
    {
        PlaceObject(position);
    }

    private void PlaceObject(Vector3 position)
    {
        transform.position = position;
        collider.isTrigger = isTrigger;
        placed = true;

        LevelEditorManager.Instance.AddLevelObject(this);
    }

    private void OnPhaseChanged(MatchManager.RoundPhase phase)
    {
        //the buy phase is over and the item wasn't purchased, destroy it
        if (ServerManager.Instance.IsOnlineMatch && hasAuthority)
            NetworkManager.Instance.RoomPlayer.CmdUnspawnObject(gameObject);
        else if (!ServerManager.Instance.IsOnlineMatch)
            Destroy(gameObject);

        MatchManager.Instance.OnPhaseChanged -= OnPhaseChanged;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        LevelObject lvlObj = collision.GetComponent<LevelObject>();
        if (!lvlObj || lvlObj.placed)
        {
            insideObjectsCount++;
        }

        OnTriggerEntered(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        insideObjectsCount = Mathf.Clamp(insideObjectsCount - 1, 0, int.MaxValue);
        OnTriggerExited(collision);
    }

    protected virtual void OnTriggerEntered(Collider2D collider) { }
    protected virtual void OnTriggerExited(Collider2D collider) { }
}
