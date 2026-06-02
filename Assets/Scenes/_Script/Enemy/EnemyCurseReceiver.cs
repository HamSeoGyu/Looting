using System.Collections;
using UnityEngine;

public class EnemyCurseReceiver : MonoBehaviour
{
    [Header("Curse")]
    [Range(0f, 2f)]
    public float currentExtraDamageTaken = 0f;

    [Header("Image Effect")]
    public Sprite curseEffectSprite;
    public float effectHeightOffset = 1.25f;
    public float effectWorldHeight = 0.85f;

    private GameObject curseEffect;
    private CurseImageWorldEffect curseFollower;
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

        if (curseFollower != null)
        {
            curseFollower.target = transform;
            curseFollower.offset = new Vector3(0f, effectHeightOffset, 0f);
            curseFollower.visible = true;
        }

        curseEffect.SetActive(true);
    }

    private void HideCurseEffect()
    {
        if (curseFollower != null)
        {
            curseFollower.visible = false;
        }

        if (curseEffect != null)
        {
            curseEffect.SetActive(false);
        }
    }

    private GameObject CreateCurseEffect()
    {
        int sortingLayerID = 0;
        int sortingOrder = 5200;

        SpriteRenderer baseRenderer = GetComponentInChildren<SpriteRenderer>();

        if (baseRenderer != null)
        {
            sortingLayerID = baseRenderer.sortingLayerID;
            sortingOrder = baseRenderer.sortingOrder + 800;
        }

        GameObject root = new GameObject("CurseImageEffect");
        root.layer = gameObject.layer;
        root.transform.position = transform.position + new Vector3(0f, effectHeightOffset, 0f);
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        curseFollower = root.AddComponent<CurseImageWorldEffect>();
        curseFollower.target = transform;
        curseFollower.offset = new Vector3(0f, effectHeightOffset, 0f);

        GameObject auraBack = CreateSpriteObject(
            root.transform,
            "CurseImageBackAura",
            curseEffectSprite,
            sortingLayerID,
            sortingOrder,
            new Color(0.55f, 0.1f, 1f, 0.35f)
        );

        GameObject main = CreateSpriteObject(
            root.transform,
            "CurseImageMain",
            curseEffectSprite,
            sortingLayerID,
            sortingOrder + 1,
            Color.white
        );

        GameObject auraFront = CreateSpriteObject(
            root.transform,
            "CurseImageFrontAura",
            curseEffectSprite,
            sortingLayerID,
            sortingOrder + 2,
            new Color(0.95f, 0.25f, 1f, 0.22f)
        );

        curseFollower.backAura = auraBack.transform;
        curseFollower.mainImage = main.transform;
        curseFollower.frontAura = auraFront.transform;

        curseFollower.backRenderer = auraBack.GetComponent<SpriteRenderer>();
        curseFollower.mainRenderer = main.GetComponent<SpriteRenderer>();
        curseFollower.frontRenderer = auraFront.GetComponent<SpriteRenderer>();

        curseFollower.effectWorldHeight = effectWorldHeight;
        curseFollower.Initialize();

        return root;
    }

    private GameObject CreateSpriteObject(
        Transform parent,
        string objectName,
        Sprite sprite,
        int sortingLayerID,
        int sortingOrder,
        Color color)
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingLayerID = sortingLayerID;
        renderer.sortingOrder = sortingOrder;

        return obj;
    }

    private void OnDestroy()
    {
        if (curseEffect != null)
        {
            Destroy(curseEffect);
        }
    }
}

public class CurseImageWorldEffect : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    public Transform backAura;
    public Transform mainImage;
    public Transform frontAura;

    public SpriteRenderer backRenderer;
    public SpriteRenderer mainRenderer;
    public SpriteRenderer frontRenderer;

    public float effectWorldHeight = 0.85f;
    public bool visible = true;

    private Vector3 backBaseScale;
    private Vector3 mainBaseScale;
    private Vector3 frontBaseScale;

    private Color backBaseColor;
    private Color mainBaseColor;
    private Color frontBaseColor;

    public void Initialize()
    {
        if (mainRenderer == null || mainRenderer.sprite == null)
            return;

        float spriteHeight = mainRenderer.sprite.bounds.size.y;

        if (spriteHeight <= 0f)
            spriteHeight = 1f;

        float baseScale = effectWorldHeight / spriteHeight;

        if (backAura != null)
        {
            backBaseScale = Vector3.one * baseScale * 1.25f;
            backAura.localScale = backBaseScale;
        }

        if (mainImage != null)
        {
            mainBaseScale = Vector3.one * baseScale;
            mainImage.localScale = mainBaseScale;
        }

        if (frontAura != null)
        {
            frontBaseScale = Vector3.one * baseScale * 1.08f;
            frontAura.localScale = frontBaseScale;
        }

        if (backRenderer != null)
            backBaseColor = backRenderer.color;

        if (mainRenderer != null)
            mainBaseColor = mainRenderer.color;

        if (frontRenderer != null)
            frontBaseColor = frontRenderer.color;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 pos = target.position + offset;
        pos.z = target.position.z - 0.1f;
        transform.position = pos;

        float t = Time.time;

        float floatY = Mathf.Sin(t * 2.6f) * 0.08f;
        float sideX = Mathf.Sin(t * 1.7f) * 0.025f;

        if (mainImage != null)
        {
            mainImage.localPosition = new Vector3(sideX, floatY, 0f);

            float pulse = 1f + Mathf.Sin(t * 4.2f) * 0.07f;
            mainImage.localScale = mainBaseScale * pulse;

            mainImage.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 2.2f) * 4f);
        }

        if (backAura != null)
        {
            backAura.localPosition = new Vector3(sideX * 0.4f, floatY * 0.4f, 0f);

            float pulse = 1f + Mathf.Sin(t * 3.2f) * 0.12f;
            backAura.localScale = backBaseScale * pulse;

            backAura.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 1.5f) * 6f);
        }

        if (frontAura != null)
        {
            frontAura.localPosition = new Vector3(sideX * -0.5f, floatY * 0.6f, 0f);

            float pulse = 1f + Mathf.Sin(t * 5.4f) * 0.09f;
            frontAura.localScale = frontBaseScale * pulse;

            frontAura.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 2.8f) * -5f);
        }

        ApplyAlpha(t);
    }

    private void ApplyAlpha(float t)
    {
        float mainAlpha = visible ? 1f : 0f;
        float auraAlpha = visible ? 1f : 0f;

        if (mainRenderer != null)
        {
            Color color = mainBaseColor;
            color.a *= mainAlpha * (0.88f + Mathf.Sin(t * 4f) * 0.08f);
            mainRenderer.color = color;
        }

        if (backRenderer != null)
        {
            Color color = backBaseColor;
            color.a *= auraAlpha * (0.65f + Mathf.Sin(t * 3f) * 0.16f);
            backRenderer.color = color;
        }

        if (frontRenderer != null)
        {
            Color color = frontBaseColor;
            color.a *= auraAlpha * (0.45f + Mathf.Sin(t * 5f) * 0.13f);
            frontRenderer.color = color;
        }
    }
}