using System.Collections.Generic;
using UnityEngine;

public class FireZone : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;

    [Header("Sprites")]
    public Sprite[] zoneSprites;

    [Header("Damage")]
    public LayerMask enemyLayer;
    public float damageRadius = 1.1f;
    public int tickDamage = 2;
    public float tickInterval = 0.5f;
    public float lifeTime = 3f;

    [Header("Animation")]
    public float frameRate = 10f;

    private float lifeTimer = 0f;
    private float tickTimer = 0f;
    private float animTimer = 0f;
    private int frameIndex = 0;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && zoneSprites != null && zoneSprites.Length > 0)
            spriteRenderer.sprite = zoneSprites[0];
    }

    void Update()
    {
        UpdateAnimation();
        UpdateDamage();
        UpdateLifetime();
    }

    void UpdateAnimation()
    {
        if (spriteRenderer == null || zoneSprites == null || zoneSprites.Length == 0)
            return;

        animTimer += Time.deltaTime;
        if (animTimer >= 1f / frameRate)
        {
            animTimer = 0f;
            frameIndex = (frameIndex + 1) % zoneSprites.Length;
            spriteRenderer.sprite = zoneSprites[frameIndex];
        }
    }

    void UpdateDamage()
    {
        tickTimer += Time.deltaTime;

        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            DamageEnemiesInArea();
        }
    }

    void DamageEnemiesInArea()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageRadius, enemyLayer);
        HashSet<EnemyHealth> hitSet = new HashSet<EnemyHealth>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            EnemyHealth hp = hit.GetComponent<EnemyHealth>();
            if (hp == null) hp = hit.GetComponentInParent<EnemyHealth>();
            if (hp == null) continue;
            if (hitSet.Contains(hp)) continue;

            hitSet.Add(hp);
            hp.TakeDamage(tickDamage);
        }
    }

    void UpdateLifetime()
    {
        lifeTimer += Time.deltaTime;

        if (lifeTimer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}