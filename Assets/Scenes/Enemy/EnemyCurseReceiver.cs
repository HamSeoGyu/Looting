using System.Collections;
using UnityEngine;

public class EnemyCurseReceiver : MonoBehaviour
{
    [Header("Curse")]
    [Range(0f, 2f)]
    public float currentExtraDamageTaken = 0f;

    [Header("Effect")]
    public Transform effectParent;

    private GameObject curseEffect;
    private Coroutine curseRoutine;

    public int ModifyIncomingDamage(int originalDamage)
    {
        if (originalDamage <= 0) return 0;

        float multiplier = 1f + currentExtraDamageTaken;
        int modifiedDamage = Mathf.RoundToInt(originalDamage * multiplier);

        return Mathf.Max(1, modifiedDamage);
    }

    public void ApplyCurse(float duration, float extraDamageTakenPercent)
    {
        extraDamageTakenPercent = Mathf.Clamp(extraDamageTakenPercent, 0f, 2f);

        if (curseRoutine != null)
        {
            StopCoroutine(curseRoutine);
        }

        curseRoutine = StartCoroutine(CurseRoutine(duration, extraDamageTakenPercent));
    }

    private IEnumerator CurseRoutine(float duration, float extraDamageTakenPercent)
    {
        currentExtraDamageTaken = extraDamageTakenPercent;

        ShowCurseEffect();

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        currentExtraDamageTaken = 0f;
        HideCurseEffect();

        curseRoutine = null;
    }

    private void ShowCurseEffect()
    {
        if (curseEffect == null)
        {
            curseEffect = CreateCurseEffect();
        }

        curseEffect.SetActive(true);

        ParticleSystem ps = curseEffect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }
    }

    private void HideCurseEffect()
    {
        if (curseEffect == null) return;

        ParticleSystem ps = curseEffect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop();
        }

        curseEffect.SetActive(false);
    }

    private GameObject CreateCurseEffect()
    {
        Transform parent = effectParent != null ? effectParent : transform;

        GameObject effect = new GameObject("CurseEffect");
        effect.transform.SetParent(parent);
        effect.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        effect.transform.localRotation = Quaternion.identity;
        effect.transform.localScale = Vector3.one;

        ParticleSystem ps = effect.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.duration = 1f;
        main.startLifetime = 0.65f;
        main.startSpeed = 0.25f;
        main.startSize = 0.18f;
        main.startColor = new Color(0.65f, 0.15f, 1f, 0.9f);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 15f;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.45f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient gradient = new Gradient();

        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.9f, 0.2f, 1f), 0f),
                new GradientColorKey(new Color(0.25f, 0f, 0.45f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );

        colorOverLifetime.color = gradient;

        ParticleSystemRenderer renderer = effect.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 150;

        return effect;
    }
}