using UnityEngine;
using UnityEngine.Audio;

public class Snapshot : MonoBehaviour
{
    public AudioMixerSnapshot calm;
    public AudioMixerSnapshot stress;
    public float fadeTime = 0.6f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) calm.TransitionTo(fadeTime);
        if (Input.GetKeyDown(KeyCode.Alpha2)) stress.TransitionTo(fadeTime);
    }
}