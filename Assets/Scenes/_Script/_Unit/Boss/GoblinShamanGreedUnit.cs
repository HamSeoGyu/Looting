using System.Collections;
using UnityEngine;

public class GoblinShamanGreedUnit : MonoBehaviour
{
    [Header("Greed Skill")]
    public float greedRange = 4f;
    public float greedInterval = 5f;
    public float greedDuration = 4f;
    public int bonusGold = 5;

    [Header("Greed Mark Image")]
    public Sprite greedEffectSprite;
    public float greedEffectHeightOffset = 1.1f;
    public float greedEffectWorldHeight = 0.55f;

    [Header("Motion")]
    public Transform visualRoot;
    public float idleFloatSpeed = 2.2f;
    public float idleFloatAmount = 0.06f;
    public float idleRotateAmount = 2.5f;

    public float castMotionDuration = 0.35f;
    public float castScaleMultiplier = 1.12f;
    public float castRiseAmount = 0.08f;

    [Header("Debug")]
    public bool showDebugRange = true;

    private float lastGreedTime = -999f;

    private Vector3 visualRootStartLocalPos;
    private Vector3 visualRootStartLocalScale;
    private float visualRootStartLocalRotZ;

    private Coroutine castMotionCoroutine;

    private void Awake()
    {
        if (visualRoot == null)
            visualRoot = transform;

        visualRootStartLocalPos = visualRoot.localPosition;
        visualRootStartLocalScale = visualRoot.localScale;
        visualRootStartLocalRotZ = visualRoot.localEulerAngles.z;
    }

    private void Update()
    {
        UpdateIdleMotion();

        if (Time.time < lastGreedTime + greedInterval)
            return;

        bool markedAnyEnemy = ApplyGreedToEnemies();

        if (markedAnyEnemy)
        {
            lastGreedTime = Time.time;

            if (castMotionCoroutine != null)
                StopCoroutine(castMotionCoroutine);

            castMotionCoroutine = StartCoroutine(CastMotionRoutine());
        }
    }

    private void UpdateIdleMotion()
    {
        if (visualRoot == null) return;

        float t = Time.time;

        Vector3 pos = visualRootStartLocalPos;
        pos.y += Mathf.Sin(t * idleFloatSpeed) * idleFloatAmount;

        float rotZ = visualRootStartLocalRotZ + Mathf.Sin(t * idleFloatSpeed * 0.85f) * idleRotateAmount;

        visualRoot.localPosition = pos;
        visualRoot.localRotation = Quaternion.Euler(0f, 0f, rotZ);
    }

    private bool ApplyGreedToEnemies()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        bool markedAnyEnemy = false;

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null) continue;
            if (enemy.IsDead) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance > greedRange) continue;

            EnemyGreedReceiver greedReceiver = enemy.GetComponent<EnemyGreedReceiver>();
            if (greedReceiver == null)
            {
                greedReceiver = enemy.gameObject.AddComponent<EnemyGreedReceiver>();
            }

            greedReceiver.greedEffectSprite = greedEffectSprite;
            greedReceiver.effectHeightOffset = greedEffectHeightOffset;
            greedReceiver.effectWorldHeight = greedEffectWorldHeight;

            greedReceiver.ApplyGreed(greedDuration, bonusGold);

            markedAnyEnemy = true;
        }

        return markedAnyEnemy;
    }

    private IEnumerator CastMotionRoutine()
    {
        if (visualRoot == null) yield break;

        Vector3 startPos = visualRoot.localPosition;
        Vector3 startScale = visualRoot.localScale;
        float half = castMotionDuration * 0.5f;

        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / half);

            visualRoot.localPosition = Vector3.Lerp(
                startPos,
                visualRootStartLocalPos + new Vector3(0f, castRiseAmount, 0f),
                p
            );

            visualRoot.localScale = Vector3.Lerp(
                startScale,
                visualRootStartLocalScale * castScaleMultiplier,
                p
            );

            yield return null;
        }

        t = 0f;
        Vector3 peakPos = visualRoot.localPosition;
        Vector3 peakScale = visualRoot.localScale;

        while (t < half)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / half);

            visualRoot.localPosition = Vector3.Lerp(
                peakPos,
                visualRootStartLocalPos,
                p
            );

            visualRoot.localScale = Vector3.Lerp(
                peakScale,
                visualRootStartLocalScale,
                p
            );

            yield return null;
        }

        visualRoot.localPosition = visualRootStartLocalPos;
        visualRoot.localScale = visualRootStartLocalScale;
        castMotionCoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugRange) return;

        Gizmos.color = new Color(1f, 0.85f, 0.15f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, greedRange);
    }
}