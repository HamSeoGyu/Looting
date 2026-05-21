using UnityEngine;
using System.Collections;

public class MageAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 4.5f;
    public int damage = 3;
    public float attackCooldown = 1.2f;
    public float castDelay = 0.2f;

    [Header("Projectile")]
    public GameObject magicProjectilePrefab;
    public Transform firePoint;

    [Header("Facing")]
    public Transform visualRoot;   // 보통 UnitRoot

    private float lastAttackTime = -999f;
    private Animator animator;
    private UnitDrag unitDrag;
    private bool isAttacking = false;
    private Vector3 originalScale;

    void Start()
    {
        animator = GetComponentInChildren<Animator>(true);
        unitDrag = GetComponent<UnitDrag>();

        if (visualRoot != null)
            originalScale = visualRoot.localScale;
    }

    void Update()
    {
        if (unitDrag != null && unitDrag.IsDragging) return;
        if (isAttacking) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        GameObject target = FindNearestEnemyInRange();
        if (target == null) return;

        StartCoroutine(AttackRoutine(target));
    }

    IEnumerator AttackRoutine(GameObject target)
    {
        if (target == null) yield break;

        isAttacking = true;
        lastAttackTime = Time.time;

        FaceTargetReverse(target.transform);

        if (animator != null)
        {
            animator.ResetTrigger("AttackTrigger");
            animator.SetTrigger("AttackTrigger");
        }

        yield return new WaitForSeconds(castDelay);

        if (target != null)
        {
            FireMagic(target.transform);
        }

        isAttacking = false;
    }

    GameObject FindNearestEnemyInRange()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float closestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance <= attackRange && distance < closestDistance)
            {
                closestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    void FireMagic(Transform target)
    {
        if (magicProjectilePrefab == null)
        {
            Debug.LogError("MageAttack: magicProjectilePrefab이 연결되지 않았습니다.");
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        spawnPos.z = 0f;

        GameObject proj = Instantiate(magicProjectilePrefab, spawnPos, Quaternion.identity);
        Debug.Log("마법 구체 생성 완료: " + proj.name);

        MagicProjectile mp = proj.GetComponent<MagicProjectile>();
        if (mp != null)
        {
            mp.Initialize(target, damage);
        }
        else
        {
            Debug.LogError("MageAttack: MagicProjectile 컴포넌트가 없습니다.");
        }
    }

    // 적이 왼쪽에 있어도 오른쪽을 보고, 적이 오른쪽에 있어도 왼쪽을 보게
    void FaceTargetReverse(Transform target)
    {
        if (visualRoot == null || target == null) return;

        float dx = target.position.x - transform.position.x;
        if (Mathf.Abs(dx) < 0.05f) return;

        Vector3 scale = originalScale;

        if (dx > 0)
            scale.x = -Mathf.Abs(originalScale.x);
        else
            scale.x = Mathf.Abs(originalScale.x);

        visualRoot.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}