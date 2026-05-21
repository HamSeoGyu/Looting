using System.Collections.Generic;
using UnityEngine;

public class MolotovProjectile : MonoBehaviour
{
    private enum State
    {
        Flying,
        Exploding
    }

    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public GameObject fireZonePrefab;

    [Header("Sprites")]
    public Sprite flySprite;          // 비행 중에는 1장만 사용
    public Sprite[] explodeSprites;   // 5~8

    [Header("Target / Damage")]
    public Transform target;
    public int damage = 10;
    public LayerMask enemyLayer;

    [Header("Move")]
    public float moveSpeed = 7f;
    public float hitDistance = 0.15f;

    [Header("Explosion")]
    public float explosionRadius = 0.9f;

    [Header("Animation")]
    public float frameRate = 12f;

    [Header("Rendering")]
    public int flightOrderInLayer = 20;
    public int explosionOrderInLayer = 21;

    private State currentState = State.Flying;
    private float frameTimer = 0f;
    private int frameIndex = 0;
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
            spriteRenderer.sortingOrder = flightOrderInLayer;
            spriteRenderer.flipX = false;

            if (flySprite != null)
                spriteRenderer.sprite = flySprite;
        }
    }

    void Update()
    {
        if (!initialized) return;

        if (currentState == State.Flying)
            UpdateFlying();
        else
            UpdateExploding();
    }

    void UpdateFlying()
    {
        if (target == null)
        {
            StartExplosion(transform.position);
            return;
        }

        Vector3 dir = target.position - transform.position;
        dir.z = 0f;

        if (dir.magnitude <= hitDistance)
        {
            StartExplosion(target.position);
            return;
        }

        transform.position += dir.normalized * moveSpeed * Time.deltaTime;

        // 회전하지 않고 좌우만 반전
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = flySprite;
            spriteRenderer.flipX = dir.x < 0f;
            spriteRenderer.enabled = true;
            spriteRenderer.sortingOrder = flightOrderInLayer;
        }
    }

    void StartExplosion(Vector3 pos)
    {
        if (currentState == State.Exploding) return;

        currentState = State.Exploding;
        impactPosition = pos;
        transform.position = pos;

        ApplyExplosionDamage();

        frameTimer = 0f;
        frameIndex = 0;

        // 폭발 시작할 때는 반전 해제
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
            spriteRenderer.sortingOrder = explosionOrderInLayer;

            if (explodeSprites != null && explodeSprites.Length > 0)
                spriteRenderer.sprite = explodeSprites[0];
        }
    }

    void UpdateExploding()
    {
        bool finished = AnimateExplodeSprites();

        if (finished)
        {
            SpawnFireZone();
            Destroy(gameObject);
        }
    }

    bool AnimateExplodeSprites()
    {
        if (spriteRenderer == null || explodeSprites == null || explodeSprites.Length == 0)
            return true;

        frameTimer += Time.deltaTime;
        if (frameTimer < 1f / frameRate)
            return false;

        frameTimer = 0f;
        frameIndex++;

        if (frameIndex >= explodeSprites.Length)
        {
            frameIndex = explodeSprites.Length - 1;
            spriteRenderer.sprite = explodeSprites[frameIndex];
            return true;
        }

        spriteRenderer.sprite = explodeSprites[frameIndex];
        spriteRenderer.enabled = true;
        return false;
    }

    void ApplyExplosionDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(impactPosition, explosionRadius, enemyLayer);
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

    void SpawnFireZone()
    {
        if (fireZonePrefab == null) return;
        Instantiate(fireZonePrefab, impactPosition, Quaternion.identity);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}