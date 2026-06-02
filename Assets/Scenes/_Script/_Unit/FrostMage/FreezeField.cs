using System.Collections.Generic;
using UnityEngine;

public class FreezeField : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;

    [Header("Sprites")]
    public Sprite[] fieldSprites;   // 1~4

    [Header("Damage / Freeze")]
    public LayerMask enemyLayer;
    public float fieldRadius = 1.0f;
    public int tickDamage = 1;
    public float tickInterval = 0.5f;

    [Range(0f, 100f)]
    public float freezeChancePercent = 15f;
    public float freezeDuration = 1.0f;

    [Header("Boss Option")]
    public bool bossImmuneToFreeze = true;
    public string bossTag = "Boss";

    [Header("Lifetime")]
    public float lifeTime = 2.4f;

    [Header("Animation")]
    public float frameRate = 10f;

    [Header("Freeze Roll Cooldown")]
    public float freezeRollCooldown = 0.6f;

    private float lifeTimer = 0f;
    private float tickTimer = 0f;
    private float animTimer = 0f;
    private int frameIndex = 0;

    // 마지막 프레임 고정용
    private bool animationFinished = false;

    private Dictionary<int, float> nextFreezeRollTime = new Dictionary<int, float>();

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && fieldSprites != null && fieldSprites.Length > 0)
        {
            frameIndex = 0;
            spriteRenderer.sprite = fieldSprites[0];
        }
    }

    void Update()
    {
        UpdateAnimation();
        UpdateDamageAndFreeze();
        UpdateLifetime();
    }

    void UpdateAnimation()
    {
        if (animationFinished) return;
        if (spriteRenderer == null || fieldSprites == null || fieldSprites.Length == 0)
            return;

        animTimer += Time.deltaTime;

        if (animTimer >= 1f / frameRate)
        {
            animTimer = 0f;
            frameIndex++;

            // 마지막 프레임에 도달하면 그 상태로 멈춤
            if (frameIndex >= fieldSprites.Length)
            {
                frameIndex = fieldSprites.Length - 1;
                animationFinished = true;
            }

            spriteRenderer.sprite = fieldSprites[frameIndex];
        }
    }

    void UpdateDamageAndFreeze()
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

            // 낮은 지속 피해
            hp.TakeDamage(tickDamage);

            // 낮은 확률 빙결
            TryFreezeEnemy(hp.gameObject);
        }
    }

    void TryFreezeEnemy(GameObject enemyObject)
    {
        if (enemyObject == null) return;

        if (bossImmuneToFreeze && IsBoss(enemyObject))
            return;

        EnemyMove move = enemyObject.GetComponent<EnemyMove>();
        if (move == null) move = enemyObject.GetComponentInParent<EnemyMove>();
        if (move == null) return;

        int id = move.gameObject.GetInstanceID();

        if (nextFreezeRollTime.ContainsKey(id) && Time.time < nextFreezeRollTime[id])
            return;

        nextFreezeRollTime[id] = Time.time + freezeRollCooldown;

        float roll = Random.Range(0f, 100f);
        if (roll <= freezeChancePercent)
        {
            move.ApplyFreeze(freezeDuration);
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
        Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, fieldRadius);
    }
}