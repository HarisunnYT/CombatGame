using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public bool IsPlayer(GameObject obj)
    {
        return obj.layer == LayerMask.NameToLayer("Player");
    }
}
