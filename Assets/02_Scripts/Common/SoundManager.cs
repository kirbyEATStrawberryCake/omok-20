using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : Singleton<SoundManager>
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;

    [SerializeField] private AudioSource sfxSource;

    [Header("Mixer Parameters")]
    [SerializeField] private string bgmParameter = "BGM_Volume";

    [SerializeField] private string sfxParameter = "SFX_Volume";

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] bgmClips;

    [SerializeField] private AudioClip[] sfxClips;


    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        LoadVolumeSettings();
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
    }

    // BGM
    public static void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (Instance?.bgmSource == null) return;

        Instance.bgmSource.clip = clip;
        Instance.bgmSource.loop = loop;
        Instance.bgmSource.Play();
    }

    public static void PlaySFX(AudioClip clip)
    {
        if (Instance?.sfxSource == null) return;

        Instance.sfxSource.PlayOneShot(clip);
    }

    public static void PlaySFX()
    {
        if (Instance?.sfxSource == null) return;

        Instance.sfxSource.PlayOneShot(Instance.sfxClips[0]);
    }

    // BGM 볼륨 설정
    public static void SetBGMVolume(float volume)
    {
        if (Instance?.audioMixer == null) return;
        volume = Mathf.Clamp01(volume);

        if (volume <= 0.0001f)
            Instance.audioMixer.SetFloat(Instance.bgmParameter, -80);
        else
            Instance.audioMixer.SetFloat(Instance.bgmParameter, Mathf.Log10(volume) * 20);

        PlayerPrefs.SetFloat("BGM", volume);
    }

    // SFX 볼륨 설정
    public static void SetSFXVolume(float volume)
    {
        if (Instance?.audioMixer == null) return;
        volume = Mathf.Clamp01(volume);

        if (volume <= 0.0001f)
            Instance.audioMixer.SetFloat(Instance.sfxParameter, -80);
        else
            Instance.audioMixer.SetFloat(Instance.sfxParameter, Mathf.Log10(volume) * 20);

        PlayerPrefs.SetFloat("SFX", volume);
    }

    public static void LoadVolumeSettings()
    {
        float bgmVolume = PlayerPrefs.GetFloat("BGM", 0.5f);
        float sfxVolume = PlayerPrefs.GetFloat("SFX", 0.5f);

        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);
    }
}