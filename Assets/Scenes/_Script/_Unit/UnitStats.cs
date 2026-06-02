using System.Reflection;
using UnityEngine;

public class UnitStats : MonoBehaviour
{
    public enum UnitType
    {
        Warrior,
        Mage,
        Archer,
        Pyromancer,
        FrostMage,
        SwampShaman
    }

    [Header("Unit Type")]
    public UnitType unitType;

    [Header("Current Stats")]
    public int level = 1;
    public int attackPower;
    public int upgradeCost;

    [Header("Base Stats (-1이면 타입 기본값 사용)")]
    public int baseAttack = -1;
    public int baseUpgradeCost = -1;
    public int attackIncreasePerUpgrade = -1;

    [Header("Upgrade Rule")]
    public int upgradeCostIncreasePerLevel = 50;

    private bool initialized = false;

    void Awake()
    {
        InitializeIfNeeded();
    }

    void Start()
    {
        SyncAttackToCombatScripts();
    }

    void OnValidate()
    {
        if (upgradeCostIncreasePerLevel <= 0)
            upgradeCostIncreasePerLevel = 50;
    }

    public void InitializeIfNeeded()
    {
        if (initialized) return;

        int defaultAttack;
        int defaultCost;
        int defaultIncrease;
        GetDefaultsByType(unitType, out defaultAttack, out defaultCost, out defaultIncrease);

        if (baseAttack <= 0)
            baseAttack = defaultAttack;

        if (baseUpgradeCost <= 0)
            baseUpgradeCost = defaultCost;

        if (attackIncreasePerUpgrade <= 0)
            attackIncreasePerUpgrade = defaultIncrease;

        level = Mathf.Max(1, level);
        attackPower = baseAttack;
        upgradeCost = baseUpgradeCost;

        initialized = true;
        SyncAttackToCombatScripts();
    }

    void GetDefaultsByType(UnitType type, out int defaultAttack, out int defaultCost, out int defaultIncrease)
    {
        switch (type)
        {
            case UnitType.Warrior:
                defaultAttack = 10;
                defaultCost = 50;
                defaultIncrease = 5;
                break;

            case UnitType.Mage:
                defaultAttack = 7;
                defaultCost = 70;
                defaultIncrease = 4;
                break;

            case UnitType.Archer:
                defaultAttack = 8;
                defaultCost = 60;
                defaultIncrease = 4;
                break;

            case UnitType.Pyromancer:
                defaultAttack = 10;
                defaultCost = 90;
                defaultIncrease = 4;
                break;

            case UnitType.FrostMage:
                defaultAttack = 6;
                defaultCost = 80;
                defaultIncrease = 3;
                break;

            case UnitType.SwampShaman:
                defaultAttack = 3;
                defaultCost = 85;
                defaultIncrease = 2;
                break;

            default:
                defaultAttack = 5;
                defaultCost = 50;
                defaultIncrease = 2;
                break;
        }
    }

    public string GetUnitNameKorean()
    {
        switch (unitType)
        {
            case UnitType.Warrior:
                return "전사";
            case UnitType.Mage:
                return "마법사";
            case UnitType.Archer:
                return "궁수";
            case UnitType.Pyromancer:
                return "방화범";
            case UnitType.FrostMage:
                return "빙결사";
            case UnitType.SwampShaman:
                return "늪지술사";
            default:
                return "유닛";
        }
    }

    public int GetCurrentAttack()
    {
        InitializeIfNeeded();
        return attackPower;
    }

    public int GetUpgradeCost()
    {
        InitializeIfNeeded();
        return upgradeCost;
    }

    public bool CanUpgrade()
    {
        InitializeIfNeeded();

        if (GoldManager.Instance == null)
            return false;

        return GoldManager.Instance.CurrentGold >= upgradeCost;
    }

    public bool TryUpgrade()
    {
        InitializeIfNeeded();

        if (GoldManager.Instance == null)
        {
            Debug.LogWarning("GoldManager가 없습니다.");
            return false;
        }

        if (!GoldManager.Instance.SpendGold(upgradeCost))
        {
            Debug.Log("골드가 부족합니다.");
            return false;
        }

        level++;
        attackPower += attackIncreasePerUpgrade;
        upgradeCost += upgradeCostIncreasePerLevel;

        SyncAttackToCombatScripts();

        Debug.Log(GetUnitNameKorean() + " 강화 완료 / Lv." + level + " / 공격력: " + attackPower + " / 다음 비용: " + upgradeCost);
        return true;
    }

    public void SyncAttackToCombatScripts()
    {
        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null) continue;

            // 일반적으로 자주 쓰는 공격력 필드명들
            TrySetIntField(behaviour, "damage", attackPower);
            TrySetIntField(behaviour, "directDamage", attackPower);
            TrySetIntField(behaviour, "attackDamage", attackPower);
            TrySetIntField(behaviour, "attackPower", attackPower);
        }
    }

    void TrySetIntField(MonoBehaviour target, string fieldName, int value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (field == null) return;
        if (field.FieldType != typeof(int)) return;

        field.SetValue(target, value);
    }

    [ContextMenu("Reset Stats By Unit Type")]
    public void ResetStatsByType()
    {
        initialized = false;

        int defaultAttack;
        int defaultCost;
        int defaultIncrease;
        GetDefaultsByType(unitType, out defaultAttack, out defaultCost, out defaultIncrease);

        baseAttack = defaultAttack;
        baseUpgradeCost = defaultCost;
        attackIncreasePerUpgrade = defaultIncrease;

        level = 1;
        attackPower = baseAttack;
        upgradeCost = baseUpgradeCost;

        initialized = true;
        SyncAttackToCombatScripts();

        Debug.Log(GetUnitNameKorean() + " 기본 스탯으로 초기화 완료");
    }
}