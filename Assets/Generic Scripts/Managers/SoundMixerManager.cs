using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.UI;

//Script che gestisce il main audio mixer e le impostazioni del volume
//Contiene i metodi per impostare il volume del master, della musica e degli effetti
public class SoundMixerManager : MonoBehaviour
{
    public static SoundMixerManager Instance { get; private set; }
    [SerializeField] private AudioMixer audioMixer;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    private void Start()
    {
        if (PlayerPrefs.HasKey("MasterVolume") && PlayerPrefs.HasKey("MusicVolume") && PlayerPrefs.HasKey("SoundFXVolume"))
            LoadVolume();

        else
        {
            Debug.Log("No PlayerPrefs found for volume settings, setting default values.");
            SetMasterVolume();
            SetMusicVolume();
            SetSFXVolume();
        }
    }
    #region Set Methods
    public void SetMasterVolume()
    {
        float volume = masterVolumeSlider.value;
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume()
    {
        float volume = musicVolumeSlider.value;
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume()
    {
        float volume = sfxVolumeSlider.value;
        audioMixer.SetFloat("SoundFXVolume", Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat("SoundFXVolume", volume);
    }
    #endregion
    #region LoadVolume
    private void LoadVolume()
    {
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SoundFXVolume");

        SetMasterVolume();
        SetMusicVolume();
        SetSFXVolume();
        Debug.Log("Volume settings loaded from PlayerPrefs.");
    }
    #endregion
}
