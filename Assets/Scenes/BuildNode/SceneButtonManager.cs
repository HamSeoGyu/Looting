using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonManager : MonoBehaviour
{
    public void GoToStageSelect()
    {
        SceneManager.LoadScene("StageSelectScene");
    }

    public void GoToSettings()
    {
        SceneManager.LoadScene("SettingsScene");
    }

    public void GoToStage1()
    {
        SceneManager.LoadScene("Stage1");
    }
    public void GoToStage2()
    {
    SceneManager.LoadScene("Stage2");
    }
    public void GoToStage3()
    {
    SceneManager.LoadScene("Stage3");
    }
    public void GoToStart()
    {
        SceneManager.LoadScene("StartScene");
    }
}