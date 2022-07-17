using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningScreen : MonoBehaviour
{
    private float flashTime;
    private float flashDuration = 0.2f;
    private float flashAlpha;
    private float flashFactor1;
    private float flashFactor2;

    private float playerDist;
    private float playerMaxDist;

    private Image screen;
    
    // Start is called before the first frame update
    void Start()
    {
        screen = gameObject.GetComponent<Image>();
        screen.enabled = false;
        flashTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        playerDist = GameManager.instance.playerDist;
        playerMaxDist = GameManager.instance.playerMaxDist;

        if (!GameManager.instance.gameOver && !GameManager.instance.levelWon)
        {
            if (playerDist > (playerMaxDist * 0.66f)) {
                screen.enabled = true;
                UpdateScreen();
            } else {
                screen.enabled = false;
            }
        } else {
            screen.enabled = false;
        }
    }

    private void UpdateScreen()
    {
        if (Time.time - flashTime > flashDuration) {
            flashTime = Time.time;
        }

        flashFactor1 = Mathf.Max(0f, 1f - ((Time.time - flashTime) / flashDuration));
        flashFactor2 = Mathf.Max(0f, (playerDist - (playerMaxDist * 0.66f)) / (playerMaxDist / 0.66f));

        Color col = screen.color;

        col.a = 0.4f * flashFactor2;
        col.a = Mathf.Max(0f, col.a - (flashFactor1 * 0.2f));

        screen.color = col;
    }
}
