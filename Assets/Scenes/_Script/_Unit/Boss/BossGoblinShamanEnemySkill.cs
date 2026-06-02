using UnityEngine;

public class BossGoblinShamanEnemySkill : MonoBehaviour
{
    [Header("Frenzy Shaman Skill")]
    public float frenzyRange = 4.5f;
    public float frenzyInterval = 6f;
    public float frenzyDuration = 4f;

    [Header("Frenzy Effect Value")]
    public float speedMultiplier = 1.2f;

    [Range(0f, 0.9f)]
    public float damageReduction = 0.15f;

    [Header("Target Option")]
    public bool excludeSelf = true;
    public bool onlyEnemyTag = true;

    [Header("Debug")]
    public bool showDebugRange = true;

    private float lastFrenzyTime = -999f;

    private void Update()
    {
        if (Time.time < lastFrenzyTime + frenzyInterval)
            return;

        bool applied = ApplyFrenzyToNearbyEnemies();

        if (applied)
        {
            lastFrenzyTime = Time.time;
        }
    }

    private bool ApplyFrenzyToNearbyEnemies()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        bool appliedAny = false;

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null) continue;
            if (enemy.IsDead) continue;

            if (excludeSelf && enemy.gameObject == gameObject)
                continue;

            if (onlyEnemyTag && !enemy.CompareTag("Enemy"))
                continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance > frenzyRange)
                continue;

            EnemyGoblinFrenzyReceiver receiver = enemy.GetComponent<EnemyGoblinFrenzyReceiver>();

            if (receiver == null)
            {
                receiver = enemy.gameObject.AddComponent<EnemyGoblinFrenzyReceiver>();
            }

            receiver.ApplyFrenzy(frenzyDuration, speedMultiplier, damageReduction);
            appliedAny = true;
        }

        return appliedAny;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugRange) return;

        Gizmos.color = new Color(1f, 0.25f, 0f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, frenzyRange);
    }
}