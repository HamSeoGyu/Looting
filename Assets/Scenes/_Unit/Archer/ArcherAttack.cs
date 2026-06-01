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
    public Transform visualRoot;   // ���̴� ĳ���� ��Ʈ (���� UnitRoot)

    private float lastAttackTime = -999f;
    private Animator animator;
    private UnitDrag unitDrag;
    private bool isAttacking = false;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip attackSound;

    private Vector3 originalVisualScale;

    void Start()
    {
        animator = GetComponentInChildren<Animator>(true);
        unitDrag = GetComponent<UnitDrag>();

        if (visualRoot != null)
        {
            originalVisualScale = visualRoot.localScale;
        }

        if (audioSource == null)
        {
        audioSource = GetComponent<AudioSource>();
        }

        Debug.Log(gameObject.name + " : ArcherAttack ����");
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
            Debug.Log(gameObject.name + " : FireArrow ȣ��");
            FireArrow(target.transform);
        }
        else
        {
            Debug.Log(gameObject.name + " : fireDelay �� target�� null");
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
            Debug.LogError(gameObject.name + " : arrowPrefab�� ������� �ʾҽ��ϴ�.");
            return;
        }

        Vector3 spawnPos = transform.position;
        if (firePoint != null)
        {
            spawnPos = firePoint.position;
        }
        else
        {
            Debug.LogWarning(gameObject.name + " : firePoint�� ���� transform.position���� �߻��մϴ�.");
        }

        if (audioSource != null && attackSound != null)
        {
        audioSource.PlayOneShot(
            attackSound,
            PlayerPrefs.GetFloat("SFXVolume", 1f)
        );
        } 

        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);
        Debug.Log(gameObject.name + " : ȭ�� ���� �Ϸ� -> " + arrow.name);

        ArrowProjectile projectile = arrow.GetComponent<ArrowProjectile>();
        if (projectile == null)
        {
            Debug.LogError(gameObject.name + " : ArrowProjectile ������Ʈ�� �����ϴ�.");
            return;
        }

        projectile.Initialize(target, damage);
    }

    void FaceTarget(Transform target)
    {
        if (visualRoot == null || target == null) return;

        float dx = target.position.x - transform.position.x;

        // ���� �����̸� ���� ����
        if (Mathf.Abs(dx) < 0.05f) return;

        Vector3 scale = originalVisualScale;

        // �⺻ ��������Ʈ ����
        // �������� �� �� �����̶�� �Ʒ� �״�� ���
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