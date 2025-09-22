using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsUI : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider sfxSlider;

    void Awake()
    {
        bgmSlider.value = PlayerPrefs.GetFloat("BGM", 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFX", 0.5f);

        bgmSlider.onValueChanged.AddListener(SoundManager.SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SoundManager.SetSFXVolume);
    }

    public void OnClickBack()
    {
        PlayerPrefs.Save();
    }
}