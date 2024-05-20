using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class AudioMixerManager : MonoBehaviour
{   
    public Slider mv;
    public Slider sv;
    public Slider muv;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource AudioSrc;

    private void Start()
    {
        mv.value = PlayerPrefs.GetFloat("mvvolume", 1f);
        sv.value = PlayerPrefs.GetFloat("svvolume", 1f);
        muv.value = PlayerPrefs.GetFloat("muvvolume", 1f);
    }

    public void SetMasterVolume(float level)
    {
        audioMixer.SetFloat("masterVolume", Mathf.Log10(level) * 20f);
        PlayerPrefs.SetFloat("mvvolume", level);
    }

    public void SetSoundFXVolume(float level)
    {
        audioMixer.SetFloat("soundfxVolume", Mathf.Log10(level) * 20f);
        PlayerPrefs.SetFloat("svvolume", level);
    }

    public void SetMusicVolume(float level)
    {
        audioMixer.SetFloat("musicVolume", Mathf.Log10(level) * 20f);
        PlayerPrefs.SetFloat("muvvolume", level);
    }
}
