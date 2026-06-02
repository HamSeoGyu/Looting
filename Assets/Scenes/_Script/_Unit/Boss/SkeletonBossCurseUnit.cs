using UnityEngine;

public class SkeletonBossCurseUnit : MonoBehaviour
{
    [Header("Curse Skill")]
    public float curseRange = 4f;
    public float curseInterval = 4f;
    public float curseDuration = 3f;

    [Range(0f, 2f)]
    public float extraDamageTakenPercent = 0.2f;

    [Header("Curse Image Effect")]
    public Sprite curseEffectSprite;
    public float curseEffectHeightOffset = 1.25f;
    public float curseEffectWorldHeight = 0.85f;

    [Header("Debug")]
    public bool showDebugRange = true;

    private float lastCurseTime = -999f;

    private void Update()
    {
        if (Time.time < lastCurseTime + curseInterval)
            return;

        bool cursedAnyEnemy = ApplyCurseToEnemies();

        if (cursedAnyEnemy)
        {
            lastCurseTime = Time.time;
        }
    }

    private bool ApplyCurseToEnemies()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        bool cursedAnyEnemy = false;

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null) continue;
            if (enemy.IsDead) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance > curseRange)
                continue;

            EnemyCurseReceiver curseReceiver = enemy.GetComponent<EnemyCurseReceiver>();

            if (curseReceiver == null)
            {
                curseReceiver = enemy.gameObject.AddComponent<EnemyCurseReceiver>();
            }

            curseReceiver.curseEffectSprite = curseEffectSprite;
            curseReceiver.effectHeightOffset = curseEffectHeightOffset;
            curseReceiver.effectWorldHeight = curseEffectWorldHeight;

            curseReceiver.ApplyCurse(curseDuration, extraDamageTakenPercent);

            cursedAnyEnemy = true;
        }

        return cursedAnyEnemy;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugRange) return;

        Gizmos.color = new Color(0.6f, 0f, 1f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, curseRange);
    }
}