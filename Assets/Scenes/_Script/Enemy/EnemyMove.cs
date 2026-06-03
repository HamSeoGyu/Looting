using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    [Header("Route")]
    public LoopRoute route;

    [Header("Move")]
    public float moveSpeed = 1.5f;
    public float reachDistance = 0.05f;

    [Header("Facing")]
    public Transform visualRoot;
    public bool invertFacing = false;

    private int currentIndex = 0;
    private Vector3 originalScale;
    private bool initializedOnRoute = false;

    [Header("Freeze")]
    public bool isFrozen = false;
    private float freezeEndTime = 0f;

    [Header("Slow")]
    public bool isSlowed = false;
    private float slowEndTime = 0f;
    private float slowMultiplier = 1f;

    void Start()
    {
        CacheOriginalScale();

        if (initializedOnRoute)
            return;

        if (route != null && route.Count > 0)
        {
            Transform startPoint = route.GetPoint(0);
            if (startPoint != null)
            {
                transform.position = startPoint.position;
            }

            currentIndex = route.Count > 1 ? 1 : 0;
            FaceCurrentDirection();
        }
    }

    void Update()
    {
        UpdateFreezeState();
        UpdateSlowState();

        if (isFrozen) return;
        if (route == null || route.Count == 0) return;

        Transform targetPoint = route.GetPoint(currentIndex);
        if (targetPoint == null) return;

        Vector3 dir = targetPoint.position - transform.position;

        UpdateFacing(dir, targetPoint);

        if (dir.magnitude <= reachDistance)
        {
            transform.position = targetPoint.position;
            currentIndex = (currentIndex + 1) % route.Count;
            return;
        }

        float currentMoveSpeed = moveSpeed;

        if (isSlowed)
        {
            currentMoveSpeed *= slowMultiplier;
        }

        transform.position += dir.normalized * currentMoveSpeed * Time.deltaTime;
    }

    public void InitializeOnRoute(LoopRoute newRoute, Vector3 spawnPosition, bool useNearestRouteSegment = true)
    {
        route = newRoute;
        transform.position = spawnPosition;

        CacheOriginalScale();

        if (route != null && route.Count > 0)
        {
            if (useNearestRouteSegment)
            {
                currentIndex = FindNextRoutePointIndex(spawnPosition);
            }
            else
            {
                currentIndex = route.Count > 1 ? 1 : 0;
            }

            FaceCurrentDirection();
        }

        initializedOnRoute = true;
    }

    private void CacheOriginalScale()
    {
        if (visualRoot != null)
        {
            originalScale = visualRoot.localScale;
        }
    }

    private void FaceCurrentDirection()
    {
        if (route == null || route.Count == 0) return;

        Transform targetPoint = route.GetPoint(currentIndex);
        if (targetPoint == null) return;

        Vector3 dir = targetPoint.position - transform.position;
        UpdateFacing(dir, targetPoint);
    }

    private int FindNextRoutePointIndex(Vector3 position)
    {
        if (route == null || route.Count == 0)
            return 0;

        if (route.Count == 1)
            return 0;

        float bestDistance = Mathf.Infinity;
        int bestNextIndex = 1;

        for (int i = 0; i < route.Count; i++)
        {
            int nextIndex = (i + 1) % route.Count;

            Transform a = route.GetPoint(i);
            Transform b = route.GetPoint(nextIndex);

            if (a == null || b == null) continue;

            Vector3 closest = GetClosestPointOnSegment(position, a.position, b.position);
            float distance = Vector3.Distance(position, closest);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestNextIndex = nextIndex;
            }
        }

        return bestNextIndex;
    }

    private Vector3 GetClosestPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;

        if (ab.sqrMagnitude <= 0.0001f)
            return a;

        float t = Vector3.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);

        return a + ab * t;
    }

    void UpdateFacing(Vector3 currentDir, Transform currentTargetPoint)
    {
        if (visualRoot == null) return;

        Vector3 scale = originalScale;

        if (Mathf.Abs(currentDir.x) > 0.01f)
        {
            float direction = currentDir.x > 0f ? 1f : -1f;

            if (invertFacing)
                direction *= -1f;

            scale.x = Mathf.Abs(originalScale.x) * direction;
            visualRoot.localScale = scale;
            return;
        }

        if (route == null || route.Count == 0) return;

        int nextIndex = (currentIndex + 1) % route.Count;
        Transform nextPoint = route.GetPoint(nextIndex);

        if (nextPoint == null || currentTargetPoint == null) return;

        Vector3 nextDir = nextPoint.position - currentTargetPoint.position;

        if (Mathf.Abs(nextDir.x) > 0.01f)
        {
            float direction = nextDir.x > 0f ? 1f : -1f;

            if (invertFacing)
                direction *= -1f;

            scale.x = Mathf.Abs(originalScale.x) * direction;
            visualRoot.localScale = scale;
        }
    }

    public void TeleportToRouteStart()
    {
        if (route == null || route.Count == 0) return;

        Transform startPoint = route.GetPoint(0);
        if (startPoint == null) return;

        transform.position = startPoint.position;
        currentIndex = route.Count > 1 ? 1 : 0;

        FaceCurrentDirection();
    }

    public void ApplyFreeze(float duration)
    {
        if (duration <= 0f) return;

        isFrozen = true;
        freezeEndTime = Mathf.Max(freezeEndTime, Time.time + duration);
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (duration <= 0f) return;

        multiplier = Mathf.Clamp(multiplier, 0.1f, 1f);

        if (!isSlowed)
        {
            isSlowed = true;
            slowMultiplier = multiplier;
            slowEndTime = Time.time + duration;
            return;
        }

        slowMultiplier = Mathf.Min(slowMultiplier, multiplier);
        slowEndTime = Mathf.Max(slowEndTime, Time.time + duration);
    }

    void UpdateFreezeState()
    {
        if (isFrozen && Time.time >= freezeEndTime)
        {
            isFrozen = false;
        }
    }

    void UpdateSlowState()
    {
        if (isSlowed && Time.time >= slowEndTime)
        {
            isSlowed = false;
            slowMultiplier = 1f;
        }
    }
}