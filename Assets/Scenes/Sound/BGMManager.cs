using System.Collections;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip normalWaveBgm;
    public AudioClip bossWaveBgm;

    [Header("Setting")]
    public float volume = 0.4f;
    public float fadeTime = 1f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        Instance = this;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = volume;
    }

    private void Start()
    {
        PlayNormalWaveBgm();
    }

    public void PlayNormalWaveBgm()
    {
        PlayBgm(normalWaveBgm);
    }

    public void PlayBossWaveBgm()
    {
        PlayBgm(bossWaveBgm);
    }

    private void PlayBgm(AudioClip clip)
    {
        if (clip == null) return;

        if (audioSource.clip == clip && audioSource.isPlaying)
            return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeChangeBgm(clip));
    }

    private IEnumerator FadeChangeBgm(AudioClip nextClip)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0f)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = nextClip;
        audioSource.Play();

        while (audioSource.volume < volume)
        {
            audioSource.volume += volume * Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSource.volume = volume;
    }
}