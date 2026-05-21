using UnityEngine;
using TMPro;

public class StageMonsterManager : MonoBehaviour
{
    public static StageMonsterManager Instance;

    [Header("Monster Count")]
    public int maxMonsterCount = 30;
    public TextMeshProUGUI monsterCountText;   // MonsterCountText 하나만 사용

    [Header("Fail UI")]
    public GameObject failPanel;

    private bool isStageFailed = false;
    private int aliveCount = 0;

    public bool IsStageFailed => isStageFailed;
    public int AliveCount => aliveCount;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (monsterCountText == null)
        {
            Debug.LogError("StageMonsterManager: monsterCountText가 연결되지 않았습니다.");
        }

        if (failPanel != null)
            failPanel.SetActive(false);

        RefreshAliveCount();
        UpdateMonsterCountUI();
    }

    void Update()
    {
        RefreshAliveCount();
        UpdateMonsterCountUI();

        if (!isStageFailed && aliveCount > maxMonsterCount)
        {
            FailStage();
        }
    }

    void RefreshAliveCount()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        aliveCount = enemies.Length;
    }

    void UpdateMonsterCountUI()
    {
        if (monsterCountText != null)
        {
            monsterCountText.text = aliveCount.ToString() + " / " + maxMonsterCount.ToString();
        }
    }

    void FailStage()
    {
        if (isStageFailed) return;

        isStageFailed = true;

        if (failPanel != null)
            failPanel.SetActive(true);

        Time.timeScale = 0f;
        Debug.Log("스테이지 실패");
    }

    // 기존 코드와 호환용
    public void RegisterEnemy(GameObject enemy)
    {
        RefreshAliveCount();
        UpdateMonsterCountUI();
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        RefreshAliveCount();
        UpdateMonsterCountUI();
    }
}