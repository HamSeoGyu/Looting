using UnityEngine;
using System.Collections;

public class ArcherAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 4.5f;
    public int damage = 2;
    public float attackCooldown = 1.0f;
    public float fireDelay = 0.18f;

    [Header("Arrow")]
    public GameObject arrowPrefab;
    public Transform firePoint;

    [Header("Facing")]
    public Transform visualRoot;   // 보이는 캐릭터 루트 (보통 UnitRoot)

    private float lastAttackTime = -999f;
    private Animator animator;
    private UnitDrag unitDrag;
    private bool isAttacking = false;

    private Vector3 originalVisualScale;

    void Start()
    {
        animator = GetComponentInChildren<Animator>(true);
        unitDrag = GetComponent<UnitDrag>();

        if (visualRoot != null)
        {
            originalVisualScale = visualRoot.localScale;
        }

        Debug.Log(gameObject.name + " : ArcherAttack 시작");
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

        FaceTarget(target.transform);

        if (animator != null)
        {
            animator.ResetTrigger("AttackTrigger");
            animator.SetTrigger("AttackTrigger");
        }

        yield return new WaitForSeconds(fireDelay);

        if (target != null)
        {
            Debug.Log(gameObject.name + " : FireArrow 호출");
            FireArrow(target.transform);
        }
        else
        {
            Debug.Log(gameObject.name + " : fireDelay 후 target이 null");
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

    void FireArrow(Transform target)
    {
        if (arrowPrefab == null)
        {
            Debug.LogError(gameObject.name + " : arrowPrefab이 연결되지 않았습니다.");
            return;
        }

        Vector3 spawnPos = transform.position;
        if (firePoint != null)
        {
            spawnPos = firePoint.position;
        }
        else
        {
            Debug.LogWarning(gameObject.name + " : firePoint가 없어 transform.position에서 발사합니다.");
        }

        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);
        Debug.Log(gameObject.name + " : 화살 생성 완료 -> " + arrow.name);

        ArrowProjectile projectile = arrow.GetComponent<ArrowProjectile>();
        if (projectile == null)
        {
            Debug.LogError(gameObject.name + " : ArrowProjectile 컴포넌트가 없습니다.");
            return;
        }

        projectile.Initialize(target, damage);
    }

    void FaceTarget(Transform target)
    {
        if (visualRoot == null || target == null) return;

        float dx = target.position.x - transform.position.x;

        // 거의 정면이면 방향 유지
        if (Mathf.Abs(dx) < 0.05f) return;

        Vector3 scale = originalVisualScale;

        // 기본 스프라이트 기준
        // 오른쪽을 볼 때 정상이라면 아래 그대로 사용
        if (dx > 0)
        {
            scale.x = -Mathf.Abs(originalVisualScale.x);
        }
        else
        {
            scale.x = Mathf.Abs(originalVisualScale.x);
        }

        visualRoot.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}