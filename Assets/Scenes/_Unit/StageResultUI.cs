using UnityEngine;
using UnityEngine.UI;

public class StageResultUI : MonoBehaviour
{
    public static StageResultUI Instance;

    [Header("Root")]
    public GameObject resultPanel;

    [Header("Images")]
    public GameObject stageClearImage;
    public GameObject stageFailImage;

    [Header("Optional")]
    public bool freezeTimeWhenResultShown = true;

    private bool isResultShown = false;

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
        if (isResultShown) return;
        isResultShown = true;

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (stageClearImage != null)
            stageClearImage.SetActive(true);

        if (stageFailImage != null)
            stageFailImage.SetActive(false);

        if (freezeTimeWhenResultShown)
            Time.timeScale = 0f;

        Debug.Log("STAGE CLEAR 표시");
    }

    public void ShowStageFail()
    {
        if (isResultShown) return;
        isResultShown = true;

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (stageClearImage != null)
            stageClearImage.SetActive(false);

        if (stageFailImage != null)
            stageFailImage.SetActive(true);

        if (freezeTimeWhenResultShown)
            Time.timeScale = 0f;

        Debug.Log("STAGE FAIL 표시");
    }

    public void HideAll()
    {
        isResultShown = false;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (stageClearImage != null)
            stageClearImage.SetActive(false);

        if (stageFailImage != null)
            stageFailImage.SetActive(false);

        Time.timeScale = 1f;
    }

    public bool IsResultShown()
    {
        return isResultShown;
    }
}