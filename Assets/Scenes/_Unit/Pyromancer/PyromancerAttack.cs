using System.Collections;
using UnityEngine;

public class PyromancerAttack : MonoBehaviour
{
    [Header("References")]
    public GameObject molotovPrefab;
    public Transform firePoint;
    public Transform visualRoot;
    public Animator animator;

    [Header("Facing")]
    public bool defaultFacesRight = true;   // 캐릭터 원본이 오른쪽을 보고 있으면 체크

    [Header("Target")]
    public LayerMask enemyLayer;
    public float attackRange = 4f;

    [Header("Stats")]
    public int directDamage = 10;
    public float attackCooldown = 2.4f;
    public float throwDelay = 0.25f;

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

        yield return new WaitForSeconds(throwDelay);

        if (molotovPrefab != null && firePoint != null && target != null)
        {
            GameObject bombObj = Instantiate(molotovPrefab, firePoint.position, Quaternion.identity);
            MolotovProjectile bomb = bombObj.GetComponent<MolotovProjectile>();

            if (bomb != null)
            {
                bomb.Initialize(target, directDamage);
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
            scale.x = targetIsRight ? -Mathf.Abs(originalScale.x) : Mathf.Abs(originalScale.x);
        }
        else
        {
            scale.x = targetIsRight ? Mathf.Abs(originalScale.x) : -Mathf.Abs(originalScale.x);
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
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}