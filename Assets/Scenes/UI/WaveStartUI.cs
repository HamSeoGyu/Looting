using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaveStartUI : MonoBehaviour
{
    public static WaveStartUI Instance;

    [Header("UI")]
    public CanvasGroup canvasGroup;
    public Image waveImage;

    [Header("Wave Images")]
    public Sprite[] waveSprites;

    [Header("Animation")]
    public float fadeInTime = 0.25f;
    public float stayTime = 1.0f;
    public float fadeOutTime = 0.35f;

    public float startScale = 1.25f;
    public float normalScale = 1.0f;

    private Coroutine showCoroutine;

    private void Awake()
    {
        Instance = this;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        HideInstant();
    }

    public IEnumerator ShowWave(int waveNumber)
    {
        if (showCoroutine != null)
            StopCoroutine(showCoroutine);

        showCoroutine = StartCoroutine(ShowWaveRoutine(waveNumber));

        yield return showCoroutine;
    }

    private IEnumerator ShowWaveRoutine(int waveNumber)
    {
        SetWaveSprite(waveNumber);

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (waveImage != null)
            waveImage.transform.localScale = Vector3.one * startScale;

        float timer = 0f;

        // 등장
        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;

            float t = timer / fadeInTime;
            t = Mathf.SmoothStep(0f, 1f, t);

            canvasGroup.alpha = t;

            if (waveImage != null)
            {
                float scale = Mathf.Lerp(startScale, normalScale, t);
                waveImage.transform.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        canvasGroup.alpha = 1f;

        if (waveImage != null)
            waveImage.transform.localScale = Vector3.one * normalScale;

        // 유지
        yield return new WaitForSeconds(stayTime);

        timer = 0f;

        // 사라짐
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;

            float t = timer / fadeOutTime;
            t = Mathf.SmoothStep(0f, 1f, t);

            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        HideInstant();
        showCoroutine = null;
    }

    private void SetWaveSprite(int waveNumber)
    {
        if (waveImage == null) return;
        if (waveSprites == null || waveSprites.Length == 0) return;

        int index = waveNumber - 1;

        if (index < 0 || index >= waveSprites.Length)
        {
            Debug.LogWarning("해당 웨이브 이미지가 없습니다. waveNumber: " + waveNumber);
            return;
        }

        waveImage.sprite = waveSprites[index];
        waveImage.SetNativeSize();
    }

    private void HideInstant()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}