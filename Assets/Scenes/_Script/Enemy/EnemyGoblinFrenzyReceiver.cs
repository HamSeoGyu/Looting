using System.Collections;
using System.Reflection;
using UnityEngine;

public class EnemyGoblinFrenzyReceiver : MonoBehaviour
{
    [Header("Frenzy State")]
    public bool isFrenzied = false;

    [Range(0f, 0.9f)]
    public float currentDamageReduction = 0f;

    [Header("Effect Position")]
    public float effectCenterOffset = 0.05f;
    public float effectScale = 0.85f;

    private Coroutine frenzyRoutine;
    private GameObject frenzyEffect;
    private FrenzyFlameWorldEffect frenzyFollower;

    private Component enemyMoveComponent;
    private FieldInfo speedField;
    private PropertyInfo speedProperty;
    private float originalSpeed;
    private bool hasOriginalSpeed = false;

    private static Sprite circleSprite;
    private static Sprite ringSprite;
    private static Sprite flameSprite;

    private void Awake()
    {
        CacheMoveSpeed();
    }

    public int ModifyIncomingDamage(int originalDamage)
    {
        if (originalDamage <= 0) return 0;

        if (!isFrenzied)
            return originalDamage;

        float multiplier = 1f - currentDamageReduction;
        int modifiedDamage = Mathf.RoundToInt(originalDamage * multiplier);

        return Mathf.Max(1, modifiedDamage);
    }

    public void ApplyFrenzy(float duration, float speedMultiplier, float damageReduction)
    {
        duration = Mathf.Max(0.1f, duration);
        speedMultiplier = Mathf.Max(0.1f, speedMultiplier);
        damageReduction = Mathf.Clamp(damageReduction, 0f, 0.9f);

        if (frenzyRoutine != null)
        {
            StopCoroutine(frenzyRoutine);
        }

        frenzyRoutine = StartCoroutine(FrenzyRoutine(duration, speedMultiplier, damageReduction));
    }

    private IEnumerator FrenzyRoutine(float duration, float speedMultiplier, float damageReduction)
    {
        isFrenzied = true;
        currentDamageReduction = damageReduction;

        ApplySpeedMultiplier(speedMultiplier);
        ShowFrenzyEffect();

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        isFrenzied = false;
        currentDamageReduction = 0f;

        RestoreSpeed();
        HideFrenzyEffect();

        frenzyRoutine = null;
    }

    private void ShowFrenzyEffect()
    {
        if (frenzyEffect == null)
        {
            frenzyEffect = CreateFrenzyEffect();
        }

        if (frenzyFollower != null)
        {
            frenzyFollower.target = transform;
            frenzyFollower.offset = new Vector3(0f, effectCenterOffset, 0f);
        }

        frenzyEffect.SetActive(true);
    }

    private void HideFrenzyEffect()
    {
        if (frenzyEffect != null)
        {
            frenzyEffect.SetActive(false);
        }
    }

    private GameObject CreateFrenzyEffect()
    {
        PrepareSprites();

        int sortingLayerID = 0;
        int sortingOrder = 4900;

        SpriteRenderer baseRenderer = GetComponentInChildren<SpriteRenderer>();

        if (baseRenderer != null)
        {
            sortingLayerID = baseRenderer.sortingLayerID;
            sortingOrder = baseRenderer.sortingOrder + 550;
        }

        GameObject root = new GameObject("VisibleGoblinFrenzyEffect");
        root.layer = gameObject.layer;
        root.transform.position = transform.position + new Vector3(0f, effectCenterOffset, 0f);
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one * effectScale;

        frenzyFollower = root.AddComponent<FrenzyFlameWorldEffect>();
        frenzyFollower.target = transform;
        frenzyFollower.offset = new Vector3(0f, effectCenterOffset, 0f);

        GameObject redGlow = CreateSpriteObject(
            root.transform,
            "FrenzyRedGlow",
            circleSprite,
            new Vector3(0f, 0f, 0f),
            new Vector3(1.2f, 0.9f, 1f),
            new Color(1f, 0.05f, 0f, 0.22f),
            sortingLayerID,
            sortingOrder
        );


        Transform[] flames = new Transform[7];

        flames[0] = CreateFlame(root.transform, "FrenzyFlame0", new Vector3(-0.48f, -0.32f, 0f), new Vector3(0.22f, 0.45f, 1f), sortingLayerID, sortingOrder + 2, new Color(1f, 0.1f, 0.02f, 0.95f)).transform;
        flames[1] = CreateFlame(root.transform, "FrenzyFlame1", new Vector3(-0.30f, -0.38f, 0f), new Vector3(0.18f, 0.38f, 1f), sortingLayerID, sortingOrder + 3, new Color(1f, 0.35f, 0.02f, 0.95f)).transform;
        flames[2] = CreateFlame(root.transform, "FrenzyFlame2", new Vector3(-0.12f, -0.34f, 0f), new Vector3(0.24f, 0.55f, 1f), sortingLayerID, sortingOrder + 4, new Color(1f, 0.05f, 0.02f, 0.95f)).transform;
        flames[3] = CreateFlame(root.transform, "FrenzyFlame3", new Vector3(0.08f, -0.36f, 0f), new Vector3(0.2f, 0.45f, 1f), sortingLayerID, sortingOrder + 5, new Color(1f, 0.45f, 0.02f, 0.95f)).transform;
        flames[4] = CreateFlame(root.transform, "FrenzyFlame4", new Vector3(0.28f, -0.34f, 0f), new Vector3(0.22f, 0.5f, 1f), sortingLayerID, sortingOrder + 6, new Color(1f, 0.08f, 0.02f, 0.95f)).transform;
        flames[5] = CreateFlame(root.transform, "FrenzyFlame5", new Vector3(0.48f, -0.38f, 0f), new Vector3(0.18f, 0.38f, 1f), sortingLayerID, sortingOrder + 7, new Color(1f, 0.38f, 0.02f, 0.95f)).transform;
        flames[6] = CreateFlame(root.transform, "FrenzyFlame6", new Vector3(0f, -0.28f, 0f), new Vector3(0.28f, 0.7f, 1f), sortingLayerID, sortingOrder + 8, new Color(1f, 0.05f, 0.02f, 0.9f)).transform;

        frenzyFollower.redGlow = redGlow.transform;
        frenzyFollower.flames = flames;
        frenzyFollower.Initialize();

        return root;
    }

