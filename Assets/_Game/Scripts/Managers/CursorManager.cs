using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CursorManager : PersistentSingleton<CursorManager>
{
    [SerializeField]
    private Cursor cursorPrefab;

    private List<Cursor> cursors = new List<Cursor>();

    private int lastInteractedPlayerIndex = 0;

    protected override void Initialize()
    {
        base.Initialize();

        //create cursor for player
        CreateCursor(0, default);

        UnityEngine.Cursor.visible = false;
    }

    private void Start()
    {
        LocalPlayersManager.Instance.OnLocalPlayerConnected += OnLocalPlayerConnected;
    }

    private void OnLocalPlayerConnected(int playerIndex, System.Guid controllerGUID)
    {
        CreateCursor(playerIndex, controllerGUID);
    }

    public void SetLastInteractedPlayer(int playerIndex)
    {
        lastInteractedPlayerIndex = playerIndex;
    }

    public int GetLastInteractedPlayerIndex()
    {
        return lastInteractedPlayerIndex;
    }

    private void CreateCursor(int playerIndex, System.Guid controllerID)
    {
        Cursor cursor = Instantiate(cursorPrefab, transform);
        cursor.AssignDevice(playerIndex, controllerID);

        cursors.Add(cursor);
    }

    public void HideCursor(Cursor cursor)
    {
        foreach (var c in cursors)
        {
            if (c == cursor)
            {
                c.gameObject.SetActive(false);
            }
        }
    }

    public void HideCursor(int playerIndex)
    {
        foreach (var c in cursors)
        {
            if (c.PlayerIndex == playerIndex)
            {
                c.gameObject.SetActive(false);
            }
        }
    }

    public void HideCursor(System.Guid controllerGUID)
    {
        foreach (var c in cursors)
        {
            if (c.ControllerID == controllerGUID)
            {
                c.gameObject.SetActive(false);
            }
        }
    }

    public void HideAllCursors()
    {
        foreach(var cursor in cursors)
        {
            cursor.gameObject.SetActive(false);
        }
    }
}
