using System.Collections;
using UnityEngine;

public class FrostMageAttack : MonoBehaviour
{
    [Header("References")]
    public GameObject iceProjectilePrefab;
    public Transform firePoint;
    public Transform visualRoot;
    public Animator animator;

    [Header("Facing")]
    public bool defaultFacesRight = false;
    // 지금 반대로 돈다고 하셨으니 기본값을 false로 둡니다.
    // 만약 또 반대면 Inspector에서 이것만 체크/해제 바꾸면 됩니다.

    [Header("Target")]
    public LayerMask enemyLayer;
    public float attackRange = 4.2f;

    [Header("Stats")]
    public int directDamage = 6;
    public float attackCooldown = 1.6f;
    public float castDelay = 0.2f;

    [Header("Animation")]
    public string attackTriggerName = "AttackTrigger";

    private float nextAttackTime = 0f;
    private bool isAttacking = false;
    private Vector3 originalScale;

    void Awake()
    {
        AutoFindReferences();

        if (visualRoot != null)
            originalScale = visualRoot.localScale;
    }

    void AutoFindReferences()
    {
        if (firePoint == null)
        {
            Transform foundFirePoint = FindDeepChild(transform, "FirePoint");
            if (foundFirePoint != null)
                firePoint = foundFirePoint;
        }

        if (visualRoot == null)
        {
            Transform unitRoot = FindDeepChild(transform, "UnitRoot");
            if (unitRoot != null)
                visualRoot = unitRoot;
            else
                visualRoot = transform;
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);
        }
    }

    void Update()
    {
        if (isAttacking) return;
        if (Time.time < nextAttackTime) return;

        Transform target = FindNearestTarget();
        if (target != null)
        {
            StartCoroutine(AttackRoutine(target));
        }
    }

    IEnumerator AttackRoutine(Transform target)
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        FaceTargetCorrectly(target);

        if (animator != null && !string.IsNullOrEmpty(attackTriggerName))
        {
            animator.SetTrigger(attackTriggerName);
        }

        yield return new WaitForSeconds(castDelay);

        if (iceProjectilePrefab != null && firePoint != null && target != null)
        {
            GameObject projectileObj = Instantiate(iceProjectilePrefab, firePoint.position, Quaternion.identity);
            IceProjectile projectile = projectileObj.GetComponent<IceProjectile>();

            if (projectile != null)
            {
                projectile.Initialize(target, directDamage);
            }
        }

        yield return new WaitForSeconds(0.05f);
        isAttacking = false;
    }

    Transform FindNearestTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        float bestDist = float.MaxValue;
        Transform bestTarget = null;

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            EnemyHealth hp = hit.GetComponent<EnemyHealth>();
            if (hp == null) hp = hit.GetComponentInParent<EnemyHealth>();
            if (hp == null) continue;

            float dist = (hp.transform.position - transform.position).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                bestTarget = hp.transform;
            }
        }

        return bestTarget;
    }

    void FaceTargetCorrectly(Transform target)
    {
        if (visualRoot == null || target == null) return;

        bool targetIsRight = target.position.x >= transform.position.x;
        Vector3 scale = originalScale;

        if (defaultFacesRight)
        {
            scale.x = targetIsRight ? Mathf.Abs(originalScale.x) : -Mathf.Abs(originalScale.x);
        }
        else
        {
            scale.x = targetIsRight ? -Mathf.Abs(originalScale.x) : Mathf.Abs(originalScale.x);
        }

        visualRoot.localScale = scale;
    }

    Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform found = FindDeepChild(child, childName);
            if (found != null)
                return found;
        }

        return null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}