using System.Collections;
using UnityEngine;

public class EnemyGreedReceiver : MonoBehaviour
{
    [Header("Greed State")]
    public bool isGreedMarked = false;
    public int currentBonusGold = 0;

    [Header("Image Effect")]
    public Sprite greedEffectSprite;
    public float effectHeightOffset = 1.1f;
    public float effectWorldHeight = 0.55f;

    private EnemyHealth enemyHealth;
    private GameObject greedEffect;
    private FloatingStatusImageEffect greedFollower;
    private Coroutine greedRoutine;

    private bool rewardGranted = false;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void LateUpdate()
    {
        if (!rewardGranted && enemyHealth != null && enemyHealth.IsDead && isGreedMarked)
        {
            GrantBonusGold();
        }
    }

    private void OnDestroy()
    {
        if (!rewardGranted && enemyHealth != null && enemyHealth.IsDead && isGreedMarked)
        {
            GrantBonusGold();
        }

        if (greedEffect != null)
        {
            Destroy(greedEffect);
        }
    }

    public void ApplyGreed(float duration, int bonusGold)
    {
        if (greedRoutine != null)
        {
            StopCoroutine(greedRoutine);
        }

        greedRoutine = StartCoroutine(GreedRoutine(duration, bonusGold));
    }

    private IEnumerator GreedRoutine(float duration, int bonusGold)
    {
        isGreedMarked = true;
        currentBonusGold = bonusGold;
        rewardGranted = false;

        ShowGreedEffect();

        float timer = 0f;
        while (timer < duration)
        {
            if (enemyHealth != null && enemyHealth.IsDead)
                yield break;

            timer += Time.deltaTime;
            yield return null;
        }

        isGreedMarked = false;
        currentBonusGold = 0;
        HideGreedEffect();

        greedRoutine = null;
    }

    private void ShowGreedEffect()
    {
        if (greedEffect == null)
        {
            greedEffect = CreateGreedEffect();
        }

        if (greedFollower != null)
        {
            greedFollower.target = transform;
            greedFollower.offset = new Vector3(0f, effectHeightOffset, 0f);
            greedFollower.visible = true;
        }

        greedEffect.SetActive(true);
    }

    private void HideGreedEffect()
    {
        if (greedFollower != null)
        {
            greedFollower.visible = false;
        }

        if (greedEffect != null)
        {
            greedEffect.SetActive(false);
        }
    }

    private GameObject CreateGreedEffect()
    {
        int sortingLayerID = 0;
        int sortingOrder = 5300;

        SpriteRenderer baseRenderer = GetComponentInChildren<SpriteRenderer>();
        if (baseRenderer != null)
        {
            sortingLayerID = baseRenderer.sortingLayerID;
            sortingOrder = baseRenderer.sortingOrder + 850;
        }

        GameObject root = new GameObject("GreedImageEffect");
        root.layer = gameObject.layer;
        root.transform.position = transform.position + new Vector3(0f, effectHeightOffset, 0f);
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        greedFollower = root.AddComponent<FloatingStatusImageEffect>();
        greedFollower.target = transform;
        greedFollower.offset = new Vector3(0f, effectHeightOffset, 0f);
        greedFollower.effectWorldHeight = effectWorldHeight;

        GameObject auraBack = CreateSpriteObject(
            root.transform,
            "GreedBackAura",
            greedEffectSprite,
            sortingLayerID,
            sortingOrder,
            new Color(1f, 0.8f, 0.15f, 0.35f)
        );

        GameObject main = CreateSpriteObject(
            root.transform,
            "GreedMainImage",
            greedEffectSprite,
            sortingLayerID,
            sortingOrder + 1,
            Color.white
        );

        GameObject auraFront = CreateSpriteObject(
            root.transform,
            "GreedFrontAura",
            greedEffectSprite,
            sortingLayerID,
            sortingOrder + 2,
            new Color(1f, 0.95f, 0.4f, 0.22f)
        );

        greedFollower.backAura = auraBack.transform;
        greedFollower.mainImage = main.transform;
        greedFollower.frontAura = auraFront.transform;

        greedFollower.backRenderer = auraBack.GetComponent<SpriteRenderer>();
        greedFollower.mainRenderer = main.GetComponent<SpriteRenderer>();
        greedFollower.frontRenderer = auraFront.GetComponent<SpriteRenderer>();

        greedFollower.Initialize();

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

    private void GrantBonusGold()
    {
        if (rewardGranted) return;

        rewardGranted = true;
        isGreedMarked = false;

        if (currentBonusGold <= 0) return;

        GameObject goldManager = GameObject.Find("GoldManager");
        if (goldManager != null)
        {
            goldManager.SendMessage("AddGold", currentBonusGold, SendMessageOptions.DontRequireReceiver);
        }

        currentBonusGold = 0;
        HideGreedEffect();
    }
}

public class FloatingStatusImageEffect : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    public Transform backAura;
    public Transform mainImage;
    public Transform frontAura;

    public SpriteRenderer backRenderer;
    public SpriteRenderer mainRenderer;
    public SpriteRenderer frontRenderer;

    public float effectWorldHeight = 0.55f;
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
            backBaseScale = Vector3.one * baseScale * 1.2f;
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

        if (backRenderer != null) backBaseColor = backRenderer.color;
        if (mainRenderer != null) mainBaseColor = mainRenderer.color;
        if (frontRenderer != null) frontBaseColor = frontRenderer.color;
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

        float floatY = Mathf.Sin(t * 2.6f) * 0.06f;
        float sideX = Mathf.Sin(t * 1.9f) * 0.02f;

        if (mainImage != null)
        {
            mainImage.localPosition = new Vector3(sideX, floatY, 0f);
            mainImage.localScale = mainBaseScale * (1f + Mathf.Sin(t * 4.1f) * 0.06f);
            mainImage.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 2.1f) * 3f);
        }

        if (backAura != null)
        {
            backAura.localPosition = new Vector3(sideX * 0.4f, floatY * 0.4f, 0f);
            backAura.localScale = backBaseScale * (1f + Mathf.Sin(t * 3.1f) * 0.12f);
            backAura.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 1.6f) * 5f);
        }

        if (frontAura != null)
        {
            frontAura.localPosition = new Vector3(sideX * -0.45f, floatY * 0.6f, 0f);
            frontAura.localScale = frontBaseScale * (1f + Mathf.Sin(t * 5.3f) * 0.08f);
            frontAura.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 2.8f) * -4f);
        }

        ApplyAlpha(t);
    }

    private void ApplyAlpha(float t)
    {
        if (mainRenderer != null)
        {
            Color color = mainBaseColor;
            color.a *= visible ? (0.88f + Mathf.Sin(t * 4f) * 0.08f) : 0f;
            mainRenderer.color = color;
        }

        if (backRenderer != null)
        {
            Color color = backBaseColor;
            color.a *= visible ? (0.62f + Mathf.Sin(t * 3f) * 0.16f) : 0f;
            backRenderer.color = color;
        }

        if (frontRenderer != null)
        {
            Color color = frontBaseColor;
            color.a *= visible ? (0.42f + Mathf.Sin(t * 5f) * 0.13f) : 0f;
            frontRenderer.color = color;
        }
    }
}