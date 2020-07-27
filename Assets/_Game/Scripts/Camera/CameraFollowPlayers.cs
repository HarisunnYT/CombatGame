using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraFollowPlayers : MonoBehaviour
{
    [SerializeField]
    private float followSpeed = 10;

    [Space()]
    [SerializeField]
    private float near = 5;

    [SerializeField]
    private float far = 10;

    [SerializeField]
    private float minDistance = 25; 

    [SerializeField]
    private float maxDistance = 35; 

    private Vector3 originalPosition;
    private float originalZoom;

    private Transform forcedTarget = null;
    private Vector2 offset;

    private void Start()
    {
        originalPosition = transform.position;
        originalZoom = CameraManager.Instance.Camera.orthographicSize;
    }

    private void FixedUpdate()
    {
        if (forcedTarget)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(forcedTarget.position.x + offset.x, forcedTarget.position.y + offset.y, transform.position.z), followSpeed * Time.deltaTime);
        }
        else
        {
            //get the furthest distance between all the players
            float furthestDistance = 0;
            foreach(var p1 in ServerManager.Instance.Players)
            {
                if (p1.PlayerController != null && p1.PlayerController.Alive)
                {
                    foreach (var p2 in ServerManager.Instance.Players)
                    {
                        if (p1 != p2 && p2.PlayerController != null && p2.PlayerController.Alive)
                        {
                            float distance = Vector3.Distance(p1.PlayerController.transform.position, p2.PlayerController.transform.position);
                            if (distance > furthestDistance)
                                furthestDistance = distance;
                        }
                    }
                }
            }

            float normalizedDistance = (furthestDistance - minDistance) / (maxDistance - minDistance);
            CameraManager.Instance.Camera.orthographicSize = Mathf.Lerp(near, far, normalizedDistance);
        }
    }

    public void ZoomInOnPlayer(GameObject target, Vector2 offset, float duration = 2.0f, float zoom = 1.0f, System.Action callback = null)
    {
        forcedTarget = target.transform;
        this.offset = offset;

        CameraManager.Instance.Camera.DOOrthoSize(zoom, duration).OnComplete(() => callback?.Invoke());
    }

    public void ResetCamera()
    {
        transform.position = originalPosition;

        forcedTarget = null;
        CameraManager.Instance.Camera.orthographicSize = originalZoom;
    }
}
