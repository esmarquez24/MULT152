using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class WinUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button quitButtonLegacy;     // optional auto-bind
    [SerializeField] private Button restartButtonLegacy;  // optional (if you add restart)
    [SerializeField] private TMP_Text titleTMP;           // optional cosmetic

    public static WinUI Instance { get; private set; }

    CursorLockMode _prevLock;
    bool _prevVisible;

    private void Awake()
    {
        Instance = this;
        if (panel) panel.SetActive(false);
        AutoBindButtons();
        EnsureEventSystem();
    }

    public void Show()
    {
        // Make sure Canvas and this GO are active
        var canvas = GetComponentInParent<Canvas>(true);
        if (canvas && !canvas.enabled) canvas.enabled = true;
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        if (!panel)
        {
            // Try to find a child named Panel/Win
            var rects = GetComponentsInChildren<RectTransform>(true);
            foreach (var r in rects)
            {
                var n = r.gameObject.name.ToLower();
                if (n.Contains("panel") || n.Contains("win"))
                {
                    panel = r.gameObject;
                    break;
                }
            }
        }

        if (panel) panel.SetActive(true);
        else Debug.LogWarning("[WinUI] No panel assigned/found.");

        // Pause world and free the cursor
        Time.timeScale = 0f;
        _prevLock = Cursor.lockState;
        _prevVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Focus a default button for keyboard/gamepad
        var es = EventSystem.current;
        if (es)
        {
            GameObject focus = null;
            if (quitButtonLegacy) focus = quitButtonLegacy.gameObject;
            else
            {
                var selectable = panel ? panel.GetComponentInChildren<Selectable>(true) : null;
                if (selectable) focus = selectable.gameObject;
            }
            if (focus) es.SetSelectedGameObject(focus);
        }
    }

    public void QuitGame()
    {
        RestoreCursor();
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Optional: restart current level (wire a button if you added one)
    public void Restart()
    {
        RestoreCursor();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel(string sceneName)
    {
        RestoreCursor();
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    void RestoreCursor()
    {
        Cursor.lockState = _prevLock;
        Cursor.visible = _prevVisible;
    }

    void AutoBindButtons()
    {
        if (!quitButtonLegacy || !restartButtonLegacy)
        {
            var buttons = GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                var n = btn.name.ToLower();
                if (!quitButtonLegacy && (n.Contains("quit") || n.Contains("exit"))) quitButtonLegacy = btn;
                if (!restartButtonLegacy && n.Contains("restart")) restartButtonLegacy = btn;
            }
        }
        if (quitButtonLegacy)
        {
            quitButtonLegacy.onClick.RemoveAllListeners();
            quitButtonLegacy.onClick.AddListener(QuitGame);
        }
        if (restartButtonLegacy)
        {
            restartButtonLegacy.onClick.RemoveAllListeners();
            restartButtonLegacy.onClick.AddListener(Restart);
        }
    }

    void EnsureEventSystem()
    {
        var es = EventSystem.current ?? FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include);
        if (!es)
        {
            var go = new GameObject("EventSystem");
            es = go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>(); // or InputSystemUIInputModule if using new input system only
        }
    }
}