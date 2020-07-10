using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CursorManager : PersistentSingleton<CursorManager>
{
    [SerializeField]
    private Cursor cursorPrefab;

    [Space()]
    [SerializeField]
    private Color[] cursorColors;

    private List<Cursor> cursors = new List<Cursor>();
    public List<Cursor> Cursors { get { return cursors; } }

    private Dictionary<Cursor, int> showCursors = new Dictionary<Cursor, int>(); //index to show and hide cursors (value stacks when shown)

    private int lastInteractedPlayerIndex = 0;

    protected override void Initialize()
    {
        base.Initialize();

        //create cursor for player
        CreateCursor(0, default).EnableAllControllerInput();

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

    private Cursor CreateCursor(int playerIndex, System.Guid controllerID)
    {
        Cursor cursor = Instantiate(cursorPrefab, transform);
        cursor.AssignDevice(playerIndex, controllerID, cursorColors[cursors.Count]);
        cursors.Add(cursor);

        cursor.transform.position = new Vector2(Random.Range(Screen.width / 3, Screen.width - Screen.width / 3), Screen.height / 2);

        ShowCursor(cursor);
        return cursor;
    }

    public void HideCursor(Cursor cursor)
    {
        if (showCursors.ContainsKey(cursor))
        {
            showCursors[cursor]--;
            if (showCursors[cursor] <= 0)
            {
                showCursors.Remove(cursor);
                cursor.gameObject.SetActive(false);
            }
        }
    }

    public void HideCursor(int playerIndex)
    {
        foreach (var c in cursors)
        {
            if (c.PlayerIndex == playerIndex)
            {
                HideCursor(c);
            }
        }
    }

    public void HideCursor(System.Guid controllerGUID)
    {
        foreach (var c in cursors)
        {
            if (c.ControllerID == controllerGUID)
            {
                HideCursor(c);
            }
        }
    }

    public void HideAllCursors()
    {
        foreach(var cursor in cursors)
        {
            HideCursor(cursor);
        }
    }

    public void ShowAllCursors()
    {
        foreach(var cursor in cursors)
        {
            ShowCursor(cursor);
        }
    }

    public void ShowCursor(System.Guid controllerGUID)
    {
        foreach (var cursor in cursors)
        {
            if (cursor.ControllerID == controllerGUID)
            {
                ShowCursor(cursor);
            }
        }
    }

    public void ShowCursor(int playerIndex)
    {
        foreach (var cursor in cursors)
        {
            if (cursor.PlayerIndex == playerIndex)
            {
                ShowCursor(cursor);
            }
        }
    }

    public void ShowCursor(Cursor cursor)
    {
        if (showCursors.ContainsKey(cursor))
            showCursors[cursor]++;
        else
        {
            showCursors.Add(cursor, 1);
            cursor.gameObject.SetActive(true);
        }
    }

    public Cursor GetCursor(int playerIndex)
    {
        foreach (var cursor in cursors)
        {
            if (cursor.PlayerIndex == playerIndex)
            {
                return cursor;
            }
        }

        return null;
    }

    public Cursor GetCursor(System.Guid guid)
    {
        foreach (var cursor in cursors)
        {
            if (cursor.ControllerID == guid)
            {
                return cursor;
            }
        }

        return null;
    }

    public Cursor GetLastInteractedCursor()
    {
        return GetCursor(lastInteractedPlayerIndex);
    }
}
