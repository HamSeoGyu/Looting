using UnityEngine;
using System;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    public static event Action<int> OnWaveStarted;

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

    [Header("Debug")]
    public bool startFromBossWave = false;
    public int debugStartWave = 1;

    [Header("Optional")]
    public Transform enemyParent;

    private int currentWave = 0;
    private bool allWavesSpawned = false;
    private bool clearShown = false;

    public int CurrentWave
    {
        get { return currentWave; }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

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

        if (startFromBossWave)
        {
            currentWave = maxWaves - 1;
        }
        else
        {
            currentWave = Mathf.Clamp(debugStartWave - 1, 0, maxWaves - 1);
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

            if (StageResultController.Instance != null)
            {
                StageResultController.Instance.ShowClear();
            }
            else
            {
                Debug.LogWarning("StageResultController가 연결되지 않아 클리어 UI를 띄우지 못했습니다.");
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

            OnWaveStarted?.Invoke(currentWave);

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
        if (WaveStartUI.Instance != null)
        {
            yield return WaveStartUI.Instance.ShowWave(wave);
        }

        bool isBossWave = wave == maxWaves && bossPrefab != null;

        if (isBossWave)
        {
            Debug.Log("보스 웨이브 시작 - 보스와 첫 몬스터 동시 소환");

            if (BGMManager.Instance != null)
            {
                BGMManager.Instance.PlayBossWaveBgm();
            }

            // 보스를 먼저 소환하고, 바로 아래 for문에서 첫 일반 몬스터도 소환됩니다.
            SpawnEnemy(bossPrefab, wave, true);
        }

        int spawnCount = GetWaveSpawnCount(wave);

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnEnemy(skeletonPrefab, wave, false);
            yield return new WaitForSeconds(spawnInterval);
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
        {
            enemy = Instantiate(prefab, startPoint.position, Quaternion.identity, enemyParent);
        }
        else
        {
            enemy = Instantiate(prefab, startPoint.position, Quaternion.identity);
        }

        EnemyMove mover = enemy.GetComponent<EnemyMove>();

        if (mover != null)
        {
            mover.route = stage1LoopRoute;
        }
        else
        {
            if (!isBoss)
            {
                Debug.LogWarning(enemy.name + " 에 EnemyMove가 없습니다.");
            }
        }

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

        if (isBoss)
        {
            enemy.transform.localScale *= bossScaleMultiplier;
        }
    }
}