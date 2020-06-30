﻿using InControl.NativeProfile;
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

    [System.NonSerialized]
    [SyncVar]
    public bool Placed;

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
        if (cursor == null) //if this hit, it's an object spawned from the server
            ServerConfigure();

        //set opacity of object
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, hasAuthority || Placed ? 1 : 0.5f);

        if (!Placed && (hasAuthority || !ServerManager.Instance.IsOnlineMatch))
        {
            int roundedSize = LevelEditorManager.Instance.RoundedGridSize;
            Vector3 target = cursor.AssignedCamera.ScreenToWorldPoint(cursor.transform.position);
            target.x = Mathf.Round(target.x / roundedSize) * roundedSize;
            target.y = Mathf.Round(target.y / roundedSize) * roundedSize;

            transform.position = new Vector3(target.x + offset.x, target.y + offset.y, target.z + 1);

            if (cursor.InputProfile.Select && Time.time > placeTimer && insideObjectsCount <= 0)
            {
                if (PlayerRoundInformation.Instance.Purchase(levelObject.Price))
                {
                    Purchased();
                }
            }

            spriteRenderer.color = insideObjectsCount > 0 ? Color.red : Color.white;
        }
    }

    private void Purchased()
    {
        transform.parent = null;
        collider.isTrigger = isTrigger;
        editorPanel.ShowPurchasableBar(true);

        MatchManager.Instance.OnPhaseChanged -= OnPhaseChanged;

        if (isServer)
            RpcPlaceObject();
        else
            CmdPlaceObject();
    }

    [Command]
    private void CmdPlaceObject()
    {
        RpcPlaceObject();
    }

    private void RpcPlaceObject()
    {
        Placed = true;
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
        LevelObject lvlObj = collision.GetComponent<LevelObject>();
        if (!lvlObj || lvlObj.Placed)
        {
            insideObjectsCount++;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        insideObjectsCount = Mathf.Clamp(insideObjectsCount - 1, 0, int.MaxValue);
    }
}