    private GameObject CreateSpriteObject(
        Transform parent,
        string objectName,
        Sprite sprite,
        Vector3 localPosition,
        Vector3 localScale,
        Color color,
        int sortingLayerID,
        int sortingOrder)
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = localScale;

        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingLayerID = sortingLayerID;
        renderer.sortingOrder = sortingOrder;

        return obj;
    }

    private GameObject CreateFlame(
        Transform parent,
        string objectName,
        Vector3 localPosition,
        Vector3 localScale,
        int sortingLayerID,
        int sortingOrder,
        Color color)
    {
        return CreateSpriteObject(
            parent,
            objectName,
            flameSprite,
            localPosition,
            localScale,
            color,
            sortingLayerID,
            sortingOrder
        );
    }

    private void CacheMoveSpeed()
    {
        enemyMoveComponent = GetComponent("EnemyMove");

        if (enemyMoveComponent == null)
            return;

        System.Type type = enemyMoveComponent.GetType();

        speedField = type.GetField("moveSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (speedField == null)
            speedField = type.GetField("speed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (speedField != null && speedField.FieldType == typeof(float))
        {
            originalSpeed = (float)speedField.GetValue(enemyMoveComponent);
            hasOriginalSpeed = true;
            return;
        }

        speedProperty = type.GetProperty("MoveSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (speedProperty == null)
            speedProperty = type.GetProperty("Speed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (speedProperty != null &&
            speedProperty.PropertyType == typeof(float) &&
            speedProperty.CanRead &&
            speedProperty.CanWrite)
        {
            originalSpeed = (float)speedProperty.GetValue(enemyMoveComponent);
            hasOriginalSpeed = true;
        }
    }

    private void ApplySpeedMultiplier(float speedMultiplier)
    {
        if (enemyMoveComponent == null || !hasOriginalSpeed)
        {
            CacheMoveSpeed();
        }

        if (enemyMoveComponent == null || !hasOriginalSpeed)
            return;

        float newSpeed = originalSpeed * speedMultiplier;

        if (speedField != null)
        {
            speedField.SetValue(enemyMoveComponent, newSpeed);
            return;
        }

        if (speedProperty != null)
        {
            speedProperty.SetValue(enemyMoveComponent, newSpeed);
        }
    }

    private void RestoreSpeed()
    {
        if (enemyMoveComponent == null || !hasOriginalSpeed)
            return;

        if (speedField != null)
        {
            speedField.SetValue(enemyMoveComponent, originalSpeed);
            return;
        }

        if (speedProperty != null)
        {
            speedProperty.SetValue(enemyMoveComponent, originalSpeed);
        }
    }

    private void OnDestroy()
    {
        if (frenzyEffect != null)
        {
            Destroy(frenzyEffect);
        }
    }

    private static void PrepareSprites()
    {
        if (circleSprite == null)
            circleSprite = CreateCircleSprite(96);

        if (ringSprite == null)
            ringSprite = CreateRingSprite(96);

        if (flameSprite == null)
            flameSprite = CreateFlameSprite(96);
    }

    private static Sprite CreateCircleSprite(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(1f, 1f, 1f, 0f);
        Color white = Color.white;

        float center = (size - 1) / 2f;
        float radius = center * 0.9f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                if (distance <= radius)
                {
                    float alpha = 1f - Mathf.Clamp01(distance / radius);
                    texture.SetPixel(x, y, new Color(white.r, white.g, white.b, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, clear);
                }
            }
        }

        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite CreateRingSprite(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(1f, 1f, 1f, 0f);
        Color white = Color.white;

        float center = (size - 1) / 2f;
        float outerRadius = center * 0.9f;
        float innerRadius = center * 0.62f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                if (distance <= outerRadius && distance >= innerRadius)
                {
                    texture.SetPixel(x, y, white);
                }
                else
                {
                    texture.SetPixel(x, y, clear);
                }
            }
        }

        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite CreateFlameSprite(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(1f, 1f, 1f, 0f);
        Color white = Color.white;

        float cx = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            float normalizedY = (float)y / (size - 1);
            float centerOffset = Mathf.Sin(normalizedY * Mathf.PI * 2.2f) * size * 0.07f;
            float currentCenterX = cx + centerOffset;

            float width = Mathf.Lerp(size * 0.36f, size * 0.02f, normalizedY);
            width *= 1f + Mathf.Sin(normalizedY * Mathf.PI * 3f) * 0.2f;

            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - currentCenterX);

                if (dx <= width)
                {
                    float alpha = 1f - dx / width;
                    alpha *= Mathf.SmoothStep(0f, 1f, normalizedY);
                    alpha = Mathf.Clamp01(alpha);

                    texture.SetPixel(x, y, new Color(white.r, white.g, white.b, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, clear);
                }
            }
        }

        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0f), size);
    }
}

