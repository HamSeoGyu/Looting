using UnityEngine;

[ExecuteAlways]
public class GridPointSnap : MonoBehaviour
{
    private Grid grid;

    void LateUpdate()
    {
        if (Application.isPlaying) return;

        if (grid == null)
        {
            grid = FindFirstObjectByType<Grid>();
        }

        if (grid == null) return;

        Vector3Int cellPos = grid.WorldToCell(transform.position);
        transform.position = grid.GetCellCenterWorld(cellPos);
    }
}