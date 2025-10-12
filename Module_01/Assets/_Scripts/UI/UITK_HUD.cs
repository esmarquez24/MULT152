using UnityEngine;
using UnityEngine.UIElements;

public class UITK_HUD : MonoBehaviour
{
    public UIDocument doc;
    ProgressBar hp; Label ammo;

    void Awake(){
        var root = doc.rootVisualElement;
        hp   = root.Q<ProgressBar>("HpBar");
        ammo = root.Q<Label>("AmmoLabel");
    }

    public void SetHP(int cur,int max){ if (hp!=null){ hp.highValue = max; hp.value = cur; hp.title = $"HP {cur}/{max}"; } }
    public void SetAmmo(int cur,int mag,int res){ if (ammo!=null) ammo.text = $"{cur}/{mag} ({res})"; }
}