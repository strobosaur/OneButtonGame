using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDmanager : MonoBehaviour
{
    public Image goScreen;
    public TMP_Text screenText;
    public GameObject[] highscoreArr;

    public TMP_Text killsText;
    public TMP_Text multiplierText;
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public TMP_Text highscoreText;

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

        DisplayHighscores(false);
    }

    private void Update()
    {
        // UPDATE HUD
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

    public void UpdateHudText(float timeLeft, float score, float multiplier, float kills)
    {
        scoreText.text = GameManager.instance.score.ToString();
        multiplierText.text = GameManager.instance.scoreManager.scoreMultiplier.ToString();
        killsText.text = GameManager.instance.killsTotal.ToString();

        System.TimeSpan result = System.TimeSpan.FromSeconds(timeLeft);
        System.DateTime actualResult = System.DateTime.MinValue.Add(result);
        timerText.text = actualResult.ToString("ss:ff");
    }

    public void DisplayHighscores(bool show = true)
    {
        if (show)
        {
            showHud = true;
            goScreen.enabled = true;
            screenText.enabled = false;
            float hueMod = 1;

            // ENABLE SCORE TEXT OBJECTS
            GameObject.Find("HighscoreText0").GetComponent<TMP_Text>().enabled = true;
            foreach (var t in highscoreArr) { t.GetComponent<TMP_Text>().enabled = true; }

            List<(System.DateTime, int, int, int)> hsList = GameManager.instance.scoreManager.highscoreList;
            if (hsList.Count > 1) hueMod = 1f / ((hsList[Mathf.Min(4, (hsList.Count - 1))].Item2 - hsList[0].Item2) / (hsList.Count + 1));

            for (int i = 0; i < Mathf.Min(5, hsList.Count); i++)
            {
                TMP_Text hs = highscoreArr[i].GetComponent<TMP_Text>();
                hs.color = Color.HSVToRGB(hueMod * i, 0.25f, 1f);
                
                hs.text = hsList[i].Item1.ToString("yyyy/MM/dd - hh:mm:ss") + " | " 
                        + "Level " + hsList[i].Item3.ToString() + " | "
                        + hsList[i].Item4.ToString() + " Kills | "
                        + hsList[i].Item2.ToString() + " pts";
            }
        } else {
            // DISABLE SCORE TEXT OBJECTS
            GameObject.Find("HighscoreText0").GetComponent<TMP_Text>().enabled = false;
            foreach (var t in highscoreArr) { t.GetComponent<TMP_Text>().enabled = false; }
        }
    }

    public void ToggleHUD(bool on = true)
    {
        if (on) {
            GameObject.Find("HUDtext").GetComponent<CanvasGroup>().alpha = 1;
        } else {            
            GameObject.Find("HUDtext").GetComponent<CanvasGroup>().alpha = 0;
        }
    }
}
