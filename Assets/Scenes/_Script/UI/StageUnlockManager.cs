using UnityEngine;
using UnityEngine.UI;

public class StageUnlockManager : MonoBehaviour
{
    public Button stage2Button;
    public Button stage3Button;

    void Start()
    {
        stage2Button.interactable =
            PlayerPrefs.GetInt("Stage2Unlocked", 0) == 1;

        stage3Button.interactable =
            PlayerPrefs.GetInt("Stage3Unlocked", 0) == 1;
    }
}