public class FrenzyFlameWorldEffect : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    public Transform redGlow;
    public Transform groundRing;
    public Transform[] flames;

    private Vector3 redGlowScale;
    private Vector3 groundRingScale;
    private Vector3[] flameStartPositions;
    private Vector3[] flameStartScales;
    private SpriteRenderer[] flameRenderers;
    private Color[] flameOriginalColors;

    private bool initialized = false;

    public void Initialize()
    {
        if (redGlow != null)
            redGlowScale = redGlow.localScale;

        if (groundRing != null)
            groundRingScale = groundRing.localScale;

        if (flames != null)
        {
            flameStartPositions = new Vector3[flames.Length];
            flameStartScales = new Vector3[flames.Length];
            flameRenderers = new SpriteRenderer[flames.Length];
            flameOriginalColors = new Color[flames.Length];

            for (int i = 0; i < flames.Length; i++)
            {
                if (flames[i] == null) continue;

                flameStartPositions[i] = flames[i].localPosition;
                flameStartScales[i] = flames[i].localScale;

                flameRenderers[i] = flames[i].GetComponent<SpriteRenderer>();

                if (flameRenderers[i] != null)
                    flameOriginalColors[i] = flameRenderers[i].color;
            }
        }

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            Initialize();

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 pos = target.position + offset;
        pos.z = target.position.z - 0.1f;
        transform.position = pos;

        float t = Time.time;

        if (redGlow != null)
        {
            float pulse = 1f + Mathf.Sin(t * 5f) * 0.12f;
            redGlow.localScale = redGlowScale * pulse;
        }

        if (groundRing != null)
        {
            groundRing.Rotate(0f, 0f, 150f * Time.deltaTime);

            float pulse = 1f + Mathf.Sin(t * 6f) * 0.1f;
            groundRing.localScale = groundRingScale * pulse;
        }

        if (flames == null) return;

        for (int i = 0; i < flames.Length; i++)
        {
            if (flames[i] == null) continue;

            float cycle = Mathf.Repeat(t * 1.8f + i * 0.18f, 1f);

            float riseY = Mathf.Lerp(0f, 0.75f, cycle);
            float sideWave = Mathf.Sin(t * 5f + i * 1.3f) * 0.04f;

            flames[i].localPosition = flameStartPositions[i] + new Vector3(sideWave, riseY, 0f);

            float scale = Mathf.Lerp(0.75f, 1.25f, cycle);
            flames[i].localScale = flameStartScales[i] * scale;

            if (flameRenderers != null && flameRenderers[i] != null)
            {
                Color color = flameOriginalColors[i];

                if (cycle < 0.15f)
                {
                    color.a *= Mathf.Lerp(0f, 1f, cycle / 0.15f);
                }
                else if (cycle > 0.7f)
                {
                    color.a *= Mathf.Lerp(1f, 0f, (cycle - 0.7f) / 0.3f);
                }

                flameRenderers[i].color = color;
            }
        }
    }
}