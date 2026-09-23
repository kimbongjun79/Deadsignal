using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// 사운드 볼륨과 마우스 감도를 조절하고 PlayerPrefs에 저장하는 설정 메뉴
public class SettingsMenu : MonoBehaviour
{
    [Header("오디오")]
    [Tooltip("AudioManager가 사용하는 AudioMixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Tooltip("BGM 볼륨 슬라이더 (0~1)")]
    [SerializeField] private Slider bgmSlider;

    [Tooltip("SFX 볼륨 슬라이더 (0~1)")]
    [SerializeField] private Slider sfxSlider;

    [Header("마우스 감도")]
    [Tooltip("플레이어 컨트롤러 참조")]
    [SerializeField] private PlayerController player;

    [Tooltip("마우스 감도 슬라이더")]
    [SerializeField] private Slider mouseSensitivitySlider;

    [Tooltip("마우스 감도 최소값")]
    [SerializeField] private float minSensitivity = 0.5f;

    [Tooltip("마우스 감도 최대값")]
    [SerializeField] private float maxSensitivity = 10f;

    private void Start()
    {
        LoadSettings();

        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
    }

    // 저장된 설정을 불러와 슬라이더와 실제 값에 반영
    private void LoadSettings()
    {
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        float sensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 3f);

        bgmSlider.value = bgmVolume;
        sfxSlider.value = sfxVolume;
        mouseSensitivitySlider.minValue = minSensitivity;
        mouseSensitivitySlider.maxValue = maxSensitivity;
        mouseSensitivitySlider.value = sensitivity;

        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);
        SetMouseSensitivity(sensitivity);
    }

    // 슬라이더(0~1)를 데시벨로 변환해 믹서에 적용
    public void SetBGMVolume(float value)
    {
        float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
        audioMixer.SetFloat("BGMVolume", dB);
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
        audioMixer.SetFloat("SFXVolume", dB);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void SetMouseSensitivity(float value)
    {
        if (player != null)
            player.SetMouseSensitivity(value);
        PlayerPrefs.SetFloat("MouseSensitivity", value);
    }
}