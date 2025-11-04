using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CheckpointSingle : MonoBehaviour
{
    [Header("Collision")]
    public string playerTag = "Player";

    private GameCheckpoints gameCheckpoints;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Player>(out Player player))
        {
            gameCheckpoints.PlayerThroughCheckpoint(this);
        }
    }

    public void SetGameCheckpoints(GameCheckpoints gameCheckpoints)
    {
        this.gameCheckpoints = gameCheckpoints;
    }
}
