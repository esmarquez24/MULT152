using UnityEngine;
using UnityEngine.Audio;

public class MixerControl : MonoBehaviour
{
    public AudioMixer mixer;

    // Set in db: 0 = full, -80 = silent
    public void SetMusicVolume(float db) { mixer.SetFloat("MusicVol", db); }
    public void SetSFXVolume(float db) { mixer.SetFloat("SFXVol", db); }
    public void SetAmbienceVolume(float db) { mixer.SetFloat("AMbVol", db); }
    public void SetMusicLowpass(float cutoffHz) { mixer.SetFloat("MusicLPF", cutoffHz); }
}