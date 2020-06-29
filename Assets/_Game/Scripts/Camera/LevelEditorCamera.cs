using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorCamera : MonoBehaviour
{
    public void SetCameraToMainPosition()
    {
        transform.position = CameraManager.Instance.transform.position;
        transform.rotation = CameraManager.Instance.transform.rotation;
    }
}
