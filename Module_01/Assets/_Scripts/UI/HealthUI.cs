using UnityEngine;
using UnityEngine.UI; // for Slider
#if TMP_PRESENT
using TMPro;
#endif

public class HealthUI : MonoBehaviour
{
    [SerializeField] private HealthComponent target;
    [SerializeField] private Slider bar;
    [SerializeField] private UnityEngine.UI.Text legacyLabel;
#if TMP_PRESENT
    [SerializeField] private TextMeshProUGUI tmpLabel;
#endif

    private void OnEnable()
    {
        if (target == null) return;
        target.OnHealthChanged += HandleChanged;
    }

    private void OnDisable()
    {
        if (target == null) return;
        target.OnHealthChanged -= HandleChanged;
    }

    private void HandleChanged(int current, int max)
    {
        if (bar != null) { bar.minValue = 0; bar.maxValue = max; bar.value = current; }
        if (legacyLabel != null) legacyLabel.text = $"{current}/{max}";
#if TMP_PRESENT
        if (tmpLabel != null) tmpLabel.text = $"{current}/{max}";
#endif
    }
}