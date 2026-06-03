using UnityEngine;
using UnityEngine.UI;

public class StageUnlockManager : MonoBehaviour
{
    public Button stage2Button;
    public Button stage3Button;

    void Start()
    {
        stage2Button.interactable =
            StageProgressManager.unlockedStage >= 2;

        stage3Button.interactable =
            StageProgressManager.unlockedStage >= 3;
    }
}