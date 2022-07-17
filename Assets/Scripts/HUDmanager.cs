using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDmanager : MonoBehaviour
{
    public Image goScreen;
    public TMP_Text screenText;

    public TMP_Text killsText;
    public TMP_Text multiplierText;
    public TMP_Text scoreText;

    private float screenFadeStartTime;
    private float screenFadeDuration = 2f;
    private float screenFadeFactor;

    private float textFadeStartTime;
    private float textFadeDuration = 3f;
    private float textFadeFactor;

    private bool showHud;

    private void Awake()
    {
        screenFadeStartTime = Time.time;
        textFadeStartTime = Time.time;
        showHud = false;
        goScreen.enabled = false;
        screenText.enabled = true;
    }

    private void Update()
    {
        // UPDATE HUD
        UpdateHudText();

        if (showHud) {
            BlackScreenFade();
        } else {
            BlackScreenFade(false);
        }
    }

    private void BlackScreenFade(bool fadeIn = true)
    {
        if ((goScreen.enabled) || (screenText.enabled)) {
            if (fadeIn){
                screenFadeFactor = Mathf.Min(1f, ((Time.time - screenFadeStartTime) / screenFadeDuration));
                textFadeFactor = Mathf.Min(1f, ((Time.time - textFadeStartTime) / textFadeDuration));
            } else {
                screenFadeFactor = Mathf.Max(0f, 1f - ((Time.time - screenFadeStartTime) / screenFadeDuration));
                textFadeFactor = Mathf.Max(0f, 1f - ((Time.time - textFadeStartTime) / textFadeDuration));

                if (screenFadeFactor <= 0) goScreen.enabled = false;                
                if (textFadeFactor <= 0) screenText.enabled = false;
            }

            Color col = goScreen.color;
            col = new Color(col.r, col.g, col.b, screenFadeFactor);
            goScreen.color = col;
            
            screenText.alpha = textFadeFactor;
        }
    }

    public void SetHud(string text, bool show = true)
    {
        screenFadeStartTime = Time.time;
        textFadeStartTime = Time.time;
        screenText.text = text;

        if (show) {
            showHud = true;
            goScreen.enabled = true;
            screenText.enabled = true;
        } else {
            showHud = false;
        }
    }

    public void StartLevel(string msg)
    {
        screenFadeStartTime = Time.time;
        textFadeStartTime = Time.time;
        showHud = false;
        goScreen.enabled = true;
        screenText.enabled = true;
        screenText.text = msg;
    }

    public void UpdateHudText()
    {
        scoreText.text = GameManager.instance.score.ToString();
        multiplierText.text = GameManager.instance.scoreManager.scoreMultiplier.ToString();
        killsText.text = GameManager.instance.killsTotal.ToString();
    }

    public void DisplayHighscores()
    {
        showHud = true;
        goScreen.enabled = true;
        screenText.enabled = true;
        string hsString = "";

        List<int> hsList = GameManager.instance.scoreManager.highscoreList;
        for (int i = 0; i < hsList.Count; i++)
        {
            hsString += (i+1).ToString() + " - " + hsList[i].ToString() + "\n";
        }

        screenText.text = hsString;
    }
}
