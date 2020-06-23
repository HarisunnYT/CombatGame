using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraFollowPlayers : MonoBehaviour
{
    [SerializeField]
    private float followSpeed;

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
            //TODO FIX
            //if (MatchManager.Instance.Players.Count == 1)
            //{
            //    Transform target = MatchManager.Instance.Players[0].transform;
            //    transform.position = Vector3.Lerp(transform.position, new Vector3(target.position.x, target.position.y, transform.position.z), followSpeed * Time.deltaTime);
            //}
            //else if (GameManager.Instance.Players.Count == 2)
            //{
            //    Transform p1 = GameManager.Instance.Players[0].transform;
            //    Transform p2 = GameManager.Instance.Players[1].transform;
            //    Vector3 target = p1.position + (p2.position - p1.position) / 2;
            //    transform.position = Vector3.Lerp(transform.position, new Vector3(target.x, target.y, transform.position.z), followSpeed * Time.deltaTime);
            //}
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
