using UnityEngine;

[CreateAssetMenu(fileName = "HealthConfig", menuName = "Scriptable Objects/HealthConfig")]
public class HealthConfig : ScriptableObject
{
    public int maxHealth = 100;
    public int startHealth = 100;
}