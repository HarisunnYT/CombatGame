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

    public bool IsPlayer(GameObject obj)
    {
        return obj.layer == LayerMask.NameToLayer("Player");
    }
}
