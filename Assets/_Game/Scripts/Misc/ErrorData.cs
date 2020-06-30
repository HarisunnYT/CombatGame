using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fisty Cuffs/Error Data")]
public class ErrorData : ScriptableObject
{
    public string ErrorCode;

    [Multiline]
    public string[] ErrorMessages;
}
