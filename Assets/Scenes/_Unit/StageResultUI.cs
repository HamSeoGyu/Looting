using UnityEngine;

public class StageResultUI : MonoBehaviour
{
    public static StageResultUI Instance;

    [Header("Root")]
    public GameObject resultPanel;

    [Header("Images")]
    public GameObject stageClearImage;
    public GameObject stageFailImage;

    [Header("Option")]
    public bool freezeTimeWhenShown = true;

    private bool resultShown = false;

    public bool IsResultShown => resultShown;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        HideAll();
    }

    public void ShowStageClear()
    {
        if (resultShown) return;
        resultShown = true;

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (stageClearImage != null)
            stageClearImage.SetActive(true);

        if (stageFailImage != null)
            stageFailImage.SetActive(false);

        if (freezeTimeWhenShown)
            Time.timeScale = 0f;

        Debug.Log("STAGE CLEAR 표시");
    }

    public void ShowStageFail()
    {
        if (resultShown) return;
        resultShown = true;

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (stageClearImage != null)
            stageClearImage.SetActive(false);

        if (stageFailImage != null)
            stageFailImage.SetActive(true);

        if (freezeTimeWhenShown)
            Time.timeScale = 0f;

        Debug.Log("STAGE FAIL 표시");
    }

    public void HideAll()
    {
        resultShown = false;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (stageClearImage != null)
            stageClearImage.SetActive(false);

        if (stageFailImage != null)
            stageFailImage.SetActive(false);

        Time.timeScale = 1f;
    }
}