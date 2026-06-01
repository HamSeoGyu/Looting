using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        musicSlider.value =
            PlayerPrefs.GetFloat("MusicVolume", 1f);

        sfxSlider.value =
            PlayerPrefs.GetFloat("SFXVolume", 1f);

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSfxVolume);
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);

        if (MenuBGMManager.Instance != null)
        {
            MenuBGMManager.Instance.SetVolume(volume);
        }
    }

    public void SetSfxVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}