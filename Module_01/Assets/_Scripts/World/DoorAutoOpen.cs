using System.Collections;
using UnityEngine; 
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))] 
public class DoorAutoOpen : MonoBehaviour
{
    [Header("Scene References")]
    public Transform doorLeaf; // assign DoorLeaf (rotates) 
    public Transform player; // assign Player transform (or auto-find by tag) 

[Header("Distances")] 
[Tooltip("Meters. Set to 1.0 for 1m, or 1.524 for ~5 feet.")] 
public float openDistanceMeters = 2.0f; 
[Tooltip("Hysteresis to avoid jitter (door won't close until beyond this).")] 
public float closeDistanceMeters = 2.25f; 
 
[Header("Motion")] 
public Vector3 openLocalEuler = new Vector3(0, 90, 0); // hinge open angle 
public float openSpeedDegPerSec = 180f;                 // rotation speed 
public float closeSpeedDegPerSec = 200f; 
 
[Header("Behavior")] 
public bool startsOpen = false; 
public bool stayOpenOnceOpened = false; // set true if you want one-time opening

public bool isLocked { get; private set; }

    [Header("Audio")] 
public AudioMixerGroup outputMixerGroup; 
public AudioClip sfxUnlock; 
public AudioClip sfxOpen; 
public AudioClip sfxClose; 
[Range(0f,1f)] public float volume = 1f; 
 
// --- internals --- 
Quaternion _rotClosed, _rotOpen, _targetRot; 
bool _isOpen, _isMoving; 
AudioSource _audio; 
 
void Reset() 
{ 
    // try auto-wire DoorLeaf by name 
    var t = transform.Find("DoorLeaf"); 
    if (t) doorLeaf = t; 
} 
 
void Awake() 
{ 
    if (!doorLeaf) Debug.LogWarning($"{name}: DoorLeaf not assigned."); 
    _audio = GetComponent<AudioSource>(); 
    if (outputMixerGroup) _audio.outputAudioMixerGroup = outputMixerGroup; 
    _audio.playOnAwake = false; 
    _audio.spatialBlend = 1f; _audio.dopplerLevel = 0f; 
 
    _rotClosed = doorLeaf ? doorLeaf.localRotation : Quaternion.identity; 
    _rotOpen = Quaternion.Euler(openLocalEuler); 
    _targetRot = startsOpen ? _rotOpen : _rotClosed; 
    _isOpen = startsOpen; 
 
    if (!player) 
    { 
        var tagged = GameObject.FindGameObjectWithTag("Player"); 
        if (tagged) player = tagged.transform; 
    } 
} 
 
void Update() 
{ 
    if (!doorLeaf || !player) return; 
 
    // Proximity logic (meters) 
    float d = Vector3.Distance(player.position, transform.position); 
 
    if (!_isOpen && !isLocked && d <= openDistanceMeters) 
        TryOpen(); 
    else if (_isOpen && !stayOpenOnceOpened && d >= closeDistanceMeters) 
        TryClose(); 
 
    // Smooth motion 
    float speed = _isOpen ? openSpeedDegPerSec : closeSpeedDegPerSec; 
    if (doorLeaf.localRotation != _targetRot) 
    { 
        doorLeaf.localRotation = Quaternion.RotateTowards(doorLeaf.localRotation, _targetRot, speed * Time.deltaTime); 
        _isMoving = true; 
    } 
    else if (_isMoving) 
    { 
        // reached target 
        _isMoving = false; 
    } 
}

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        if (locked) TryClose();
    }

    public void TryOpen() 
{
    if (isLocked || _isOpen) return;

    _isOpen = true; _targetRot = _rotOpen;

    // Play unlock -> open sequence 
    StartCoroutine(PlaySequence(sfxUnlock, sfxOpen, 0.05f)); 
} 
 
public void TryClose() 
{ 
    if (!_isOpen) return; 
    _isOpen = false;  
    _targetRot = _rotClosed; 
 
    PlayOneShot(sfxClose); 
} 
 
IEnumerator PlaySequence(AudioClip first, AudioClip second, float gap) 
{ 
    if (first) PlayOneShot(first); 
    if (second) 
    { 
        if (first) yield return new WaitForSeconds(gap); 
        PlayOneShot(second); 
    } 
} 
 
void PlayOneShot(AudioClip clip) 
{ 
    if (!clip) return; 
    _audio.PlayOneShot(clip, volume); 
}

    // gizmo to visualize open radius 
void OnDrawGizmosSelected() 
{ 
    Gizmos.color = Color.cyan; 
    Gizmos.DrawWireSphere(transform.position, openDistanceMeters); 
    Gizmos.color = Color.gray; 
    Gizmos.DrawWireSphere(transform.position, Mathf.Max(openDistanceMeters, closeDistanceMeters)); 
} 
  

}