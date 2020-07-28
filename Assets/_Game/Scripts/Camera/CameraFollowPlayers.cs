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
    private float zoomSpeed = 20f;

    [SerializeField]
    private float boundingBoxPadding = 2f;

    [SerializeField]
    private float near = 5;

    [SerializeField]
    private float far = 10;

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
            transform.position = Vector3.Lerp(transform.position, new Vector3(forcedTarget.position.x + offset.x, forcedTarget.position.y + offset.y, transform.position.z), followSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (forcedTarget == null && FightManager.Instance)
        {
            Rect boundingBox = CalculateTargetsBoundingBox();
            try
            {
                transform.position = Vector3.Lerp(transform.position, CalculateCameraPosition(boundingBox), followSpeed * Time.deltaTime);
            }
            catch { }

            CameraManager.Instance.Camera.orthographicSize = CalculateOrthographicSize(boundingBox);
        }
    }

    Rect CalculateTargetsBoundingBox()
    {
        float minX = Mathf.Infinity;
        float maxX = Mathf.NegativeInfinity;
        float minY = Mathf.Infinity;
        float maxY = Mathf.NegativeInfinity;

        foreach (var target in FightManager.Instance.AlivePlayers)
        {
            if (ServerManager.Instance == null)
                return default;

            PlayerController pc = ServerManager.Instance.GetPlayer(target).PlayerController;
            if (pc == null || !pc.Alive)
                continue;

            Vector3 position = pc.transform.position;

            minX = Mathf.Min(minX, position.x);
            minY = Mathf.Min(minY, position.y);
            maxX = Mathf.Max(maxX, position.x);
            maxY = Mathf.Max(maxY, position.y);
        }

        return Rect.MinMaxRect(minX - boundingBoxPadding, maxY + boundingBoxPadding, maxX + boundingBoxPadding, minY - boundingBoxPadding);
    }

    Vector3 CalculateCameraPosition(Rect boundingBox)
    {
        Vector2 boundingBoxCenter = boundingBox.center;

        return new Vector3(boundingBoxCenter.x, boundingBoxCenter.y, CameraManager.Instance.Camera.transform.position.z);
    }

    float CalculateOrthographicSize(Rect boundingBox)
    {
        float orthographicSize = 0;
        float previousOrthographicSize = CameraManager.Instance.Camera.orthographicSize;
        Vector3 topRight = new Vector3(boundingBox.x + boundingBox.width, boundingBox.y, 0f);
        Vector3 topRightAsViewport = CameraManager.Instance.Camera.WorldToViewportPoint(topRight);

        if (topRightAsViewport.x >= topRightAsViewport.y)
            orthographicSize = Mathf.Abs(boundingBox.width) / CameraManager.Instance.Camera.aspect / 2f;
        else
            orthographicSize = Mathf.Abs(boundingBox.height) / 2f;

        previousOrthographicSize = previousOrthographicSize == float.NaN ? 0 : previousOrthographicSize;

        return Mathf.Clamp(Mathf.Lerp(previousOrthographicSize, orthographicSize, Time.deltaTime * zoomSpeed), near, far);
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
        CameraManager.Instance.Camera.orthographicSize = originalZoom;
        forcedTarget = null;
    }
}
