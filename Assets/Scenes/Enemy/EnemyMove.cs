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

    private int currentIndex = 0;
    private Vector3 originalScale;

    [Header("Freeze")]
    public bool isFrozen = false;
    private float freezeEndTime = 0f;

    [Header("Slow")]
    public bool isSlowed = false;
    private float slowEndTime = 0f;
    private float slowMultiplier = 1f;

    void Start()
    {
        if (visualRoot != null)
        {
            originalScale = visualRoot.localScale;
        }

        if (route != null && route.Count > 0)
        {
            transform.position = route.GetPoint(0).position;
            currentIndex = route.Count > 1 ? 1 : 0;

            if (route.Count > 1)
            {
                Transform firstTarget = route.GetPoint(currentIndex);
                if (firstTarget != null)
                {
                    Vector3 dir = firstTarget.position - transform.position;
                    UpdateFacing(dir, firstTarget);
                }
            }
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

    void UpdateFacing(Vector3 currentDir, Transform currentTargetPoint)
    {
        if (visualRoot == null) return;

        Vector3 scale = originalScale;

        if (Mathf.Abs(currentDir.x) > 0.01f)
        {
            if (currentDir.x > 0f)
                scale.x = Mathf.Abs(originalScale.x);
            else
                scale.x = -Mathf.Abs(originalScale.x);

            visualRoot.localScale = scale;
            return;
        }

        int nextIndex = (currentIndex + 1) % route.Count;
        Transform nextPoint = route.GetPoint(nextIndex);
        if (nextPoint == null) return;

        Vector3 nextDir = nextPoint.position - currentTargetPoint.position;

        if (Mathf.Abs(nextDir.x) > 0.01f)
        {
            if (nextDir.x > 0f)
                scale.x = Mathf.Abs(originalScale.x);
            else
                scale.x = -Mathf.Abs(originalScale.x);

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

        if (route.Count > 1)
        {
            Transform nextTarget = route.GetPoint(currentIndex);
            if (nextTarget != null)
            {
                Vector3 dir = nextTarget.position - transform.position;
                UpdateFacing(dir, nextTarget);
            }
        }
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