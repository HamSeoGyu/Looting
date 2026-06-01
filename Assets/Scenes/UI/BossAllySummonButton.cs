using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BossAllySummonButton : MonoBehaviour
{
    [Header("Boss Unit")]
    public GameObject bossUnitPrefab;
    public Transform summonPoint;
    public Transform unitParent;

    [Header("Stage Restriction")]
    public int requiredStageNumber = 0;

    [Header("Wave Restriction")]
    public int requiredWaveNumber = 0;

    [Header("Lifetime")]
    public int activeForWaveCount = 2;

    [Header("Use Limit")]
    public bool canSummonOnlyOnce = true;

    [Header("UI")]
    public Button button;
    public GameObject lockedVisual;

    private bool hasSummoned = false;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(SummonBossUnit);
    }

    private void Update()
    {
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        bool available = IsAvailable();

        if (button != null)
            button.interactable = available;

        if (lockedVisual != null)
            lockedVisual.SetActive(!available);
    }

    private bool IsAvailable()
    {
        if (bossUnitPrefab == null)
            return false;

        if (canSummonOnlyOnce && hasSummoned)
            return false;

        if (requiredStageNumber > 0)
        {
            int currentStage = GetCurrentStageNumber();

            if (currentStage != requiredStageNumber)
                return false;
        }

        if (requiredWaveNumber > 0)
        {
            if (EnemySpawner.Instance == null)
                return false;

            if (EnemySpawner.Instance.CurrentWave != requiredWaveNumber)
                return false;
        }

        return true;
    }

    private void SummonBossUnit()
    {
        if (!IsAvailable())
            return;

        Vector3 spawnPosition = Vector3.zero;

        if (summonPoint != null)
            spawnPosition = summonPoint.position;

        GameObject bossUnit;

        if (unitParent != null)
        {
            bossUnit = Instantiate(bossUnitPrefab, spawnPosition, Quaternion.identity, unitParent);
        }
        else
        {
            bossUnit = Instantiate(bossUnitPrefab, spawnPosition, Quaternion.identity);
        }

        TemporaryBossWaveLifetime lifetime = bossUnit.GetComponent<TemporaryBossWaveLifetime>();

        if (lifetime == null)
        {
            lifetime = bossUnit.AddComponent<TemporaryBossWaveLifetime>();
        }

        int currentWave = 1;

        if (EnemySpawner.Instance != null)
            currentWave = Mathf.Max(1, EnemySpawner.Instance.CurrentWave);

        lifetime.Setup(currentWave, activeForWaveCount);

        hasSummoned = true;

        UpdateButtonState();

        Debug.Log("보스 유닛 소환 완료: " + bossUnit.name);
    }

    private int GetCurrentStageNumber()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        string numberText = "";

        for (int i = 0; i < sceneName.Length; i++)
        {
            if (char.IsDigit(sceneName[i]))
            {
                numberText += sceneName[i];
            }
        }

        int result = 0;

        if (int.TryParse(numberText, out result))
        {
            return result;
        }

        return 0;
    }
}