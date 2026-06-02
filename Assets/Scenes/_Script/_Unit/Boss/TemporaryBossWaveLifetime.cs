using System.Collections;
using UnityEngine;

public class TemporaryBossWaveLifetime : MonoBehaviour
{
    [Header("Wave Lifetime")]
    public int activeForWaveCount = 2;

    [Header("Disappear")]
    public float fadeOutSeconds = 0.7f;

    private int summonWave;
    private bool initialized = false;
    private bool disappearing = false;

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                originalColors[i] = spriteRenderers[i].color;
            }
        }
    }

    private void OnEnable()
    {
        EnemySpawner.OnWaveStarted += HandleWaveStarted;
    }

    private void OnDisable()
    {
        EnemySpawner.OnWaveStarted -= HandleWaveStarted;
    }

    public void Setup(int currentWave, int waveCount)
    {
        summonWave = Mathf.Max(1, currentWave);
        activeForWaveCount = Mathf.Max(1, waveCount);
        initialized = true;

        Debug.Log(gameObject.name + " 보스 유닛 소환. 시작 웨이브: " + summonWave + ", 유지 웨이브 수: " + activeForWaveCount);
    }

    private void HandleWaveStarted(int newWave)
    {
        if (!initialized) return;
        if (disappearing) return;

        int expireWave = summonWave + activeForWaveCount;

        if (newWave >= expireWave)
        {
            StartCoroutine(DisappearRoutine());
        }
    }

    private IEnumerator DisappearRoutine()
    {
        disappearing = true;

        float timer = 0f;

        while (timer < fadeOutSeconds)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / fadeOutSeconds);
            float alpha = Mathf.Lerp(1f, 0f, t);

            SetAlpha(alpha);

            yield return null;
        }

        Destroy(gameObject);
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderers == null) return;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null) continue;

            Color color = originalColors[i];
            color.a = alpha;
            spriteRenderers[i].color = color;
        }
    }
}