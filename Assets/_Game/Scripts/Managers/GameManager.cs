using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private CharacterData gameData;
    public CharacterData GameData { get { return gameData; } }

    public bool CanPause { get; set; }

    public bool IsPlayer(GameObject obj)
    {
        return obj.layer == LayerMask.NameToLayer("Player");
    }

    private void Update()
    {
        //show debug panel but only in dev builds
        if (Debug.isDebugBuild)
        {
            if ((Input.GetButton("Left Bumper") && Input.GetButton("Right Bumper")) ||
                (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.D)))
            {
                PanelManager.Instance.ShowPanel<DebugPanel>();
            }
        }
    }
}
