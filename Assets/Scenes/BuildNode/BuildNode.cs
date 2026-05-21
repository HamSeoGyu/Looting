using UnityEngine;

public class BuildNode : MonoBehaviour
{
    public GameObject placedUnit;
    public bool isOccupied = false;

    public void SyncState()
    {
        if (placedUnit == null)
        {
            isOccupied = false;
            return;
        }

        UnitDrag drag = placedUnit.GetComponent<UnitDrag>();
        if (drag == null || drag.currentNode != this)
        {
            placedUnit = null;
            isOccupied = false;
            return;
        }

        isOccupied = true;
    }

    public bool IsEmpty()
    {
        SyncState();
        return placedUnit == null && isOccupied == false;
    }

    public void SetOccupant(GameObject unit)
    {
        placedUnit = unit;
        isOccupied = (unit != null);
    }

    public void Clear(GameObject requester = null)
    {
        if (requester != null && placedUnit != requester)
            return;

        placedUnit = null;
        isOccupied = false;
    }
}