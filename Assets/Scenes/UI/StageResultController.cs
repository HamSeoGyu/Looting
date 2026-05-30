using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageResultController : MonoBehaviour
{
    public static StageResultController Instance;

    [Header("Root Panel")]
    public GameObject stageResultPanel;
    public CanvasGroup resultCanvasGroup;

    [Header("Panels")]
    public GameObject clearPanel;
    public GameObject failPanel;

    [Header("Clear Buttons")]
    public Button clearMainMenuButton;
    public Button nextStageButton;

    [Header("Fail Buttons")]
    public Button failMainMenuButton;
    public Button retryButton;

    [Header("Scene Names")]
    public string mainMenuSceneName = "StageSelectScene";
    public string nextStageSceneName = "";

    [Header("Option")]
    public bool pauseGameWhenResult = true;
    public bool debugTestKey = true;

    private bool isResultShown = false;

    private void Awake()
    {
        Instance = this;

        if (stageResultPanel == null)
            stageResultPanel = gameObject;

        stageResultPanel.SetActive(true);

        if (resultCanvasGroup == null)
            resultCanvasGroup = stageResultPanel.GetComponent<CanvasGroup>();

        if (resultCanvasGroup == null)
            resultCanvasGroup = stageResultPanel.AddComponent<CanvasGroup>();

        HidePanels();

        if (clearMainMenuButton != null)
            clearMainMenuButton.onClick.AddListener(GoMainMenu);

        if (failMainMenuButton != null)
            failMainMenuButton.onClick.AddListener(GoMainMenu);

        if (nextStageButton != null)
            nextStageButton.onClick.AddListener(GoNextStage);

        if (retryButton != null)
            retryButton.onClick.AddListener(RetryStage);

        Debug.Log("StageResultController 준비 완료");
    }

    private void Update()
    {
        if (!debugTestKey) return;

        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("F1 테스트: 클리어 UI 표시");
            ShowClear();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("F2 테스트: 실패 UI 표시");
            ShowFail();
        }
    }

    public void ShowClear()
    {
        Debug.Log("ShowClear 실행됨");

        if (isResultShown) return;

        isResultShown = true;

        ShowRootPanel();

        if (clearPanel != null)
            clearPanel.SetActive(true);

        if (failPanel != null)
            failPanel.SetActive(false);

        if (pauseGameWhenResult)
            Time.timeScale = 0f;
    }

    public void ShowFail()
    {
        Debug.Log("ShowFail 실행됨");

        if (isResultShown) return;

        isResultShown = true;

        ShowRootPanel();

        if (clearPanel != null)
            clearPanel.SetActive(false);

        if (failPanel != null)
            failPanel.SetActive(true);

        if (pauseGameWhenResult)
            Time.timeScale = 0f;
    }

    private void ShowRootPanel()
    {
        if (stageResultPanel != null)
            stageResultPanel.SetActive(true);

        if (resultCanvasGroup != null)
        {
            resultCanvasGroup.alpha = 1f;
            resultCanvasGroup.interactable = true;
            resultCanvasGroup.blocksRaycasts = true;
        }
    }

    private void HidePanels()
    {
        isResultShown = false;

        if (stageResultPanel != null)
            stageResultPanel.SetActive(true);

        if (resultCanvasGroup != null)
        {
            resultCanvasGroup.alpha = 0f;
            resultCanvasGroup.interactable = false;
            resultCanvasGroup.blocksRaycasts = false;
        }

        if (clearPanel != null)
            clearPanel.SetActive(false);

        if (failPanel != null)
            failPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    private void GoMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void RetryStage()
    {
        Time.timeScale = 1f;

        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    private void GoNextStage()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(nextStageSceneName))
        {
            SceneManager.LoadScene(nextStageSceneName);
            return;
        }

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.LogWarning("다음 스테이지가 Build Settings에 없습니다.");
        }
    }
}