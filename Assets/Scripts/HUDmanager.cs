using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDmanager : MonoBehaviour
{
    public GameObject hud;
    public Image goScreen;
    public TMP_Text screenText;

    private float fadeStartTime;
    private float fadeDuration = 2f;
    private bool showHud;

    private void Awake()
    {
        fadeStartTime = Time.time;
        showHud = false;
        goScreen.enabled = false;
        screenText.enabled = true;
    }

    private void Update()
    {
        if (showHud) {
            BlackScreenFade();
        } else {
            BlackScreenFade(false);
        }
    }

    private void BlackScreenFade(bool fadeIn = true)
    {
        if ((goScreen.enabled) || (screenText.enabled)) {
            float alpha1;
            float alpha2;

            if (fadeIn){
                alpha1 = Mathf.Min(1f, ((Time.time - fadeStartTime) / fadeDuration));
                alpha2 = Mathf.Min(1f, ((Time.time - (fadeStartTime * 3)) / (fadeDuration * 3)));
            } else {
                alpha1 = Mathf.Max(0f, 1f - ((Time.time - fadeStartTime) / fadeDuration));
                alpha2 = Mathf.Max(0f, 1f - ((Time.time - (fadeStartTime * 3)) / (fadeDuration * 3)));

                if (alpha1 <= 0) goScreen.enabled = false;                
                if (alpha2 <= 0) screenText.enabled = false;
            }

            Color col = goScreen.color;
            col = new Color(col.r, col.g, col.b, alpha1);
            goScreen.color = col;

            screenText.alpha = alpha2;
        }
    }

    public void SetHud(string text, bool show = true)
    {
        fadeStartTime = Time.time;
        screenText.text = text;

        if (show) {
            showHud = true;
            goScreen.enabled = true;
            screenText.enabled = false;
        } else {
            showHud = false;
        }
    }

    public void StartLevel(string msg)
    {
        fadeStartTime = Time.time;
        showHud = false;
        goScreen.enabled = true;
        screenText.enabled = true;
        screenText.text = msg;
    }
}
