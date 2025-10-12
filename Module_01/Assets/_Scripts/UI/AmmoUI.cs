using UnityEngine;
using TMPro;


public class AmmoUI : MonoBehaviour
{
    [SerializeField] int current = 30, mag = 30;
    [SerializeField] int reserves = 90;
    [SerializeField] UnityEngine.UI.Text legacyLabel;

    [SerializeField] TextMeshProUGUI tmpLabel;


    public void Set(int cur, int magSize, int res){ current=cur; mag=magSize; reserves=res; Refresh(); }

    void Start()
    {
        Refresh();
    }
    
    void Refresh()
    {
        string t = $"AMMO: {current}/{mag} ({reserves})";
        if (legacyLabel) legacyLabel.text = t;
        if (tmpLabel) tmpLabel.text = t;
    }
    // Call Set(...) from your weapon script events.
}