using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenuManager : MonoBehaviour
{
    public TMP_Dropdown graphicsDeropdown;
    public Slider MasterVol, MusicVol, SFXVol;
    public AudioMixer audioMixer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        graphicsDeropdown.value = QualitySettings.GetQualityLevel();
        audioMixer.GetFloat("MasterVol", out float masterVol);
        MasterVol.value = masterVol;
        audioMixer.GetFloat("MusicVol", out float musicVol);
        MusicVol.value = musicVol;
        audioMixer.GetFloat("SFXVol", out float sfxVol);
        SFXVol.value = sfxVol;
    }

    public void ChangeGraphicsQuality()
    {
        QualitySettings.SetQualityLevel(graphicsDeropdown.value);
    }
    public void SetMasterVolume()
    {
        audioMixer.SetFloat("MasterVol", MasterVol.value);
    }
    public void SetMusicVolume()
    {
        audioMixer.SetFloat("MusicVol", MusicVol.value);
    }
    public void SetSFXVolume()
    {
        audioMixer.SetFloat("SFXVol", SFXVol.value);
    }
}
