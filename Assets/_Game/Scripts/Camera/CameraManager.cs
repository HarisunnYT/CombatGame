using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    public Camera Camera { get; private set; }

    public CameraFollowPlayers CameraFollow { get; private set; }

    protected override void Initialize()
    {
        Camera = GetComponent<Camera>();
        CameraFollow = GetComponent<CameraFollowPlayers>();
    }
}
