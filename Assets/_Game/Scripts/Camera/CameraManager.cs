using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    public Camera Camera { get; private set; }

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
        Camera = GetComponent<Camera>();

        originalPosition = transform.position;
        originalZoom = Camera.orthographicSize;
    }

    private void FixedUpdate()
    {
        if (forcedTarget)
            transform.position = Vector3.Lerp(transform.position, new Vector3(forcedTarget.position.x + offset.x, forcedTarget.position.y + offset.y, transform.position.z), followSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (forcedTarget == null && FightManager.Instance && FightManager.Instance.AlivePlayers.Count > 0)
        {
            Rect boundingBox = CalculateTargetsBoundingBox();
            transform.position = Vector3.Lerp(transform.position, CalculateCameraPosition(boundingBox), followSpeed * Time.deltaTime);
            Camera.orthographicSize = CalculateOrthographicSize(boundingBox);
        }
    }

    Rect CalculateTargetsBoundingBox()
    {
        float minX = Mathf.Infinity;
        float maxX = Mathf.NegativeInfinity;
        float minY = Mathf.Infinity;
        float maxY = Mathf.NegativeInfinity;

        Debug.Log(FightManager.Instance.AlivePlayers.Count);

        foreach (var target in FightManager.Instance.AlivePlayers)
        {
            if (ServerManager.Instance == null || ServerManager.Instance.GetPlayer(target) == null)
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
        boundingBoxCenter = boundingBoxCenter == new Vector2(float.NaN, float.NaN) ? Vector2.zero : boundingBoxCenter;

        return new Vector3(boundingBoxCenter.x, boundingBoxCenter.y, Camera.transform.position.z);
    }

    float CalculateOrthographicSize(Rect boundingBox)
    {
        float orthographicSize = 0;
        float previousOrthographicSize = Camera.orthographicSize;
        Vector3 topRight = new Vector3(boundingBox.x + boundingBox.width, boundingBox.y, 0f);
        Vector3 topRightAsViewport = Camera.WorldToViewportPoint(topRight);

        if (topRightAsViewport.x >= topRightAsViewport.y)
            orthographicSize = Mathf.Abs(boundingBox.width) / Camera.aspect / 2f;
        else
            orthographicSize = Mathf.Abs(boundingBox.height) / 2f;

        previousOrthographicSize = previousOrthographicSize == float.NaN ? 0 : previousOrthographicSize;

        return Mathf.Clamp(Mathf.Lerp(previousOrthographicSize, orthographicSize, Time.deltaTime * zoomSpeed), near, far);
    }

    public void ZoomInOnPlayer(GameObject target, Vector2 offset, float duration = 2.0f, float zoom = 1.0f, System.Action callback = null)
    {
        forcedTarget = target.transform;
        this.offset = offset;

        Camera.DOOrthoSize(zoom, duration).OnComplete(() => callback?.Invoke());
    }

    public void ResetCamera()
    {
        transform.position = originalPosition;
        Camera.orthographicSize = originalZoom;
        forcedTarget = null;
    }
}
