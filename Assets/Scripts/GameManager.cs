using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public CameraController cam;

    public InputController playerControls;
    private InputAction btn;
    private InputAction escape;
    private InputAction restart;
    public PlayerController player;

    public FloatingTextManager floatingTextManager;
    public HUDmanager hud;
    public ScoreManager scoreManager;

    public List<GameObject> enemyList;
    public GameObject enemyPrefab;
    public ParticleSystem bloodPS;

    public Vector2 gameCenterPoint;
    private float minDistBetweenEnemies;
    private float maxDistSpawn;
    public int enemiesThisLevel;
    public int gameLevel;

    [HideInInspector]
    public float playerDist;
    public float playerMaxDist;
    private float playerLowestY = -30f;
    public bool gameOver;
    private float levelOverFade = 2f;
    private float levelOverTime;
    public bool levelWon;
    public bool gameStart;

    private bool showHighscore = false;
    [HideInInspector]
    public float highscoreTime;
    [HideInInspector]
    public float highScoreFade = 2f;

    public int score;
    public int killsTotal;

    // DEATH CONDITIONS
    public float timeLastKill;
    public float timeToKill;
    public float timeMustKill;

    // AWAKE
    private void Awake()
    {
        // CREATE SINGLETON
        if (GameManager.instance != null) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // GET CONTROL CONNECTION
        playerControls = new InputController();
        
        // PREPARE SCENE LOAD
        SceneManager.sceneLoaded += LoadState;

        // DISABLE MOUSE CURSOR
        Cursor.visible = false;
    }

    // ON APPLICATION QUIT
    private void OnApplicationQuit()
    {
        SaveGame();
    }

    // ON ENABLE
    private void OnEnable()
    {
        btn = playerControls.Player.Button;
        btn.Enable();
        
        escape = playerControls.Player.Escape;
        escape.Enable();
        
        restart = playerControls.Player.Restart;
        restart.Enable();
    }

    // ON DISABLE
    private void OnDisable()
    {
        btn.Disable();
        escape.Disable();
        restart.Disable();
    }

    // START
    void Start()
    {
        List<GameObject> enemyList = new List<GameObject>();        
        hud.StartLevel("LEVEL " + gameLevel);

        // LOAD SCORES
        LoadGame();
        //SaveManager.LoadGame();
    }

    // UPDATE
    void Update()
    {
        // KILL GAME
        if (escape.IsPressed()) {
            //SaveManager.SaveGame(scoreManager.highscoreList);
            Application.Quit();
            return;
        }

        // RESTART GAME
        if (restart.IsPressed()) {
            // DELETE ALL SAVE STATE DATA
            showHighscore = false;
            PlayerPrefs.DeleteKey("SaveState");
            ResetLevel();
            SceneManager.LoadScene("Level_01");
            return;
        }

        // RUN GAME
        if (gameStart)
        {
            if (!gameOver && !levelWon) {
                CheckWinCondition();
            } else if (levelWon) {
                HandleLevelWin();
            }

            if (!gameOver && !levelWon) {
                CheckPlayerDeath();
            } else if (gameOver) {
                HandleGameOver();
            }

            timeToKill = Mathf.Max(0f, (timeToKill - Time.deltaTime));

            // UPDATE HUD
            hud.UpdateHudText(timeToKill, score, scoreManager.scoreMultiplier, killsTotal);
        } else if (!gameStart && !gameOver && !levelWon && btn.WasPressedThisFrame()) {
            timeLastKill = Time.time;
            timeToKill = ((timeLastKill + timeMustKill) - Time.time);

            gameOver = false;
            gameStart = true;

            hud.ToggleHUD();
            GameObject.Find("Platform").GetComponent<Animator>().SetBool("disabled", true);
            GameObject.Find("Instructions").GetComponent<Animator>().SetBool("disabled", true);
        } else {
            hud.ToggleHUD(false);
        }
    }

    // LOAD STATE SCENE MANAGEMENT
    public void LoadState(Scene s, LoadSceneMode mode)
    {
        // FIND ALL OBJECT REFERENCES
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        cam = Camera.main.GetComponent<CameraController>();
        floatingTextManager = GameObject.Find("FloatingTextManager").GetComponent<FloatingTextManager>();

        // FIND HUD OBJECT REFERENCES
        hud.goScreen = GameObject.Find("GameOverScreen").GetComponent<Image>();
        hud.screenText = GameObject.Find("BlackScreenText").GetComponent<TMP_Text>();
        hud.highscoreText = GameObject.Find("HighscoreText0").GetComponent<TMP_Text>();

        for (int i = 0; i < 5; i++) { hud.highscoreArr[i] = GameObject.Find("HighscoreText" + (i+1).ToString()); }

        // FIND HUD OSD OBJECT REFERENCES
        hud.killsText = GameObject.Find("fieldKills").GetComponent<TMP_Text>();
        hud.multiplierText = GameObject.Find("fieldMultiplier").GetComponent<TMP_Text>();
        hud.scoreText = GameObject.Find("fieldScore").GetComponent<TMP_Text>();
        hud.timerText = GameObject.Find("fieldTimer").GetComponent<TMP_Text>();

        // CHECK FOR SAVE STATE
        if (!PlayerPrefs.HasKey("SaveState"))
        {
            // RESET GAME MANAGER LEVEL PARAMETERS
            ResetGame();
        } else {
            string[] saveData = PlayerPrefs.GetString("SaveState").Split(';');
            score = int.Parse(saveData[0]);
            gameLevel = int.Parse(saveData[1]);
            killsTotal = int.Parse(saveData[2]);

            HandleNewLevel();
            PlayerPrefs.DeleteKey("SaveState");
        }

        // RESET HUD
        hud.StartLevel("LEVEL " + gameLevel);

        // RESET LEVEL PARAMETERS
        ResetLevel();

        // SPAN NEW ENEMIES
        enemyList.Clear();
        SpawnEnemies(enemiesThisLevel, maxDistSpawn, gameCenterPoint, minDistBetweenEnemies);
    }

    // LOAD STATE SCENE MANAGEMENT
    public void SaveState()
    {
        string s = "";
        s += score.ToString() + ";";
        s += gameLevel.ToString() + ";";
        s += killsTotal.ToString();

        PlayerPrefs.SetString("SaveState", s);
    }

    // LOAD STATE SCENE MANAGEMENT
    public void SaveGame()
    {
        string s = JsonUtility.ToJson(scoreManager.highscoreList);

        PlayerPrefs.SetString("SaveGame", s);
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("SaveGame"))
        {
            return;
        } else {
            scoreManager.highscoreList = JsonUtility.FromJson<List<(System.DateTime, int, int, int)>>(PlayerPrefs.GetString("SaveGame"));
        }
    }

    // TOTAL RESET OF GAME MANAGERS PARAMETERS
    public void ResetGame()
    {
        PlayerPrefs.DeleteKey("SaveState");

        // LOAD GAME
        //LoadGame();

        gameLevel = 1;

        maxDistSpawn = 6f;
        minDistBetweenEnemies = maxDistSpawn * 0.33f;
        enemiesThisLevel = 5;

        score = 0;
        killsTotal = 0;

        timeMustKill = 15f;

        ResetLevel();
    }

    // RESET LEVEL PARAMETERS
    public void ResetLevel()
    {
        // GAME MANAGER PARAMETERS
        gameCenterPoint = Vector2.up * 10f;
        levelWon = false;
        gameOver = false;
        gameStart = false;

        // SCORE MANAGER PARAMETERS
        scoreManager.killStreak = 0;
        scoreManager.scoreMultiplier = 1.0f;
    }

    // SHOW FLOATING TEXT AT POSITION
    public void ShowText(string msg, Vector3 position, Color color, float duration = 2.0f, float motion = 15.0f, int fontSize = 16)
    {
        floatingTextManager.Show(msg, fontSize, color, position, new Vector3(0,motion,0), duration);
    }

    // SPAWN ENEMIES WITH PARAMETERS
    private void SpawnEnemies(int amount, float maxDist, Vector2 center, float minDistEnemy)
    {
        Vector3 spawnPoint;
        int overload;
        float distOffset = 0;
        for (int i = 0; i < amount; i++)
        {
            overload = 1000;
            do {
                spawnPoint = center + Random.insideUnitCircle * (maxDist + distOffset);
            
                center.y += 0.5f;
                distOffset += 0.5f;

                overload--;
                if (overload <= 0) break;
            } while 
            ((DistanceNearestEnemy(spawnPoint) < minDistEnemy) 
            || (Vector2.Distance(spawnPoint, player.transform.position) < minDistEnemy)
            || (spawnPoint.y < -2f));

            if (overload <= 0) {
                center = gameCenterPoint;
                continue;
            }

            var ob = Instantiate(enemyPrefab, new Vector3(spawnPoint.x, spawnPoint.y, 0), Quaternion.identity);
            enemyList.Add(ob);
        }
    }

    // FIND NEAREST ENEMY AND DELIVER OBJECT & DISTANCE
    public (GameObject, float) FindNearestEnemy()
    {
        Transform t = player.GetComponent<Transform>();
        float distance = 10000f;
        float tempDist;
        int index = 0;
        GameObject output = null;
        
        // LOOP THROUGH LIST OF ENEMY OBJECTS AND FIND SHORTEST DISTANCE
        for(int i = 0; i < enemyList.Count; i++)
        {
            tempDist = Vector3.Distance(t.localPosition, enemyList[i].transform.localPosition);
            if (tempDist < distance) {
                distance = tempDist;
                index = i;
            }

            output = enemyList[index];
        }

        return (output, distance);
    }

    // FIND DISTANCE TO NEAREST ENEMY FROM GIVEN POINT
    public float DistanceNearestEnemy(Vector2 origin)
    {
        float distance = Mathf.Infinity;
        float tempDist;
        
        foreach (var enemy in enemyList)
        {
            tempDist = Vector3.Distance(origin, enemy.GetComponent<Transform>().position);
            if (tempDist < distance)
                distance = tempDist;
        }

        return distance;
    }

    // SPAWN BLOOD SPLASH PARTICLES
    public void SpawnBlood(Vector2 position, Vector2 angle = new Vector2())
    {
        var ob = Instantiate(bloodPS, position, Quaternion.identity);
        if (angle.magnitude > 0) {
            var bs = ob.GetComponent<BloodPS>();
            bs.SetAngle(angle);
        }
    }

    // CHECK PLAYER DEATH CONDITIONS
    public void CheckPlayerDeath()
    {
        if ((timeToKill <= 0) || (player.transform.position.y < playerLowestY)){
            gameOver = true;
            levelOverTime = Time.time;

            System.DateTime date = System.DateTime.Now;
            scoreManager.highscoreList.Add((date,score,gameLevel,killsTotal));
            scoreManager.highscoreList = scoreManager.highscoreList.OrderByDescending(score => score.Item2).ToList();

            SaveGame();

            hud.ToggleHUD(false);
            hud.SetHud("GAME OVER");
        }
    }

    // HANDLE GAME OVER
    public void HandleGameOver()
    {
        // CHECK TIMER
        if ((!showHighscore) && (Time.time - levelOverTime > levelOverFade) && (btn.WasPressedThisFrame())){
            showHighscore = true;
            hud.DisplayHighscores();
            SaveState();
        } else if ((showHighscore) && (Time.time - highscoreTime > highScoreFade) && (btn.WasPressedThisFrame())) {
            // DELETE ALL SAVE DATA
            hud.DisplayHighscores(false);
            showHighscore = false;
            PlayerPrefs.DeleteKey("SaveState");
            ResetLevel();
            SceneManager.LoadScene("Level_01");
        }
    }

    // HANDLE NEXT LEVEL
    public void HandleLevelWin()
    {
        // CHECK TIMER
        if ((Time.time - levelOverTime > levelOverFade) && (btn.WasPressedThisFrame())){
            // SAVE DATA
            SaveState();
            ResetLevel();
            SceneManager.LoadScene("Level_01");
        }
    }

    // CHECK LEVEL WIN CONDITIONS
    public void CheckWinCondition()
    {
        if (enemyList.Count <= 0) {
            cam.isFollowing = false;
            levelWon = true;
            gameOver = false;
            levelOverTime = Time.time;

            hud.ToggleHUD(false);
            hud.SetHud("LEVEL " + gameLevel.ToString() + " WIN");
        }
    }

    // HANDLE NEW LEVEL
    public void HandleNewLevel()
    {
        gameLevel++;
        if (gameLevel % 5 == 0) {
            enemiesThisLevel = 10;
            maxDistSpawn = 6f + (gameLevel * 0.25f);
            minDistBetweenEnemies = maxDistSpawn * 0.33f;
        }

        enemiesThisLevel += Mathf.RoundToInt(gameLevel * 1.5f);
        maxDistSpawn += gameLevel * 0.25f;
        minDistBetweenEnemies = maxDistSpawn * 0.33f;

        timeMustKill = 3f + Mathf.Max(0f, ((timeMustKill - 3f) * 0.9f));
    }

    // HANDLE NEW KILL 
    public void NewKill(Vector3 position)
    {
        // SHOW DAMAGE MESSAGE
        SpawnBlood(position);

        scoreManager.NewKill(position + Vector3.up);

        timeLastKill = Time.time;
        timeToKill = ((timeLastKill + timeMustKill) - Time.time);
    }
}