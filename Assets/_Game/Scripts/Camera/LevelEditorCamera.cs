using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorCamera : MonoBehaviour
{
    [SerializeField]
    private int localPlayerIndex = 0;
    public int LocalPlayerIndex { get { return localPlayerIndex; } }

    public void SetCameraToMainPosition()
    {
        transform.position = CameraManager.Instance.transform.position;
        transform.rotation = CameraManager.Instance.transform.rotation;
    }
}
