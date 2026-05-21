using UnityEngine;

public class LoopRoute : MonoBehaviour
{
    public Transform[] points;

    void Awake()
    {
        if (points == null || points.Length == 0)
        {
            points = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                points[i] = transform.GetChild(i);
            }
        }
    }

    public Transform GetPoint(int index)
    {
        if (points == null || points.Length == 0) return null;

        index = Mathf.Clamp(index, 0, points.Length - 1);
        return points[index];
    }

    public int Count
    {
        get { return points != null ? points.Length : 0; }
    }
}