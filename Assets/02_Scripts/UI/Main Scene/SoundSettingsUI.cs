using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsUI : MonoBehaviour
{
    public Slider bgmSlider;            // UI �����̴�
    public Slider sfxSlider;

    void Start()
    {

        // �����̴� �ʱⰪ ����
        bgmSlider.value = PlayerPrefs.GetFloat("BGM", 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFX", 0.5f);

        // �̺�Ʈ ����
        bgmSlider.onValueChanged.AddListener(SoundManager.SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SoundManager.SetSFXVolume);
    }
}
