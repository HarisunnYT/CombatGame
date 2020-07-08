using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fisty Cuffs/Fighter Data")]
public class FighterData : ScriptableObject
{
    public string FighterName;
    public Sprite FigherIcon;

    [Space()]
    public PlayerController PlayerControllerPrefab;
    public RuntimeAnimatorController UIAnimatorController;

    [Space()]
    public MoveData[] Moves;
}
