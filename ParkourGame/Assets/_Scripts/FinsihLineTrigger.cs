using UnityEngine;

public class FinsihLineTrigger : MonoBehaviour
{
    public GameManager gameManager;
    private void OnTriggerEnter()
    {
        gameManager.CompleteLevel();
    }
}
