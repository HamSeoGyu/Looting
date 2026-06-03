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

    [Header("Boss Wave Normal Monster Count")]
    public bool useCustomBossWaveSpawnCount = true;
    public int bossWaveNormalSpawnCount = 18;

    [Header("Spawn Timing")]
    public float spawnInterval = 1f;
    public float timeBetweenWaves = 5f;

    [Header("Boss")]
    public float bossScaleMultiplier = 1.6f;
    public bool spawnBossAtStartOfBossWave = true;
    public bool playBossBgm = true;

    [Header("Boss Time Limit")]
    public bool useBossTimeLimit = true;
    public float bossTimeLimitSeconds = 90f;
    public bool showBossLimitFromStart = true;

    [Header("Stats From Spawner")]
    public bool useSpawnerStats = true;

    [Header("Normal Enemy Stats")]
    public int normalEnemyBaseHP = 35;
    public int normalEnemyHPAddPerWave = 0;
    public int normalEnemyBaseReward = 7;
    public int normalEnemyRewardAddPerWave = 0;

    [Header("Boss Stats")]
    public int bossMaxHP = 380;
    public int bossRewardGold = 45;

    [Header("Optional")]
    public Transform enemyParent;
    public bool useWaveStartUI = true;

    private int currentWave = 0;
    private bool allWavesSpawned = false;
    private bool clearShown = false;
    private bool stageEnded = false;

    private EnemyHealth currentBossHealth;
    private bool bossTimerActive = false;

    public int CurrentWave
    {
        get { return currentWave; }
    }

    public int MaxWaves
    {
        get { return maxWaves; }
    }

    public bool IsBossWave
    {
        get { return currentWave == maxWaves; }
    }

    public bool IsStageEnded
    {
        get { return stageEnded; }
    }

    public bool AllWavesSpawned
    {
        get { return allWavesSpawned; }
    }

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (skeletonPrefab == null)
        {
            Debug.LogError("EnemySpawner: 일반 몬스터 프리팹이 비어 있습니다.");
            return;
        }

        if (stage1LoopRoute == null || stage1LoopRoute.Count == 0)
        {
            Debug.LogError("EnemySpawner: Route가 비어 있거나 포인트가 없습니다.");
            return;
        }

        if (StageTimerHUD.Instance != null)
        {
            StageTimerHUD.Instance.onBossTimeExpired.AddListener(HandleBossTimeExpired);
            StageTimerHUD.Instance.ResetBossTimer();
            StageTimerHUD.Instance.ResetNextWaveTimer();

            if (useBossTimeLimit && showBossLimitFromStart)
            {
                StageTimerHUD.Instance.SetBossLimitPreview(bossTimeLimitSeconds);
            }
        }
        else
        {
            Debug.LogWarning("EnemySpawner: StageTimerHUD.Instance가 없습니다. 타이머 UI 없이 진행됩니다.");
        }

        StartCoroutine(StageRoutine());
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (StageTimerHUD.Instance != null)
        {
            StageTimerHUD.Instance.onBossTimeExpired.RemoveListener(HandleBossTimeExpired);
        }
    }

    private void Update()
    {
        if (stageEnded) return;

        UpdateBossTimerStopCheck();
        CheckStageClear();
    }

    private IEnumerator StageRoutine()
    {
        while (currentWave < maxWaves)
        {
            if (stageEnded)
                yield break;

            currentWave++;

            OnWaveStarted?.Invoke(currentWave);

            Debug.Log("웨이브 시작: " + currentWave);

            yield return StartCoroutine(SpawnWave(currentWave));

            if (stageEnded)
                yield break;

            if (currentWave < maxWaves)
            {
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        allWavesSpawned = true;
        Debug.Log("모든 웨이브 생성 완료");
    }

    private IEnumerator SpawnWave(int wave)
    {
        if (StageTimerHUD.Instance != null)
        {
            StageTimerHUD.Instance.StopNextWaveTimer();

            if (useBossTimeLimit && showBossLimitFromStart && wave < maxWaves && !bossTimerActive)
            {
                StageTimerHUD.Instance.SetBossLimitPreview(bossTimeLimitSeconds);
            }
        }

        if (useWaveStartUI && WaveStartUI.Instance != null)
        {
            yield return WaveStartUI.Instance.ShowWave(wave);
        }

        bool isBossWave = wave == maxWaves && bossPrefab != null;
        int spawnCount = GetWaveSpawnCount(wave);

        if (!isBossWave && wave < maxWaves)
        {
            float nextWaveSeconds = CalculateNextWaveSeconds(spawnCount);
            StartNextWaveCountdown(nextWaveSeconds);
        }
        else
        {
            if (StageTimerHUD.Instance != null)
            {
                StageTimerHUD.Instance.StopNextWaveTimer();
            }
        }

        if (isBossWave && spawnBossAtStartOfBossWave)
        {
            StartBossWave();

            GameObject boss = SpawnEnemy(bossPrefab, wave, true);
            RegisterBoss(boss);
        }

        for (int i = 0; i < spawnCount; i++)
        {
            if (stageEnded)
                yield break;

            SpawnEnemy(skeletonPrefab, wave, false);

            yield return new WaitForSeconds(spawnInterval);
        }

        if (isBossWave && !spawnBossAtStartOfBossWave)
        {
            StartBossWave();

            GameObject boss = SpawnEnemy(bossPrefab, wave, true);
            RegisterBoss(boss);
        }
    }

    private float CalculateNextWaveSeconds(int spawnCount)
    {
        float spawnTime = spawnCount * spawnInterval;
        float totalTime = spawnTime + timeBetweenWaves;

        return Mathf.Max(0f, totalTime);
    }

    private void StartNextWaveCountdown(float seconds)
    {
        if (StageTimerHUD.Instance == null)
            return;

        StageTimerHUD.Instance.StartNextWaveTimer(seconds);
    }

    private void StartBossWave()
    {
        Debug.Log("보스 웨이브 시작");

        if (StageTimerHUD.Instance != null)
        {
            StageTimerHUD.Instance.StopNextWaveTimer();
        }

        if (playBossBgm)
        {
            if (BGMManager.Instance != null)
            {
                BGMManager.Instance.PlayBossWaveBgm();
            }
            else
            {
                Debug.LogWarning("BGMManager.Instance가 없습니다.");
            }
        }
    }

    private void RegisterBoss(GameObject boss)
    {
        if (boss == null)
            return;

        currentBossHealth = boss.GetComponent<EnemyHealth>();
        bossTimerActive = true;

        if (useBossTimeLimit && StageTimerHUD.Instance != null)
        {
            StageTimerHUD.Instance.StartBossTimer(bossTimeLimitSeconds);
        }
    }

    private void UpdateBossTimerStopCheck()
    {
        if (!bossTimerActive)
            return;

        if (currentBossHealth == null)
        {
            bossTimerActive = false;

            if (StageTimerHUD.Instance != null)
            {
                StageTimerHUD.Instance.StopBossTimer();
            }

            return;
        }

        if (currentBossHealth.IsDead)
        {
            bossTimerActive = false;

            if (StageTimerHUD.Instance != null)
            {
                StageTimerHUD.Instance.StopBossTimer();
            }

            Debug.Log("보스 처치 - 보스 제한시간 정지");
        }
    }

    private void CheckStageClear()
    {
        if (clearShown) return;
        if (!allWavesSpawned) return;
        if (StageMonsterManager.Instance == null) return;
        if (StageMonsterManager.Instance.IsStageFailed) return;

        if (StageMonsterManager.Instance.AliveCount == 0)
        {
            ShowStageClear();
        }
    }

    private void ShowStageClear()
    {
        if (clearShown) return;

        clearShown = true;
        stageEnded = true;

        if (StageTimerHUD.Instance != null)
        {
            StageTimerHUD.Instance.StopBossTimer();
            StageTimerHUD.Instance.StopNextWaveTimer();
        }

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

    private void HandleBossTimeExpired()
    {
        if (stageEnded)
            return;

        stageEnded = true;
        clearShown = true;

        StopAllCoroutines();

        if (StageTimerHUD.Instance != null)
        {
            StageTimerHUD.Instance.StopNextWaveTimer();
        }

        if (StageResultController.Instance != null)
        {
            StageResultController.Instance.ShowFail();
        }
        else
        {
            Time.timeScale = 0f;
            Debug.LogWarning("StageResultController가 없어 Time.timeScale만 정지했습니다.");
        }

        Debug.Log("보스 제한시간 초과 - 스테이지 실패");
    }

    private int GetWaveSpawnCount(int wave)
    {
        if (wave == maxWaves && useCustomBossWaveSpawnCount)
        {
            return Mathf.Max(0, bossWaveNormalSpawnCount);
        }

        int count = firstWaveCount + ((wave - 1) * addCountPerWave);
        return Mathf.Max(0, count);
    }

    private int GetWaveHP(int wave, bool isBoss)
    {
        if (isBoss)
            return bossMaxHP;

        return normalEnemyBaseHP + ((wave - 1) * normalEnemyHPAddPerWave);
    }

    private int GetWaveReward(int wave, bool isBoss)
    {
        if (isBoss)
            return bossRewardGold;

        return normalEnemyBaseReward + ((wave - 1) * normalEnemyRewardAddPerWave);
    }

    private GameObject SpawnEnemy(GameObject prefab, int wave, bool isBoss)
    {
        if (prefab == null)
            return null;

        Transform startPoint = stage1LoopRoute.GetPoint(0);

        if (startPoint == null)
        {
            Debug.LogWarning("EnemySpawner: 시작 포인트가 없습니다.");
            return null;
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
            Debug.LogWarning(enemy.name + " 에 EnemyMove가 없습니다.");
        }

        EnemyHealth hp = enemy.GetComponent<EnemyHealth>();

        if (hp != null)
        {
            if (useSpawnerStats)
            {
                hp.SetMaxHP(GetWaveHP(wave, isBoss));
                hp.rewardGold = GetWaveReward(wave, isBoss);
            }
        }
        else
        {
            Debug.LogWarning(enemy.name + " 에 EnemyHealth가 없습니다.");
        }

        if (isBoss)
        {
            enemy.transform.localScale *= bossScaleMultiplier;
        }

        return enemy;
    }
}