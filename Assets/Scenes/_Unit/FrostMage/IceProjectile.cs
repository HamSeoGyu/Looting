using System.Collections.Generic;
using UnityEngine;

public class IceProjectile : MonoBehaviour
{
    private enum State
    {
        Flying,
        Exploding
    }

    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public GameObject freezeFieldPrefab;

    [Header("Sprites")]
    public Sprite[] projectileSprites;   // 얼음 탄 4장 넣기
    public Sprite[] impactSprites;       // 마지막 폭발 4장 넣기

    [Header("Projectile Animation")]
    public bool animateProjectile = false;
    public float projectileFrameRate = 12f;

    [Header("Target / Damage")]
    public Transform target;
    public int damage = 6;
    public LayerMask enemyLayer;

    [Header("Move")]
    public float moveSpeed = 7f;
    public float hitDistance = 0.15f;

    [Header("Explosion")]
    public float explosionRadius = 0.7f;

    [Header("Impact Animation")]
    public float impactFrameRate = 12f;

    [Header("Direction")]
    public bool rotateToMoveDirection = true;
    public float spriteForwardAngleOffset = 0f;

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
            spriteRenderer.color = Color.white;
            spriteRenderer.flipX = false;
            spriteRenderer.sortingOrder = flightOrderInLayer;

            if (projectileSprites != null && projectileSprites.Length > 0)
            {
                spriteRenderer.sprite = projectileSprites[0];
            }
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
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = Color.white;
            spriteRenderer.sortingOrder = flightOrderInLayer;

            if (projectileSprites != null && projectileSprites.Length > 0 && spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = projectileSprites[0];
            }
        }

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

        if (rotateToMoveDirection)
        {
            RotateToDirection(dir);
        }

        UpdateProjectileVisual();
    }

    void UpdateProjectileVisual()
    {
        if (spriteRenderer == null) return;
        if (projectileSprites == null || projectileSprites.Length == 0) return;

        if (!animateProjectile)
        {
            spriteRenderer.sprite = projectileSprites[0];
            return;
        }

        frameTimer += Time.deltaTime;
        if (frameTimer >= 1f / projectileFrameRate)
        {
            frameTimer = 0f;
            frameIndex = (frameIndex + 1) % projectileSprites.Length;
            spriteRenderer.sprite = projectileSprites[frameIndex];
        }
    }

    void RotateToDirection(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + spriteForwardAngleOffset);
    }

    void StartExplosion(Vector3 pos)
    {
        if (currentState == State.Exploding) return;

        currentState = State.Exploding;
        impactPosition = pos;
        transform.position = pos;
        transform.rotation = Quaternion.identity;

        ApplyExplosionDamage();

        frameTimer = 0f;
        frameIndex = 0;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = Color.white;
            spriteRenderer.flipX = false;
            spriteRenderer.sortingOrder = explosionOrderInLayer;

            if (impactSprites != null && impactSprites.Length > 0)
            {
                spriteRenderer.sprite = impactSprites[0];
            }
        }
    }

    void UpdateExploding()
    {
        bool finished = AnimateImpactSprites();

        if (finished)
        {
            SpawnFreezeField();
            Destroy(gameObject);
        }
    }

    bool AnimateImpactSprites()
    {
        if (spriteRenderer == null || impactSprites == null || impactSprites.Length == 0)
            return true;

        frameTimer += Time.deltaTime;
        if (frameTimer < 1f / impactFrameRate)
            return false;

        frameTimer = 0f;
        frameIndex++;

        if (frameIndex >= impactSprites.Length)
        {
            frameIndex = impactSprites.Length - 1;
            spriteRenderer.sprite = impactSprites[frameIndex];
            return true;
        }

        spriteRenderer.sprite = impactSprites[frameIndex];
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

    void SpawnFreezeField()
    {
        if (freezeFieldPrefab == null) return;
        Instantiate(freezeFieldPrefab, impactPosition, Quaternion.identity);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}