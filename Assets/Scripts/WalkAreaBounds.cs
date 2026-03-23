using UnityEngine;

public class WalkAreaBounds : MonoBehaviour
{
    public static WalkAreaBounds Instance;

    private PolygonCollider2D poly;

    void Awake()
    {
        Instance = this;
        poly = GetComponent<PolygonCollider2D>();
    }

    public bool IsInsideBounds(Vector2 point)
    {
        if (poly == null) return true;
        return poly.OverlapPoint(point);
    }

    public Vector2 GetCenter()
    {
        if (poly == null) return Vector2.zero;
        return poly.bounds.center;
    }
}