using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsUI : MonoBehaviour
{
    public Slider bgmSlider;            // UI 슬라이더
    public Slider sfxSlider;

    void Start()
    {

        // 슬라이더 초기값 설정
        bgmSlider.value = PlayerPrefs.GetFloat("BGM", 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFX", 0.5f);

        // 이벤트 연결
        bgmSlider.onValueChanged.AddListener(SoundManager.Instance.SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SoundManager.Instance.SetSFXVolume);
    }
}
