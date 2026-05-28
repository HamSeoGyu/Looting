using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject skeletonPrefab;
    public GameObject bossPrefab;

    [Header("Route")]
    public LoopRoute stage1LoopRoute;

    [Header("Wave Settings")]
    public int maxWaves = 10;
    public int firstWaveCount = 5;
    public int addCountPerWave = 2;

    [Header("Spawn Timing")]
    public float spawnInterval = 0.5f;
    public float timeBetweenWaves = 5f;

    [Header("Boss")]
    public float bossScaleMultiplier = 1.6f;

    [Header("Optional")]
    public Transform enemyParent;

    private int currentWave = 0;
    private bool allWavesSpawned = false;
    private bool clearShown = false;

    void Start()
    {
        if (skeletonPrefab == null)
        {
            Debug.LogError("EnemySpawner: skeletonPrefab이 비어 있습니다.");
            return;
        }

        if (stage1LoopRoute == null || stage1LoopRoute.Count == 0)
        {
            Debug.LogError("EnemySpawner: stage1LoopRoute가 비어 있거나 포인트가 없습니다.");
            return;
        }

        StartCoroutine(StageRoutine());
    }

    void Update()
    {
        if (clearShown) return;
        if (!allWavesSpawned) return;
        if (StageMonsterManager.Instance == null) return;
        if (StageMonsterManager.Instance.IsStageFailed) return;

        if (StageMonsterManager.Instance.AliveCount == 0)
        {
            clearShown = true;

            if (StageResultUI.Instance != null)
            {
                StageResultUI.Instance.ShowStageClear();
            }
            else
            {
                Debug.LogWarning("StageResultUI가 연결되지 않아 클리어 UI를 띄우지 못했습니다.");
            }

            Debug.Log("스테이지 클리어");
        }
    }

    IEnumerator StageRoutine()
    {
        while (currentWave < maxWaves)
        {
            currentWave++;
            Debug.Log("웨이브 시작: " + currentWave);

            yield return StartCoroutine(SpawnWave(currentWave));

            if (currentWave < maxWaves)
            {
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        allWavesSpawned = true;
        Debug.Log("모든 웨이브 생성 완료");
    }

    IEnumerator SpawnWave(int wave)
    {
        int spawnCount = GetWaveSpawnCount(wave);

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnEnemy(skeletonPrefab, wave, false);
            yield return new WaitForSeconds(spawnInterval);
        }

        // 마지막 웨이브에서 보스 추가
        if (wave == maxWaves && bossPrefab != null)
        {
            SpawnEnemy(bossPrefab, wave, true);
        }
    }

    int GetWaveSpawnCount(int wave)
    {
        return firstWaveCount + ((wave - 1) * addCountPerWave);
    }

    int GetWaveHP(int wave, bool isBoss)
    {
        if (isBoss)
            return 180 + ((wave - 1) * 25);

        return 20 + ((wave - 1) * 6);
    }

    int GetWaveReward(int wave, bool isBoss)
    {
        if (isBoss)
            return 100 + ((wave - 1) * 10);

        return 10 + ((wave - 1) * 2);
    }

    void SpawnEnemy(GameObject prefab, int wave, bool isBoss)
    {
        if (prefab == null) return;

        Transform startPoint = stage1LoopRoute.GetPoint(0);
        if (startPoint == null)
        {
            Debug.LogWarning("EnemySpawner: 시작 포인트가 없습니다.");
            return;
        }

        GameObject enemy;

        if (enemyParent != null)
            enemy = Instantiate(prefab, startPoint.position, Quaternion.identity, enemyParent);
        else
            enemy = Instantiate(prefab, startPoint.position, Quaternion.identity);

        // 루프 경로 연결
        EnemyMove mover = enemy.GetComponent<EnemyMove>();
        if (mover != null)
        {
            mover.route = stage1LoopRoute;
        }
        else
        {
            Debug.LogWarning(enemy.name + " 에 EnemyMove가 없습니다.");
        }

        // 체력/보상 설정
        EnemyHealth hp = enemy.GetComponent<EnemyHealth>();
        if (hp != null)
        {
            hp.SetMaxHP(GetWaveHP(wave, isBoss));
            hp.rewardGold = GetWaveReward(wave, isBoss);
        }
        else
        {
            Debug.LogWarning(enemy.name + " 에 EnemyHealth가 없습니다.");
        }

        // 보스 크기 확대
        if (isBoss)
        {
            enemy.transform.localScale *= bossScaleMultiplier;
        }
    }
}