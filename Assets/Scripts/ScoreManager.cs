using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private float lastKill;
    private float lastKillDuration = 3.0f;
    private float multiplierDuration = 4.0f;
    public float scoreMultiplier = 1.0f;
    public int killStreak = 0;

    public int baseScore = 10;

    public int scoreTotal;

    public List<int> highscoreList = new List<int>();

    private void Awake()
    {
        scoreTotal = 0;
        lastKill = Time.time;
    }

    private void Update()
    {
        if (Time.time - lastKill > lastKillDuration) {
            killStreak = 0;
        }

        if (Time.time - lastKill > multiplierDuration) {
            scoreMultiplier = 1.0f;
        }
    }

    public void NewKill(Vector3 pos)
    {
        float killScore = baseScore * (GameManager.instance.gameLevel * 2.5f);
        float spdMtp;
        int killTotal;

        GameManager.instance.killsTotal++;

        spdMtp = Mathf.Max(0, 1 - ((Time.time - lastKill) / lastKillDuration));

        killScore *= (1 + (4 * spdMtp));
        killScore *= scoreMultiplier;

        killTotal = Mathf.RoundToInt(killScore);

        float hue = Mathf.Min(1, killTotal / 5000f);
        Color col = Color.HSVToRGB(hue, 0.25f, 1);

        GameManager.instance.score += killTotal;
        GameManager.instance.ShowText(killTotal.ToString(), pos, col);

        if (Time.time - lastKill < lastKillDuration)
        {
            killStreak++;
            scoreMultiplier = Mathf.Min(1000f,((scoreMultiplier * 1.5f) + (0.25f * killStreak)));
        }

        lastKill = Time.time;
    }
}
