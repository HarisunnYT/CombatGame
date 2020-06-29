using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fisty Cuffs/Level Object Data")]
public class LevelObjectData : ScriptableObject
{
    public string ObjectName;
    public Sprite Icon;

    [Space()]
    public LevelObject Prefab;

    [Space()]
    public int Price;
}
