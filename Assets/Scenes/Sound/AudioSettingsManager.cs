using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
{
    if (musicSlider != null)
    {
        musicSlider.value =
            PlayerPrefs.GetFloat("MusicVolume", 1f);

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
    }

    if (sfxSlider != null)
    {
        sfxSlider.value =
            PlayerPrefs.GetFloat("SFXVolume", 1f);

        sfxSlider.onValueChanged.AddListener(SetSfxVolume);
    }
}

    public void SetMusicVolume(float volume)
{
    PlayerPrefs.SetFloat("MusicVolume", volume);
    PlayerPrefs.Save();

    if (MenuBGMManager.Instance != null)
    {
        MenuBGMManager.Instance.SetVolume(volume);
    }

    if (BGMManager.Instance != null)
    {
        BGMManager.Instance.SetVolume(volume);
    }
}

    public void SetSfxVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}