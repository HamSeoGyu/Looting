using UnityEngine;

public class UnitDragManager : MonoBehaviour
{
    private Camera mainCam;
    private UnitDrag selectedUnit;

    void Awake()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null) return;
        }

        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        if (Input.GetMouseButtonDown(0))
        {
            Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorld);

            foreach (Collider2D hit in hits)
            {
                UnitDrag drag = hit.GetComponentInParent<UnitDrag>();

                if (drag != null)
                {
                    selectedUnit = drag;
                    selectedUnit.BeginDrag(mouseWorld);
                    break;
                }
            }
        }

        if (selectedUnit != null && Input.GetMouseButton(0))
        {
            selectedUnit.DragTo(mouseWorld);
        }

        if (selectedUnit != null && Input.GetMouseButtonUp(0))
        {
            selectedUnit.EndDrag();
            selectedUnit = null;
        }
    }
}