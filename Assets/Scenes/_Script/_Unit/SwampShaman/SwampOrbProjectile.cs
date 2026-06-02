using System.Collections.Generic;
using UnityEngine;

public class SwampOrbProjectile : MonoBehaviour
{
    private enum State
    {
        Flying,
        Impact
    }

    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public GameObject swampFieldPrefab;

    [Header("Sprites")]
    public Sprite projectileSprite;   // 첫 번째 이미지 1장만 사용

    [Header("Target / Damage")]
    public Transform target;
    public int damage = 3;
    public LayerMask enemyLayer;

    [Header("Move")]
    public float moveSpeed = 5.5f;
    public float hitDistance = 0.15f;

    [Header("Explosion")]
    public float hitRadius = 0.55f;

    [Header("Rendering")]
    public int sortingOrder = 20;

    private State currentState = State.Flying;
    private Vector3 impactPosition;
    private bool initialized = false;

    public void Init(Transform newTarget, int newDamage)
    {
        target = newTarget;
        damage = newDamage;
        initialized = true;
    }

    public void Initialize(Transform newTarget, int newDamage)
    {
        Init(newTarget, newDamage);
    }

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = Color.white;
            spriteRenderer.sortingOrder = sortingOrder;

            if (projectileSprite != null)
                spriteRenderer.sprite = projectileSprite;
        }
    }

    void Update()
    {
        if (!initialized) return;
        if (currentState != State.Flying) return;

        UpdateFlying();
    }

    void UpdateFlying()
    {
        if (target == null)
        {
            Impact(transform.position);
            return;
        }

        Vector3 dir = target.position - transform.position;
        dir.z = 0f;

        if (dir.magnitude <= hitDistance)
        {
            Impact(target.position);
            return;
        }

        transform.position += dir.normalized * moveSpeed * Time.deltaTime;
    }

    void Impact(Vector3 pos)
    {
        currentState = State.Impact;
        impactPosition = pos;
        transform.position = pos;

        ApplyHitDamage();
        SpawnSwampField();

        Destroy(gameObject);
    }

    void ApplyHitDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(impactPosition, hitRadius, enemyLayer);
        HashSet<EnemyHealth> hitSet = new HashSet<EnemyHealth>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            EnemyHealth hp = hit.GetComponent<EnemyHealth>();
            if (hp == null) hp = hit.GetComponentInParent<EnemyHealth>();
            if (hp == null) continue;
            if (hitSet.Contains(hp)) continue;

            hitSet.Add(hp);
            hp.TakeDamage(damage);
        }
    }

    void SpawnSwampField()
    {
        if (swampFieldPrefab == null) return;

        Vector3 pos = impactPosition;
        pos.z = 0f;
        Instantiate(swampFieldPrefab, pos, Quaternion.identity);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}