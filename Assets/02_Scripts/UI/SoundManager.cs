using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSources;

    [Header("Mixer Parameters")]
    public string bgmParameter = "BGM_Volume";
    public string sfxParameter = "SFX_Volume";


    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 유지

        }
        else
        {
            Destroy(gameObject);
        }
    }

    // BGM
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (bgmSource == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    //// 효과음 착수 시 나는 소리 (아직 안함)
    public void PlaySFX(AudioClip clip)
    {
        if (bgmSource == null) return;
        bgmSource.clip = clip;
        bgmSource.Play();

    }

    // BGM 볼륨 설정
    public void SetBGMVolume(float volume)
    {
        if (volume <= 0.0001f)
            audioMixer.SetFloat(sfxParameter, -80);
        else
            audioMixer.SetFloat(sfxParameter, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFX", volume);
    }

    // SFX 볼륨 설정
    public void SetSFXVolume(float volume)
    {
        if (volume <= 0.0001f)
            audioMixer.SetFloat(sfxParameter, -80);
        else
            audioMixer.SetFloat(sfxParameter, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFX", volume);
    }
}
