using System.Collections.Specialized;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform playerTarget; // Assign your player's transform in the Inspector 
    public float moveSpeed = 5f;

    void Update()
    {
        if (playerTarget != null)
        {
            // Calculate direction towards the player 
            Vector3 direction = (playerTarget.position - transform.position).normalized;

            // Move the enemy towards the player 
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

            // Optional: Make the enemy face the player 
            transform.LookAt(playerTarget);
        }
        else
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}