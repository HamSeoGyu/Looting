using System.Collections.Generic;
using UnityEngine;

public class SwampField : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;

    [Header("Visual")]
    public Sprite fieldSprite;   // 두 번째 늪 장판 이미지 1장

    [Header("Damage / Slow")]
    public LayerMask enemyLayer;
    public float fieldRadius = 1.15f;
    public int tickDamage = 1;
    public float tickInterval = 0.5f;

    [Range(0f, 100f)]
    public float slowPercent = 35f;
    public float slowDuration = 0.6f;

    [Header("Lifetime")]
    public float lifeTime = 3.0f;

    [Header("Boss Option")]
    public bool bossImmuneToSlow = false;
    public string bossTag = "Boss";

    private float lifeTimer = 0f;
    private float tickTimer = 0f;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && fieldSprite != null)
            spriteRenderer.sprite = fieldSprite;
    }

    void Update()
    {
        UpdateDamageAndSlow();
        UpdateLifetime();
    }

    void UpdateDamageAndSlow()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer < tickInterval) return;
        tickTimer = 0f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, fieldRadius, enemyLayer);
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
            ApplySlowToEnemy(hp.gameObject);
        }
    }

    void ApplySlowToEnemy(GameObject enemyObject)
    {
        if (enemyObject == null) return;

        if (bossImmuneToSlow && IsBoss(enemyObject))
            return;

        EnemyMove move = enemyObject.GetComponent<EnemyMove>();
        if (move == null) move = enemyObject.GetComponentInParent<EnemyMove>();
        if (move == null) return;

        float multiplier = 1f - (slowPercent / 100f);
        move.ApplySlow(multiplier, slowDuration);
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
        Gizmos.color = new Color(0.2f, 0.7f, 0.2f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, fieldRadius);
    }
}