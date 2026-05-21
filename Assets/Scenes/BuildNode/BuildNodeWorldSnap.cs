using UnityEngine;

[ExecuteAlways]
public class BuildNodeWorldSnap : MonoBehaviour
{
    [Header("Snap Settings")]
    public Vector2 origin = Vector2.zero;
    public float cellSize = 1f;
    public bool snapX = true;
    public bool snapY = true;

    void Update()
    {
        SnapNow();
    }

    void SnapNow()
    {
        Vector3 pos = transform.position;

        if (snapX)
        {
            float localX = pos.x - origin.x;
            localX = Mathf.Round(localX / cellSize) * cellSize;
            pos.x = origin.x + localX;
        }

        if (snapY)
        {
            float localY = pos.y - origin.y;
            localY = Mathf.Round(localY / cellSize) * cellSize;
            pos.y = origin.y + localY;
        }

        pos.z = 0f;
        transform.position = pos;
    }
}