using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{
    [Header("Normal Unit Prefabs")]
    public GameObject warriorPrefab;
    public GameObject magePrefab;
    public GameObject archerPrefab;
    public GameObject pyromancerPrefab;
    public GameObject frostMagePrefab;
    public GameObject swampShamanPrefab;

    [Header("Boss Ally Prefab")]
    public GameObject bossAllyPrefab;

    [Header("Normal Unit Costs")]
    public int warriorCost = 50;
    public int mageCost = 70;
    public int archerCost = 60;
    public int pyromancerCost = 90;
    public int frostMageCost = 80;
    public int swampShamanCost = 85;

    [Header("Boss Ally Cost")]
    public int bossAllyCost = 0;

    [Header("Boss Ally Limit")]
    public bool canSummonBossOnlyOnce = true;
    public int bossActiveForWaveCount = 2;

    [Header("Stage Restriction")]
    [Tooltip("0이면 스테이지 제한 없음. 2로 두면 Stage2에서만 사용 가능.")]
    public int requiredStageNumberForBoss = 0;

    [Header("Boss Button")]
    public Button bossSummonButton;
    public GameObject bossLockedVisual;

    [Header("Build Nodes")]
    public Transform buildNodeParent;
    private BuildNode[] buildNodes;

    [Header("Spawn Parent")]
    public Transform unitParent;

    private bool bossSummoned = false;

    void Awake()
    {
        if (buildNodeParent != null)
        {
            buildNodes = buildNodeParent.GetComponentsInChildren<BuildNode>(true);
            Debug.Log("BuildNode 개수: " + buildNodes.Length);
        }
        else
        {
            Debug.LogWarning("buildNodeParent가 연결되지 않았습니다.");
        }

        UpdateBossButtonState();
    }

    void Start()
    {
        UpdateBossButtonState();
    }

    void Update()
    {
        UpdateBossButtonState();
    }

    public void SummonWarrior()
    {
        Debug.Log("전사 버튼 눌림");
        SummonUnit(warriorPrefab, warriorCost);
    }

    public void SummonMage()
    {
        Debug.Log("마법사 버튼 눌림");
        SummonUnit(magePrefab, mageCost);
    }

    public void SummonArcher()
    {
        Debug.Log("궁수 버튼 눌림");
        SummonUnit(archerPrefab, archerCost);
    }

    public void SummonPyromancer()
    {
        Debug.Log("방화범 버튼 눌림");
        SummonUnit(pyromancerPrefab, pyromancerCost);
    }

    public void SummonFrostMage()
    {
        Debug.Log("빙결사 버튼 눌림");
        SummonUnit(frostMagePrefab, frostMageCost);
    }

    public void SummonSwampShaman()
    {
        Debug.Log("늪지술사 버튼 눌림");
        SummonUnit(swampShamanPrefab, swampShamanCost);
    }

    public void SummonBossAlly()
    {
        Debug.Log("보스 소환 버튼 눌림");

        if (!CanSummonBossAlly())
        {
            Debug.Log("현재 보스 소환을 사용할 수 없습니다.");
            UpdateBossButtonState();
            return;
        }

        GameObject bossUnit = SummonUnit(bossAllyPrefab, bossAllyCost);

        if (bossUnit == null)
        {
            Debug.Log("보스 유닛 소환 실패");
            return;
        }

        TemporaryBossWaveLifetime lifetime = bossUnit.GetComponent<TemporaryBossWaveLifetime>();

        if (lifetime == null)
        {
            lifetime = bossUnit.AddComponent<TemporaryBossWaveLifetime>();
        }

        int currentWave = 1;

        if (EnemySpawner.Instance != null)
        {
            currentWave = Mathf.Max(1, EnemySpawner.Instance.CurrentWave);
        }

        lifetime.Setup(currentWave, bossActiveForWaveCount);

        bossSummoned = true;

        UpdateBossButtonState();

        Debug.Log("보스 유닛 소환 완료: " + bossUnit.name);
    }

    GameObject SummonUnit(GameObject unitPrefab, int cost)
    {
        if (unitPrefab == null)
        {
            Debug.LogWarning("유닛 프리팹이 연결되지 않았습니다.");
            return null;
        }

        if (GoldManager.Instance == null)
        {
            Debug.LogWarning("GoldManager가 없습니다.");
            return null;
        }

        if (!GoldManager.Instance.SpendGold(cost))
        {
            Debug.Log("골드가 부족합니다.");
            return null;
        }

        BuildNode randomNode = GetRandomEmptyNode();

        if (randomNode == null)
        {
            Debug.Log("빈 칸이 없습니다.");
            GoldManager.Instance.AddGold(cost);
            return null;
        }

        GameObject unit = Instantiate(unitPrefab);

        if (unitParent != null)
        {
            unit.transform.SetParent(unitParent, true);
        }

        unit.transform.position = randomNode.transform.position;
        unit.transform.localScale = Vector3.one;

        UnitDrag drag = unit.GetComponent<UnitDrag>();

        if (drag != null)
        {
            drag.SetCurrentNode(randomNode);
        }
        else
        {
            randomNode.placedUnit = unit;
            randomNode.isOccupied = true;
        }

        Debug.Log(unit.name + " 소환 완료");

        return unit;
    }

    BuildNode GetRandomEmptyNode()
    {
        List<BuildNode> emptyNodes = new List<BuildNode>();

        if (buildNodes == null || buildNodes.Length == 0)
        {
            Debug.LogWarning("BuildNode 배열이 비어 있습니다.");
            return null;
        }

        foreach (BuildNode node in buildNodes)
        {
            if (node == null) continue;

            if (node.IsEmpty())
            {
                emptyNodes.Add(node);
            }
        }

        if (emptyNodes.Count == 0)
            return null;

        int randomIndex = Random.Range(0, emptyNodes.Count);
        return emptyNodes[randomIndex];
    }

    bool CanSummonBossAlly()
    {
        if (bossAllyPrefab == null)
            return false;

        if (canSummonBossOnlyOnce && bossSummoned)
            return false;

        if (requiredStageNumberForBoss > 0)
        {
            int currentStageNumber = GetCurrentStageNumber();

            if (currentStageNumber != requiredStageNumberForBoss)
                return false;
        }

        return true;
    }

    void UpdateBossButtonState()
    {
        bool canSummon = CanSummonBossAlly();

        if (bossSummonButton != null)
        {
            bossSummonButton.interactable = canSummon;
        }

        if (bossLockedVisual != null)
        {
            bossLockedVisual.SetActive(!canSummon);
        }
    }

    int GetCurrentStageNumber()
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

        int stageNumber = 0;

        if (int.TryParse(numberText, out stageNumber))
        {
            return stageNumber;
        }

        return 0;
    }
}