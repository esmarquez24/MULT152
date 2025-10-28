using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class RestartUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button restartButtonLegacy;
    [SerializeField] private Button quitButtonLegacy;
    [SerializeField] private TMP_Text restartTMPButtonText;
    [SerializeField] private TMP_Text quitTMPButtonText;

    public static RestartUI Instance { get; private set; }

    // Remember cursor state so we can restore if needed
    CursorLockMode _prevLock;
    bool _prevVisible;

    void Awake()
    {
        Instance = this;

        if (panel) panel.SetActive(false);
        AutoBindButtons();
        EnsureEventSystem();
    }

    // ===== Public API =========================================================
    public void ShowPanel()
    {
        // Make sure the Canvas is active
        var canvas = GetComponentInParent<Canvas>(true);
        if (canvas && !canvas.enabled) canvas.enabled = true;
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        // Try to auto-find a panel if not assigned
        if (!panel)
        {
            var rects = GetComponentsInChildren<RectTransform>(true);
            foreach (var r in rects)
            {
                var n = r.gameObject.name.ToLower();
                if (n.Contains("panel") || n.Contains("restart"))
                {
                    panel = r.gameObject;
                    break;
                }
            }
        }

        if (panel) panel.SetActive(true);
        else Debug.LogWarning("[RestartUI] No panel assigned/found.");

        // --- CRITICAL: Unlock & show the cursor for mouse interaction
        _prevLock = Cursor.lockState;
        _prevVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Make sure raycasts are accepted
        var cg = panel ? panel.GetComponent<CanvasGroup>() : null;
        if (cg)
        {
            cg.blocksRaycasts = true;
            cg.interactable = true;
            cg.alpha = 1f;
        }

        // Focus a default button so keyboard/gamepad can navigate instantly
        var es = EventSystem.current;
        if (es)
        {
            GameObject focus = null;
            if (restartButtonLegacy) focus = restartButtonLegacy.gameObject;
            else
            {
                // fallback: first selectable under panel
                var selectable = panel ? panel.GetComponentInChildren<Selectable>(true) : null;
                if (selectable) focus = selectable.gameObject;
            }
            if (focus) es.SetSelectedGameObject(focus);
        }
    }

    public void Restart()
    {
        RestoreCursor();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

    // ===== Helpers ============================================================
    void RestoreCursor()
    {
        Cursor.lockState = _prevLock;
        Cursor.visible = _prevVisible;
    }

    void AutoBindButtons()
    {
        // Legacy Buttons
        if (!restartButtonLegacy || !quitButtonLegacy)
        {
            var buttons = GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                var n = btn.name.ToLower();
                if (!restartButtonLegacy && n.Contains("restart")) restartButtonLegacy = btn;
                if (!quitButtonLegacy && (n.Contains("quit") || n.Contains("exit"))) quitButtonLegacy = btn;
            }
        }
        if (restartButtonLegacy)
        {
            restartButtonLegacy.onClick.RemoveAllListeners();
            restartButtonLegacy.onClick.AddListener(Restart);
        }
        if (quitButtonLegacy)
        {
            quitButtonLegacy.onClick.RemoveAllListeners();
            quitButtonLegacy.onClick.AddListener(QuitGame);
        }

        // TMP labels auto-detected (optional; purely cosmetic)
        if (!restartTMPButtonText || !quitTMPButtonText)
        {
            var texts = GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in texts)
            {
                var n = t.name.ToLower();
                if (!restartTMPButtonText && n.Contains("restart")) restartTMPButtonText = t;
                if (!quitTMPButtonText && (n.Contains("quit") || n.Contains("exit"))) quitTMPButtonText = t;
            }
        }
    }

    void EnsureEventSystem()
    {
        // In 6.2, either legacy StandaloneInputModule or InputSystemUIInputModule works.
        var es = EventSystem.current ?? FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include);
        if (!es)
        {
            var go = new GameObject("EventSystem");
            es = go.AddComponent<EventSystem>();
            // If you're on the old Input Manager:
            go.AddComponent<StandaloneInputModule>();
            // If you're on the new Input System, you can instead use:
            // go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }
}