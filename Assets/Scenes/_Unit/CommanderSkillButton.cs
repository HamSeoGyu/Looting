using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommanderSkillButton : MonoBehaviour
{
    [Header("UI")]
    public Button skillButton;
    public Image iconImage;
    public Image cooldownFillImage;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI remainUseText;

    [Header("Target")]
    public Transform unitParent;

    [Header("Skill Setting")]
    public int maxUseCount = 3;
    public float attackSpeedMultiplier = 1.5f; // 1.5배 빠르게
    public float buffSeconds = 5f;             // 버프 지속 시간
    public float cooldownSeconds = 10f;        // 쿨타임

    private int remainUseCount;
    private bool isCooling;

    private Coroutine buffCoroutine;

    private readonly Dictionary<MonoBehaviour, float> originalCooldowns =
        new Dictionary<MonoBehaviour, float>();

    private void Awake()
    {
        if (skillButton == null)
            skillButton = GetComponent<Button>();

        remainUseCount = maxUseCount;

        if (skillButton != null)
            skillButton.onClick.AddListener(OnClickSkill);

        UpdateUI(0f);
    }

    public void OnClickSkill()
    {
        if (isCooling) return;
        if (remainUseCount <= 0) return;

        remainUseCount--;

        // 혹시 이전 버프가 남아있으면 원복 후 다시 적용
        RestoreAttackSpeedBuff();

        if (buffCoroutine != null)
            StopCoroutine(buffCoroutine);

        buffCoroutine = StartCoroutine(BuffRoutine());
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator BuffRoutine()
    {
        float left = buffSeconds;

        // 버프 중 새로 소환된 유닛도 적용되도록 주기적으로 다시 찾음
        while (left > 0f)
        {
            ApplyAttackSpeedBuff();

            float interval = 0.25f;
            yield return new WaitForSeconds(interval);
            left -= interval;
        }

        RestoreAttackSpeedBuff();
        buffCoroutine = null;
    }

    private IEnumerator CooldownRoutine()
    {
        isCooling = true;

        float left = cooldownSeconds;

        while (left > 0f)
        {
            UpdateUI(left);
            left -= Time.deltaTime;
            yield return null;
        }

        isCooling = false;
        UpdateUI(0f);
    }

    private void ApplyAttackSpeedBuff()
    {
        List<UnitStats> units = GetAllyUnits();

        foreach (UnitStats unit in units)
        {
            if (unit == null) continue;

            MonoBehaviour[] behaviours = unit.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null) continue;

                FieldInfo field = behaviour.GetType().GetField(
                    "attackCooldown",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                );

                if (field == null) continue;
                if (field.FieldType != typeof(float)) continue;

                if (!originalCooldowns.ContainsKey(behaviour))
                {
                    float originalValue = (float)field.GetValue(behaviour);
                    originalCooldowns.Add(behaviour, originalValue);
                }

                float originalCooldown = originalCooldowns[behaviour];

                // 공속 증가 = 공격 쿨타임 감소
                float buffedCooldown = originalCooldown / attackSpeedMultiplier;

                // 너무 빨라져서 오류나는 것 방지
                buffedCooldown = Mathf.Max(0.05f, buffedCooldown);

                field.SetValue(behaviour, buffedCooldown);
            }
        }
    }

    private void RestoreAttackSpeedBuff()
    {
        foreach (KeyValuePair<MonoBehaviour, float> pair in originalCooldowns)
        {
            MonoBehaviour behaviour = pair.Key;

            if (behaviour == null) continue;

            FieldInfo field = behaviour.GetType().GetField(
                "attackCooldown",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );

            if (field == null) continue;
            if (field.FieldType != typeof(float)) continue;

            field.SetValue(behaviour, pair.Value);
        }

        originalCooldowns.Clear();
    }

    private List<UnitStats> GetAllyUnits()
    {
        AutoFindUnitParent();

        List<UnitStats> result = new List<UnitStats>();

        if (unitParent != null)
        {
            UnitStats[] unitsInParent = unitParent.GetComponentsInChildren<UnitStats>(true);
            result.AddRange(unitsInParent);
        }

        // unitParent 설정을 안 했을 때를 위한 예비 처리
        if (result.Count == 0)
        {
            UnitStats[] allUnits = FindObjectsOfType<UnitStats>();
            result.AddRange(allUnits);
        }

        return result;
    }

    private void AutoFindUnitParent()
    {
        if (unitParent != null) return;

        UnitManager unitManager = FindObjectOfType<UnitManager>();

        if (unitManager != null)
            unitParent = unitManager.unitParent;
    }

    private void UpdateUI(float cooldownLeft)
    {
        if (remainUseText != null)
            remainUseText.text = remainUseCount.ToString();

        bool canUse = !isCooling && remainUseCount > 0;

        if (skillButton != null)
            skillButton.interactable = canUse;

        if (iconImage != null)
        {
            Color color = iconImage.color;
            color.a = canUse ? 1f : 0.45f;
            iconImage.color = color;
        }

        if (cooldownFillImage != null)
        {
            cooldownFillImage.enabled = isCooling;
            cooldownFillImage.fillAmount =
                isCooling && cooldownSeconds > 0f
                    ? Mathf.Clamp01(cooldownLeft / cooldownSeconds)
                    : 0f;
        }

        if (cooldownText != null)
        {
            cooldownText.text =
                isCooling
                    ? Mathf.CeilToInt(cooldownLeft).ToString()
                    : "";
        }
    }
}