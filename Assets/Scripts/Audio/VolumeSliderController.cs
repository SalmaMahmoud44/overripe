using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderController : MonoBehaviour
{
    public enum SliderType { Music, SFX }

    [SerializeField] SliderType sliderType;
    [SerializeField] Slider slider;

    void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (AudioManager.Instance != null)
        {
            slider.value = sliderType == SliderType.Music
                ? AudioManager.Instance.MusicVolume
                : AudioManager.Instance.SFXVolume;
        }

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value)
    {
        if (AudioManager.Instance == null)
            return;

        if (sliderType == SliderType.Music)
            AudioManager.Instance.SetMusicVolume(value);
        else
            AudioManager.Instance.SetSFXVolume(value);
    }
}