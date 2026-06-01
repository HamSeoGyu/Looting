using System.Collections;
using UnityEngine;

public class TreeBossBuffer : MonoBehaviour
{
    [Header("Buff Setting")]
    public string enemyTag = "Enemy";
    public float buffRange = 6f;
    public float buffInterval = 4f;
    public float buffDuration = 5f;

    [Range(0f, 0.9f)]
    public float damageReductionPercent = 0.25f;

    [Header("Effect")]
    public GameObject buffEffectPrefab;
    public bool autoAddBuffReceiver = true;

    [Header("Boss Visual")]
    public GameObject idleVisual;
    public GameObject castVisual;
    public float castVisualSeconds = 0.7f;

    private Coroutine routine;

    private void OnEnable()
    {
        routine = StartCoroutine(BuffLoop());
    }

    private void OnDisable()
    {
        if (routine != null)
            StopCoroutine(routine);
    }

    private IEnumerator BuffLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            // EnemyHealth가 죽을 때 태그를 Untagged로 바꾸므로,
            // 보스가 죽은 뒤에는 버프를 중단합니다.
            if (!CompareTag(enemyTag))
                yield break;

            CastBuff();

            yield return new WaitForSeconds(buffInterval);
        }
    }

    private void CastBuff()
    {
        StartCoroutine(CastVisualRoutine());

        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            if (enemy == gameObject) continue;

            // 다른 나무 보스끼리는 버프하지 않음
            if (enemy.GetComponent<TreeBossBuffer>() != null)
                continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance > buffRange)
                continue;

            EnemyBuffReceiver receiver = enemy.GetComponent<EnemyBuffReceiver>();

            if (receiver == null && autoAddBuffReceiver)
                receiver = enemy.AddComponent<EnemyBuffReceiver>();

            if (receiver == null)
                continue;

            receiver.ApplyDamageReductionBuff(
                buffDuration,
                damageReductionPercent,
                buffEffectPrefab
            );
        }

        Debug.Log("나무 보스 버프 발동");
    }

    private IEnumerator CastVisualRoutine()
    {
        if (castVisual != null)
            castVisual.SetActive(true);

        if (idleVisual != null)
            idleVisual.SetActive(false);

        yield return new WaitForSeconds(castVisualSeconds);

        if (castVisual != null)
            castVisual.SetActive(false);

        if (idleVisual != null)
            idleVisual.SetActive(true);
    }
}