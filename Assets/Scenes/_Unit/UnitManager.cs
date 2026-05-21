using UnityEngine;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{
    [Header("Unit Prefabs")]
    public GameObject warriorPrefab;
    public GameObject magePrefab;
    public GameObject archerPrefab;
    public GameObject pyromancerPrefab;
    public GameObject frostMagePrefab;
    public GameObject swampShamanPrefab;

    [Header("Unit Costs")]
    public int warriorCost = 50;
    public int mageCost = 70;
    public int archerCost = 60;
    public int pyromancerCost = 90;
    public int frostMageCost = 80;
    public int swampShamanCost = 85;

    [Header("Build Nodes")]
    public Transform buildNodeParent;
    private BuildNode[] buildNodes;

    [Header("Spawn Parent")]
    public Transform unitParent;

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

    void SummonUnit(GameObject unitPrefab, int cost)
    {
        if (unitPrefab == null)
        {
            Debug.LogWarning("유닛 프리팹이 연결되지 않았습니다.");
            return;
        }

        if (GoldManager.Instance == null)
        {
            Debug.LogWarning("GoldManager가 없습니다.");
            return;
        }

        if (!GoldManager.Instance.SpendGold(cost))
        {
            Debug.Log("골드가 부족합니다.");
            return;
        }

        BuildNode randomNode = GetRandomEmptyNode();

        if (randomNode == null)
        {
            Debug.Log("빈 칸이 없습니다.");
            GoldManager.Instance.AddGold(cost);
            return;
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
}