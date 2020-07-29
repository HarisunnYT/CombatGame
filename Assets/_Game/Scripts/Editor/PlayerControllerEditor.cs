using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerController), true)]
public class PlayerControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            PlayerController player = target as PlayerController;
            if (GUILayout.Button("Die"))
            {
                player.RpcOnDamaged(ServerManager.Time, 9999, 0);
            }
        }

        base.OnInspectorGUI();
    }
}
