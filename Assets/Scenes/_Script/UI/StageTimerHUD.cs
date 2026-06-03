using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class StageTimerHUD : MonoBehaviour
{
    public static StageTimerHUD Instance;

    [Header("Text")]
    public TMP_Text bossTimeText;
    public TMP_Text nextWaveTimeText;

    [Header("Root")]
    public GameObject bossTimeRoot;
    public GameObject nextWaveTimeRoot;

    [Header("Idle Text")]
    public string bossIdleText = "00";
    public string nextWaveIdleText = "00";

    [Header("Color")]
    public Color normalColor = Color.white;
    public Color bossWarningColor = new Color(1f, 0.25f, 0.25f);
    public Color nextWaveWarningColor = new Color(1f, 0.9f, 0.25f);

    [Header("Warning Seconds")]
    public float bossWarningSeconds = 10f;
    public float nextWaveWarningSeconds = 5f;

    [Header("Boss Time Expired Event")]
    public UnityEvent onBossTimeExpired;

    private float bossRemainTime = 0f;
    private float nextWaveRemainTime = 0f;

    private bool bossTimerRunning = false;
    private bool nextWaveTimerRunning = false;

    private bool bossTimeExpiredCalled = false;

    // 보스 웨이브가 시작되기 전에도 제한시간 숫자를 보여주기 위한 값
    private bool bossPreviewEnabled = false;
    private int bossPreviewSeconds = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RefreshUI();
    }

    private void Update()
    {
        UpdateBossTimer();
        UpdateNextWaveTimer();
        RefreshUI();
    }

    private void UpdateBossTimer()
    {
        if (!bossTimerRunning)
            return;

        bossRemainTime -= Time.deltaTime;

        if (bossRemainTime <= 0f)
        {
            bossRemainTime = 0f;
            bossTimerRunning = false;
            bossPreviewEnabled = false;

            if (!bossTimeExpiredCalled)
            {
                bossTimeExpiredCalled = true;
                onBossTimeExpired?.Invoke();
            }
        }
    }

    private void UpdateNextWaveTimer()
    {
        if (!nextWaveTimerRunning)
            return;

        nextWaveRemainTime -= Time.deltaTime;

        if (nextWaveRemainTime <= 0f)
        {
            nextWaveRemainTime = 0f;
            nextWaveTimerRunning = false;
        }
    }

    public void SetBossLimitPreview(float seconds)
    {
        bossPreviewSeconds = Mathf.CeilToInt(Mathf.Max(0f, seconds));
        bossPreviewEnabled = true;
        bossTimerRunning = false;
        bossTimeExpiredCalled = false;

        if (bossTimeRoot != null)
            bossTimeRoot.SetActive(true);

        RefreshUI();
    }

    public void StartBossTimer(float seconds)
    {
        bossRemainTime = Mathf.Max(0f, seconds);
        bossTimerRunning = true;
        bossPreviewEnabled = false;
        bossTimeExpiredCalled = false;

        if (bossTimeRoot != null)
            bossTimeRoot.SetActive(true);

        RefreshUI();
    }

    public void StopBossTimer()
    {
        bossTimerRunning = false;
        bossRemainTime = 0f;
        bossPreviewEnabled = false;
        bossTimeExpiredCalled = true;

        RefreshUI();
    }

    public void ResetBossTimer()
    {
        bossTimerRunning = false;
        bossRemainTime = 0f;
        bossPreviewEnabled = false;
        bossTimeExpiredCalled = false;

        RefreshUI();
    }

    public void StartNextWaveTimer(float seconds)
    {
        nextWaveRemainTime = Mathf.Max(0f, seconds);
        nextWaveTimerRunning = true;

        if (nextWaveTimeRoot != null)
            nextWaveTimeRoot.SetActive(true);

        RefreshUI();
    }

    public void StopNextWaveTimer()
    {
        nextWaveTimerRunning = false;
        nextWaveRemainTime = 0f;

        RefreshUI();
    }

    public void ResetNextWaveTimer()
    {
        nextWaveTimerRunning = false;
        nextWaveRemainTime = 0f;

        RefreshUI();
    }

    private void RefreshUI()
    {
        RefreshBossText();
        RefreshNextWaveText();
    }

    private void RefreshBossText()
    {
        if (bossTimeText == null)
            return;

        if (bossTimerRunning)
        {
            int seconds = Mathf.CeilToInt(bossRemainTime);
            bossTimeText.text = seconds.ToString();

            if (seconds <= bossWarningSeconds)
                bossTimeText.color = bossWarningColor;
            else
                bossTimeText.color = normalColor;

            return;
        }

        if (bossPreviewEnabled)
        {
            bossTimeText.text = bossPreviewSeconds.ToString();
            bossTimeText.color = normalColor;
            return;
        }

        bossTimeText.text = bossIdleText;
        bossTimeText.color = normalColor;
    }

    private void RefreshNextWaveText()
    {
        if (nextWaveTimeText == null)
            return;

        if (nextWaveTimerRunning)
        {
            int seconds = Mathf.CeilToInt(nextWaveRemainTime);
            nextWaveTimeText.text = seconds.ToString();

            if (seconds <= nextWaveWarningSeconds)
                nextWaveTimeText.color = nextWaveWarningColor;
            else
                nextWaveTimeText.color = normalColor;

            return;
        }

        nextWaveTimeText.text = nextWaveIdleText;
        nextWaveTimeText.color = normalColor;
    }
}