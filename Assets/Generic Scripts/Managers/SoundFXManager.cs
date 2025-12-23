using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Script Singleton per gestire la riproduzione di suoni
public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;
    [SerializeField] private AudioSource soundFXObject;
    private AudioSource audioSource;

    private void Awake()
    {
        Instance ??= this;
    }

    public void PlaySound(AudioClip sound, Transform soundTransform, float volume)
    {
        audioSource = Instantiate(soundFXObject, soundTransform.position, Quaternion.identity);
        audioSource.clip = sound;
        audioSource.volume = volume;
        audioSource.Play();
        Destroy(audioSource.gameObject, audioSource.clip.length);
    }
}
