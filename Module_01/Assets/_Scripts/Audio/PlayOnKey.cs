using UnityEngine;

public class PlayOnKey : MonoBehaviour
{
    public AudioSource src;
    public KeyCode key = KeyCode.F;

    void Update()
    {
        if (Input.GetKeyDown(key)) src?.Play();
    }
}