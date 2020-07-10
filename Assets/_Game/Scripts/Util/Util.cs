using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Text;
//using DG.Tweening;

public enum Axis
{
    Forward,
    NegativeForward,
    Left,
    Right,
    Up,
    Down
}

public static class Util
{
    public static bool CheckInsideLayer(LayerMask layerMask, int layer)
    {
        return layerMask == (layerMask | (1 << layer));
    }

    public static string FormatToCurrency(int value)
    {
        string result = value.ToString("C");
        result = result.Remove(result.Length - 3, 3);

        return result;
    }

    public static bool IsFullyVisibleFrom(this RectTransform rectTransform, Camera camera = null)
    {
        if (!rectTransform.gameObject.activeInHierarchy)
            return false;

        return CountCornersVisibleFrom(rectTransform, camera) == 4; // True if all 4 corners are visible
    }

    private static int CountCornersVisibleFrom(this RectTransform rectTransform, Camera camera = null)
    {
        Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
        Vector3[] objectCorners = new Vector3[4];
        rectTransform.GetWorldCorners(objectCorners);

        int visibleCorners = 0;
        Vector3 tempScreenSpaceCorner; // Cached
        for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
        {
            if (camera != null)
                tempScreenSpaceCorner = camera.WorldToScreenPoint(objectCorners[i]); // Transform world space position of corner to screen space
            else
            {
                tempScreenSpaceCorner = objectCorners[i]; // If no camera is provided we assume the canvas is Overlay and world space == screen space
            }

            if (screenBounds.Contains(tempScreenSpaceCorner)) // If the corner is inside the screen
            {
                visibleCorners++;
            }
        }
        return visibleCorners;
    }

    public static void ResetCamera(this List<Cursor> cursors)
    {
        foreach (var cursor in cursors)
        {
            cursor.ResetCamera();
        }
    }
}
