using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public GameObject completeLevelUI;
    public void CompleteLevel()
    {
        completeLevelUI.SetActive(true);
    }

    public GameObject gameOverUI;
    public void GameOver()
    {
        gameOverUI.SetActive(true);
    }
}
