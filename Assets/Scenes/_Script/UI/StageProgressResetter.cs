using UnityEngine;

public class StageProgressResetter : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.DeleteKey("Stage2Unlocked");
        PlayerPrefs.DeleteKey("Stage3Unlocked");
        PlayerPrefs.Save();
    }
}