using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageResultController : MonoBehaviour
{
    public static StageResultController Instance;

    [Header("Root")]
    public GameObject stageResultPanel;
    public CanvasGroup resultCanvasGroup;

    [Header("Button Panels")]
    public GameObject clearPanel;
    public GameObject failPanel;

    [Header("Result Image Parent")]
    public GameObject resultPanel;

    [Header("Existing Result Images")]
    public GameObject clearTitleObject;
    public GameObject failTitleObject;

    [Header("Clear Buttons")]
    public Button clearMainMenuButton;
    public Button nextStageButton;

    [Header("Fail Buttons")]
    public Button failMainMenuButton;
    public Button retryButton;

    [Header("Scene Names")]
    public string mainMenuSceneName = "StageSelectScene";
    public string nextStageSceneName = "";

    [Header("Options")]
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

        AutoFindObjectsIfMissing();

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
        if (isResultShown) return;

        isResultShown = true;
        ShowRootPanel();

        if (clearPanel != null)
            clearPanel.SetActive(true);

        if (failPanel != null)
            failPanel.SetActive(false);

        ShowResultImages(true);

        if (pauseGameWhenResult)
            Time.timeScale = 0f;

        Debug.Log("스테이지 클리어 UI 표시");
    }

    public void ShowFail()
    {
        if (isResultShown) return;

        isResultShown = true;
        ShowRootPanel();

        if (clearPanel != null)
            clearPanel.SetActive(false);

        if (failPanel != null)
            failPanel.SetActive(true);

        ShowResultImages(false);

        if (pauseGameWhenResult)
            Time.timeScale = 0f;

        Debug.Log("스테이지 실패 UI 표시");
    }

    public void ShowStageClear()
    {
        ShowClear();
    }

    public void ShowStageFail()
    {
        ShowFail();
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

        // StageResultPanel이 먼저 깔리고
        if (stageResultPanel != null)
            stageResultPanel.transform.SetAsLastSibling();
    }

    private void ShowResultImages(bool isClear)
    {
        // ResultPanel 부모도 반드시 켜야 함
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            ForceCanvasGroupVisible(resultPanel);

            // ResultPanel을 StageResultPanel보다 더 위에 보이게 함
            resultPanel.transform.SetAsLastSibling();
        }

        if (clearTitleObject != null)
        {
            clearTitleObject.SetActive(isClear);
            ForceGraphicVisible(clearTitleObject);
        }

        if (failTitleObject != null)
        {
            failTitleObject.SetActive(!isClear);
            ForceGraphicVisible(failTitleObject);
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

        if (clearTitleObject != null)
            clearTitleObject.SetActive(false);

        if (failTitleObject != null)
            failTitleObject.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    private void ForceGraphicVisible(GameObject target)
    {
        if (target == null) return;

        Graphic[] graphics = target.GetComponentsInChildren<Graphic>(true);

        foreach (Graphic graphic in graphics)
        {
            if (graphic == null) continue;

            graphic.enabled = true;

            Color color = graphic.color;
            color.a = 1f;
            graphic.color = color;

            // 결과 이미지는 버튼 클릭을 막으면 안 됨
            graphic.raycastTarget = false;
        }

        Image[] images = target.GetComponentsInChildren<Image>(true);

        foreach (Image image in images)
        {
            if (image == null) continue;

            image.enabled = true;

            Color color = image.color;
            color.a = 1f;
            image.color = color;

            image.raycastTarget = false;
        }
    }

    private void ForceCanvasGroupVisible(GameObject target)
    {
        if (target == null) return;

        CanvasGroup[] groups = target.GetComponentsInChildren<CanvasGroup>(true);

        foreach (CanvasGroup group in groups)
        {
            if (group == null) continue;

            group.alpha = 1f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
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

    private void AutoFindObjectsIfMissing()
    {
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null) return;

        Transform[] all = canvas.GetComponentsInChildren<Transform>(true);

        foreach (Transform t in all)
        {
            string n = t.name.ToLower();

            if (resultPanel == null && n == "resultpanel")
                resultPanel = t.gameObject;

            if (clearTitleObject == null &&
                (n.Contains("stageclear") || n.Contains("clearimage") || n.Contains("clear")))
            {
                if (t.GetComponent<Button>() == null)
                    clearTitleObject = t.gameObject;
            }

            if (failTitleObject == null &&
                (n.Contains("stagefail") || n.Contains("failimage") || n.Contains("fail")))
            {
                if (t.GetComponent<Button>() == null)
                    failTitleObject = t.gameObject;
            }
        }

        if (resultPanel == null && clearTitleObject != null)
        {
            resultPanel = clearTitleObject.transform.parent.gameObject;
        }
    }
}