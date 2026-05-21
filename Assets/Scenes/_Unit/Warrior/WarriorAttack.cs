using UnityEngine;

public class WarriorAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 2.0f;
    public int damage = 3;
    public float attackCooldown = 1.0f;

    private float lastAttackTime = -999f;
    private Animator animator;
    private UnitDrag unitDrag;

    void Start()
    {
        animator = GetComponentInChildren<Animator>(true);
        unitDrag = GetComponent<UnitDrag>();
    }

    void Update()
    {
        if (unitDrag != null && unitDrag.IsDragging) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        bool hasEnemyInRange = false;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance <= attackRange)
            {
                hasEnemyInRange = true;
                break;
            }
        }

        if (!hasEnemyInRange) return;

        Attack();
    }

    void Attack()
    {
        lastAttackTime = Time.time;

        if (animator != null)
        {
            animator.ResetTrigger("AttackTrigger");
            animator.SetTrigger("AttackTrigger");
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance <= attackRange)
            {
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth == null)
                    enemyHealth = enemy.GetComponentInChildren<EnemyHealth>();
                if (enemyHealth == null)
                    enemyHealth = enemy.GetComponentInParent<EnemyHealth>();

                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}