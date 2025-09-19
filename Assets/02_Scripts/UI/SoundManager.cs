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
        // �̱��� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ���� �ٲ� ����

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

    //// ȿ���� ���� �� ���� �Ҹ� (���� ����)
    //public void PlaySFX(AudioClip clip)
    //{

    //}

    // BGM ���� ����
    public void SetBGMVolume(float volume)
    {
        audioMixer.SetFloat(bgmParameter, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("BGM", volume);
    }

    // SFX ���� ����
    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat(sfxParameter, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFX", volume);
    }
}
