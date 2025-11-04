using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    public static GameOver Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        if (panel) panel.SetActive(false);
    }
    CursorLockMode _prevLock;
    bool _prevVisible;
    void RestoreCursor()
    {
        Cursor.lockState = _prevLock;
        Cursor.visible = _prevVisible;
    }
   
    public void Show()
    {
        if (panel) panel.SetActive(true);

        // Pause world and free the cursor
        Time.timeScale = 0f;
        _prevLock = Cursor.lockState;
        _prevVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Restart()
    {
        RestoreCursor();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnPlayAgainButton()
    {
        SceneManager.LoadScene("Scene_0");
    }
}
