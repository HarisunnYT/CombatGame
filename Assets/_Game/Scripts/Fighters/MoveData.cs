using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
    Offensive,
    Defensive
}

[CreateAssetMenu(menuName = "Fisty Cuffs/Move Data")]
public class MoveData : ScriptableObject
{
    public string MoveName;
    public int MoveId;

    [Space()]
    public int Cooldown;
    public AttackType AttackType;

    [Space()]
    public Sprite Icon;
    public int Price;

    [Space()]
    [Multiline]
    public string Description;
}
