using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingText
{
    public bool active;
    public GameObject go;
    public TMP_Text txt;
    public Vector3 motion;
    public float duration;
    public float lastShown;
    private float durFraction;

    public void Show()
    {
        active = true;
        lastShown = Time.time;
        go.SetActive(active);
    }

    public void Hide()
    {
        active = false;
        go.SetActive(false);
    }

    public void UpdateFloatingText()
    {
        if(!active)
            return;

        if (Time.time - lastShown > duration)
            Hide();

        durFraction = 1 - ((Time.time - lastShown) / duration);
        go.transform.position += motion * Time.deltaTime;
        txt.alpha = Globals.EaseOutSine(durFraction);
    }
}