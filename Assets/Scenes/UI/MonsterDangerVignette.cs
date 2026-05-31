using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class MonsterDangerVignette : MonoBehaviour
{
    [Header("Monster Count")]
    public string enemyTag = "Enemy";
    public int warningCount = 20;
    public int dangerCount = 25;
    public float countRefreshInterval = 0.2f;

    [Header("Vignette Shape")]
    public int textureSize = 512;

    [Range(0f, 1f)]
    public float edgeStart = 0.48f;

    [Range(0.5f, 4f)]
    public float edgePower = 1.8f;

    [Range(0f, 1f)]
    public float cornerBoost = 0.35f;

    [Header("Blink")]
    public float pulseSpeed = 2.2f;
    public float dangerPulseSpeed = 4.2f;

    [Range(0f, 1f)]
    public float minAlpha = 0.05f;

    [Range(0f, 1f)]
    public float maxAlpha = 0.45f;

    [Range(0f, 1f)]
    public float dangerMaxAlpha = 0.7f;

    public float appearSmoothSpeed = 6f;
    public float disappearSpeed = 3f;

    [Header("Color")]
    public Color warningColor = new Color(1f, 0f, 0f, 1f);

    [Header("Debug")]
    public bool alwaysShow = false;
    public int debugMonsterCount = -1;

    private RawImage rawImage;
    private Texture2D vignetteTexture;

    private int aliveMonsterCount;
    private float nextCountRefreshTime;
    private float currentAlpha;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        rawImage.raycastTarget = false;

        vignetteTexture = CreateVignetteTexture(textureSize);
        rawImage.texture = vignetteTexture;

        SetAlpha(0f);
    }

    private void Update()
    {
        RefreshMonsterCount();

        bool shouldShow = alwaysShow || aliveMonsterCount >= warningCount;

        if (shouldShow)
        {
            float dangerRatio = 0f;

            if (dangerCount > warningCount)
            {
                dangerRatio = Mathf.InverseLerp(
                    warningCount,
                    dangerCount,
                    aliveMonsterCount
                );
            }

            float speed = Mathf.Lerp(pulseSpeed, dangerPulseSpeed, dangerRatio);
            float targetMaxAlpha = Mathf.Lerp(maxAlpha, dangerMaxAlpha, dangerRatio);

            float pulse = (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f;
            pulse = Mathf.SmoothStep(0f, 1f, pulse);

            float targetAlpha = Mathf.Lerp(minAlpha, targetMaxAlpha, pulse);

            currentAlpha = Mathf.Lerp(
                currentAlpha,
                targetAlpha,
                Time.unscaledDeltaTime * appearSmoothSpeed
            );
        }
        else
        {
            currentAlpha = Mathf.MoveTowards(
                currentAlpha,
                0f,
                Time.unscaledDeltaTime * disappearSpeed
            );
        }

        SetAlpha(currentAlpha);
    }

    private void RefreshMonsterCount()
    {
        if (debugMonsterCount >= 0)
        {
            aliveMonsterCount = debugMonsterCount;
            return;
        }

        if (Time.unscaledTime < nextCountRefreshTime)
            return;

        nextCountRefreshTime = Time.unscaledTime + countRefreshInterval;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        aliveMonsterCount = enemies.Length;
    }

    private void SetAlpha(float alpha)
    {
        if (rawImage == null) return;

        Color color = warningColor;
        color.a = alpha;
        rawImage.color = color;
    }

    private Texture2D CreateVignetteTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = x / (float)(size - 1);
                float v = y / (float)(size - 1);

                float dx = Mathf.Abs(u - 0.5f) * 2f;
                float dy = Mathf.Abs(v - 0.5f) * 2f;

                // 화면 가장자리 쪽일수록 강해짐
                float edge = Mathf.Max(dx, dy);

                // 모서리 쪽은 조금 더 강하게
                float corner = dx * dy;

                float edgeAlpha = Mathf.InverseLerp(edgeStart, 1f, edge);
                edgeAlpha = Mathf.SmoothStep(0f, 1f, edgeAlpha);

                float cornerAlpha = Mathf.SmoothStep(0.25f, 1f, corner) * cornerBoost;

                float alpha = Mathf.Clamp01(edgeAlpha + cornerAlpha);
                alpha = Mathf.Pow(alpha, edgePower);

                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return texture;
    }

    private void OnDestroy()
    {
        if (vignetteTexture != null)
            Destroy(vignetteTexture);
    }
}