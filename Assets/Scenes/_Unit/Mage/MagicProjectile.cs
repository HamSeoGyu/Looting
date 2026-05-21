using System.Collections.Generic;
using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    [Header("Target / Damage")]
    public Transform target;
    public int damage = 3;

    [Header("Move")]
    public float moveSpeed = 7f;
    public float hitDistance = 0.15f;

    [Header("Explosion")]
    public float splashRadius = 0.8f;

    [Header("Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] flySprites;
    public Sprite[] impactSprites;
    public float frameRate = 12f;

    [Header("Teleport Effect")]
    [Range(0f, 100f)]
    public float teleportChancePercent = 15f;
    public bool bossImmuneToTeleport = true;
    public string bossTag = "Boss";

    private bool hasImpacted = false;
    private Vector3 impactPosition;
    private float frameTimer = 0f;
    private int frameIndex = 0;

    public void Init(Transform newTarget, int newDamage)
    {
        target = newTarget;
        damage = newDamage;
    }

    // 예전 MageAttack.cs와 호환용
    public void Initialize(Transform newTarget, int newDamage)
    {
        Init(newTarget, newDamage);
    }

    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        if (hasImpacted)
        {
            PlayImpactAnimation();
            return;
        }

        FlyTowardTarget();
    }

    void FlyTowardTarget()
    {
        if (target == null)
        {
            ImpactAt(transform.position);
            return;
        }

        Vector3 dir = target.position - transform.position;

        if (dir.magnitude <= hitDistance)
        {
            ImpactAt(target.position);
            return;
        }

        transform.position += dir.normalized * moveSpeed * Time.deltaTime;

        UpdateFacing(dir);
        PlayFlyAnimation();
    }

    void UpdateFacing(Vector3 dir)
    {
        if (spriteRenderer == null) return;

        if (Mathf.Abs(dir.x) > 0.01f)
        {
            spriteRenderer.flipX = dir.x < 0f;
        }
    }

    void PlayFlyAnimation()
    {
        if (spriteRenderer == null) return;
        if (flySprites == null || flySprites.Length == 0) return;

        frameTimer += Time.deltaTime;
        if (frameTimer >= 1f / frameRate)
        {
            frameTimer = 0f;
            frameIndex = (frameIndex + 1) % flySprites.Length;
            spriteRenderer.sprite = flySprites[frameIndex];
        }
    }

    void ImpactAt(Vector3 worldPos)
    {
        if (hasImpacted) return;

        hasImpacted = true;
        impactPosition = worldPos;
        transform.position = worldPos;

        ApplyExplosionDamageAndEffects();

        frameTimer = 0f;
        frameIndex = 0;

        if (impactSprites != null && impactSprites.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = impactSprites[0];
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void ApplyExplosionDamageAndEffects()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(impactPosition, splashRadius);
        HashSet<EnemyHealth> damagedTargets = new HashSet<EnemyHealth>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            EnemyHealth hp = hit.GetComponent<EnemyHealth>();
            if (hp == null) hp = hit.GetComponentInParent<EnemyHealth>();

            if (hp == null) continue;
            if (damagedTargets.Contains(hp)) continue;

            damagedTargets.Add(hp);

            hp.TakeDamage(damage);
            TryTeleportEnemy(hp.gameObject);
        }
    }

    void TryTeleportEnemy(GameObject enemyObject)
    {
        if (enemyObject == null) return;

        if (bossImmuneToTeleport && IsBoss(enemyObject))
            return;

        float roll = Random.Range(0f, 100f);
        if (roll > teleportChancePercent)
            return;

        EnemyMove move = enemyObject.GetComponent<EnemyMove>();
        if (move == null) move = enemyObject.GetComponentInParent<EnemyMove>();

        if (move != null)
        {
            move.TeleportToRouteStart();
        }
    }

    bool IsBoss(GameObject obj)
    {
        if (obj == null) return false;

        if (obj.CompareTag(bossTag)) return true;

        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            if (parent.CompareTag(bossTag)) return true;
            parent = parent.parent;
        }

        return false;
    }

    void PlayImpactAnimation()
    {
        if (spriteRenderer == null || impactSprites == null || impactSprites.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        frameTimer += Time.deltaTime;
        if (frameTimer >= 1f / frameRate)
        {
            frameTimer = 0f;
            frameIndex++;

            if (frameIndex >= impactSprites.Length)
            {
                Destroy(gameObject);
                return;
            }

            spriteRenderer.sprite = impactSprites[frameIndex];
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}