using System.Collections;
using UnityEngine;

public class EnemyBuffReceiver : MonoBehaviour
{
    [Header("Damage Reduction")]
    [Range(0f, 0.9f)]
    public float currentDamageReduction = 0f;

    [Header("Effect")]
    public Transform effectParent;

    private GameObject activeBuffEffect;
    private Coroutine buffRoutine;

    public int ModifyIncomingDamage(int originalDamage)
    {
        if (originalDamage <= 0) return 0;

        float multiplier = 1f - currentDamageReduction;
        int modifiedDamage = Mathf.CeilToInt(originalDamage * multiplier);

        return Mathf.Max(1, modifiedDamage);
    }

    public void ApplyDamageReductionBuff(float duration, float reductionPercent, GameObject buffEffectPrefab)
    {
        reductionPercent = Mathf.Clamp(reductionPercent, 0f, 0.9f);

        if (buffRoutine != null)
            StopCoroutine(buffRoutine);

        buffRoutine = StartCoroutine(BuffRoutine(duration, reductionPercent, buffEffectPrefab));
    }

    private IEnumerator BuffRoutine(float duration, float reductionPercent, GameObject buffEffectPrefab)
    {
        currentDamageReduction = reductionPercent;

        ShowBuffEffect(buffEffectPrefab);

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        currentDamageReduction = 0f;
        HideBuffEffect();

        buffRoutine = null;
    }

    private void ShowBuffEffect(GameObject buffEffectPrefab)
    {
        if (buffEffectPrefab == null) return;

        if (activeBuffEffect == null)
        {
            Transform parent = effectParent != null ? effectParent : transform;
            activeBuffEffect = Instantiate(buffEffectPrefab, parent);
            activeBuffEffect.transform.localPosition = Vector3.zero;
            activeBuffEffect.transform.localRotation = Quaternion.identity;
            activeBuffEffect.transform.localScale = Vector3.one;
        }

        activeBuffEffect.SetActive(true);
    }

    private void HideBuffEffect()
    {
        if (activeBuffEffect != null)
            activeBuffEffect.SetActive(false);
    }
}