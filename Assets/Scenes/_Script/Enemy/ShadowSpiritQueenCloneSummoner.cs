using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowSpiritQueenCloneSummoner : MonoBehaviour
{
    [Header("Clone Summon")]
    public GameObject clonePrefab;
    public int clonesPerSummon = 2;
    public int maxActiveClones = 6;
    public float firstSummonDelay = 3f;
    public float summonInterval = 7f;
    public float spawnRadius = 0.35f;

    [Header("Clone Stats")]
    public int cloneMaxHP = 80;
    public int cloneRewardGold = 0;
    public float cloneMoveSpeed = 1.9f;
    public float cloneScaleMultiplier = 0.65f;

    [Header("Clone Visual")]
    public bool applyCloneTint = false;
    public Color cloneTint = new Color(0.65f, 0.45f, 1f, 0.9f);

    [Header("Cast Motion")]
    public Transform visualRoot;
    public float castMotionDuration = 0.35f;
    public float castScaleMultiplier = 1.12f;
    public float castRiseAmount = 0.08f;
    public string animatorTriggerName = "";

    [Header("Debug")]
    public bool showDebugLog = true;

    private EnemyHealth bossHealth;
    private EnemyMove bossMove;
    private Animator animator;
    private Vector3 visualStartPos;
    private Vector3 visualStartScale;

    private readonly List<GameObject> activeClones = new List<GameObject>();

    private void Awake()
    {
        bossHealth = GetComponent<EnemyHealth>();
        bossMove = GetComponent<EnemyMove>();
        animator = GetComponentInChildren<Animator>();

        if (visualRoot == null)
        {
            visualRoot = transform;
        }

        visualStartPos = visualRoot.localPosition;
        visualStartScale = visualRoot.localScale;
    }

    private void Start()
    {
        StartCoroutine(SummonLoop());
    }

    private IEnumerator SummonLoop()
    {
        yield return new WaitForSeconds(firstSummonDelay);

        while (true)
        {
            yield return new WaitForSeconds(summonInterval);

            if (!CanSummon())
                continue;

            yield return StartCoroutine(CastMotionRoutine());

            SummonClones();
        }
    }

    private bool CanSummon()
    {
        if (bossHealth != null && bossHealth.IsDead)
            return false;

        if (clonePrefab == null)
            return false;

        if (bossMove == null || bossMove.route == null)
            return false;

        CleanCloneList();

        if (activeClones.Count >= maxActiveClones)
            return false;

        return true;
    }

    private void CleanCloneList()
    {
        for (int i = activeClones.Count - 1; i >= 0; i--)
        {
            if (activeClones[i] == null)
            {
                activeClones.RemoveAt(i);
                continue;
            }

            EnemyHealth hp = activeClones[i].GetComponent<EnemyHealth>();
            if (hp != null && hp.IsDead)
            {
                activeClones.RemoveAt(i);
            }
        }
    }

    private void SummonClones()
    {
        CleanCloneList();

        int availableCount = maxActiveClones - activeClones.Count;
        int summonCount = Mathf.Min(clonesPerSummon, availableCount);

        for (int i = 0; i < summonCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(0.05f, spawnRadius);
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, randomCircle.y, 0f);

            GameObject clone = Instantiate(clonePrefab, spawnPos, Quaternion.identity);
            clone.name = "ShadowSpiritQueenClone";

            clone.tag = "Enemy";

            // 분신이 다시 분신을 소환하지 못하게 막기
            ShadowSpiritQueenCloneSummoner cloneSummoner = clone.GetComponent<ShadowSpiritQueenCloneSummoner>();
            if (cloneSummoner != null)
            {
                Destroy(cloneSummoner);
            }

            EnemyHealth cloneHealth = clone.GetComponent<EnemyHealth>();
            if (cloneHealth != null)
            {
                cloneHealth.SetMaxHP(cloneMaxHP);
                cloneHealth.rewardGold = cloneRewardGold;
            }

            EnemyMove cloneMove = clone.GetComponent<EnemyMove>();
            if (cloneMove != null)
            {
                cloneMove.route = bossMove.route;
                cloneMove.moveSpeed = cloneMoveSpeed;
                cloneMove.InitializeOnRoute(bossMove.route, spawnPos, true);
            }

            clone.transform.localScale = clone.transform.localScale * cloneScaleMultiplier;

            if (applyCloneTint)
            {
                ApplyTint(clone);
            }

            activeClones.Add(clone);
        }

        if (showDebugLog)
        {
            Debug.Log("ShadowSpiritQueen 분신 소환: " + summonCount + "마리");
        }
    }

    private void ApplyTint(GameObject clone)
    {
        SpriteRenderer[] renderers = clone.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null) continue;

            // HP바까지 색이 바뀌는 게 싫으면 이름으로 제외
            if (renderer.name.ToLower().Contains("hp"))
                continue;

            renderer.color = cloneTint;
        }
    }

    private IEnumerator CastMotionRoutine()
    {
        PlayAnimatorTrigger();

        if (visualRoot == null)
            yield break;

        float half = castMotionDuration * 0.5f;

        Vector3 startPos = visualRoot.localPosition;
        Vector3 startScale = visualRoot.localScale;

        Vector3 peakPos = visualStartPos + new Vector3(0f, castRiseAmount, 0f);
        Vector3 peakScale = visualStartScale * castScaleMultiplier;

        float timer = 0f;

        while (timer < half)
        {
            timer += Time.deltaTime;
            float p = Mathf.Clamp01(timer / half);

            visualRoot.localPosition = Vector3.Lerp(startPos, peakPos, p);
            visualRoot.localScale = Vector3.Lerp(startScale, peakScale, p);

            yield return null;
        }

        timer = 0f;

        while (timer < half)
        {
            timer += Time.deltaTime;
            float p = Mathf.Clamp01(timer / half);

            visualRoot.localPosition = Vector3.Lerp(peakPos, visualStartPos, p);
            visualRoot.localScale = Vector3.Lerp(peakScale, visualStartScale, p);

            yield return null;
        }

        visualRoot.localPosition = visualStartPos;
        visualRoot.localScale = visualStartScale;
    }

    private void PlayAnimatorTrigger()
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(animatorTriggerName)) return;

        if (!HasTriggerParameter(animator, animatorTriggerName))
            return;

        animator.ResetTrigger(animatorTriggerName);
        animator.SetTrigger(animatorTriggerName);
    }

    private bool HasTriggerParameter(Animator anim, string paramName)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Trigger)
                return true;
        }

        return false;
    }
}