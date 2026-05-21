using UnityEngine;

public class UnitDrag : MonoBehaviour
{
    public BuildNode currentNode;
    public float snapRange = 1.2f;

    public bool IsDragging => isDragging;

    private Vector3 dragOffset;
    private Vector3 originalPosition;
    private BuildNode originalNode;
    private bool isDragging = false;

    public void BeginDrag(Vector3 mouseWorld)
    {
        if (isDragging) return;

        isDragging = true;
        originalPosition = transform.position;
        originalNode = currentNode;

        if (originalNode != null)
        {
            originalNode.Clear(gameObject);
        }

        dragOffset = transform.position - mouseWorld;
    }

    public void DragTo(Vector3 mouseWorld)
    {
        if (!isDragging) return;

        Vector3 targetPos = mouseWorld + dragOffset;
        targetPos.z = 0f;
        transform.position = targetPos;
    }

    public void EndDrag()
    {
        if (!isDragging) return;

        isDragging = false;

        BuildNode targetNode = FindNearestNode();

        if (targetNode == null)
        {
            ReturnToOriginalNode();
            return;
        }

        targetNode.SyncState();

        if (targetNode == originalNode)
        {
            PlaceToNode(targetNode);
            return;
        }

        if (targetNode.IsEmpty())
        {
            PlaceToNode(targetNode);
            return;
        }

        GameObject otherUnitObj = targetNode.placedUnit;
        if (otherUnitObj == null)
        {
            PlaceToNode(targetNode);
            return;
        }

        UnitDrag otherDrag = otherUnitObj.GetComponent<UnitDrag>();
        if (otherDrag == null || otherDrag == this)
        {
            ReturnToOriginalNode();
            return;
        }

        if (originalNode == null)
        {
            ReturnToOriginalNode();
            return;
        }

        originalNode.SyncState();

        if (!originalNode.IsEmpty())
        {
            ReturnToOriginalNode();
            return;
        }

        SwapWith(otherDrag, targetNode);
    }

    BuildNode FindNearestNode()
    {
        BuildNode[] nodes = FindObjectsByType<BuildNode>(FindObjectsSortMode.None);

        BuildNode nearestNode = null;
        float nearestDistance = Mathf.Infinity;

        foreach (BuildNode node in nodes)
        {
            if (node == null) continue;

            float distance = Vector2.Distance(transform.position, node.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestNode = node;
            }
        }

        if (nearestDistance <= snapRange)
            return nearestNode;

        return null;
    }

    void PlaceToNode(BuildNode node)
    {
        transform.position = node.transform.position;
        currentNode = node;
        node.SetOccupant(gameObject);
    }

    void ReturnToOriginalNode()
    {
        if (originalNode != null)
        {
            transform.position = originalNode.transform.position;
            currentNode = originalNode;
            originalNode.SetOccupant(gameObject);
        }
        else
        {
            transform.position = originalPosition;
        }
    }

    void SwapWith(UnitDrag otherDrag, BuildNode targetNode)
    {
        BuildNode myOldNode = originalNode;
        GameObject otherUnitObj = otherDrag.gameObject;

        otherDrag.transform.position = myOldNode.transform.position;
        otherDrag.currentNode = myOldNode;
        myOldNode.SetOccupant(otherUnitObj);

        transform.position = targetNode.transform.position;
        currentNode = targetNode;
        targetNode.SetOccupant(gameObject);
    }

    public void SetCurrentNode(BuildNode node)
    {
        currentNode = node;

        if (node != null)
        {
            transform.position = node.transform.position;
            node.SetOccupant(gameObject);
        }
    }
}