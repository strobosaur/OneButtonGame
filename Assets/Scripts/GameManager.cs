using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public CameraController cam;

    public InputController playerControls;
    private InputAction btn;
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

    private float playerDist;
    private float playerMaxDist;
    public bool gameOver;
    private float levelOverFade = 2f;
    private float levelOverTime;
    public bool levelWon;

    public int score;
    public int killsTotal;

    private void Awake()
    {
        if (GameManager.instance != null) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        playerControls = new InputController();

        DontDestroyOnLoad(gameObject);
        
        SceneManager.sceneLoaded += LoadState;
    }

    private void OnEnable()
    {
        btn = playerControls.Player.Button;
        btn.Enable();
    }

    private void OnDisable()
    {
        btn.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        List<GameObject> enemyList = new List<GameObject>();        
        hud.StartLevel("LEVEL " + gameLevel);
    }

    // Update is called once per frame
    void Update()
    {
        playerDist = Vector2.Distance(player.transform.position, gameCenterPoint);

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

        hud.killsText = GameObject.Find("fieldKills").GetComponent<TMP_Text>();
        hud.multiplierText = GameObject.Find("fieldMultiplier").GetComponent<TMP_Text>();
        hud.scoreText = GameObject.Find("fieldScore").GetComponent<TMP_Text>();

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
            PlayerPrefs.DeleteAll();
        }

        // RESET HUD
        hud.StartLevel("LEVEL " + gameLevel);

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

    // TOTAL RESET OF GAME MANAGERS PARAMETERS
    public void ResetGame()
    {
        gameLevel = 1;

        gameCenterPoint = Vector2.up * 10f;
        minDistBetweenEnemies = 2f;
        maxDistSpawn = 6f;
        enemiesThisLevel = 5;

        score = 0;
        killsTotal = 0;

        ResetLevel();
    }

    // RESET LEVEL PARAMETERS
    public void ResetLevel()
    {
        playerMaxDist = 80f;
        levelWon = false;
        gameOver = false;
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
        float distOffset = 0;
        for (int i = 0; i < amount; i++)
        {
            do {
                spawnPoint = center + Random.insideUnitCircle * (maxDist + distOffset);
            } while ((DistanceNearestEnemy(spawnPoint) < minDistEnemy) || (Vector2.Distance(spawnPoint, player.transform.position) < minDistEnemy));

            var ob = Instantiate(enemyPrefab, new Vector3(spawnPoint.x, spawnPoint.y, 0), Quaternion.identity);
            enemyList.Add(ob);
            
            center.y += 1f;
            distOffset += 1.0f;
        }

        gameCenterPoint = center;
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
        //Transform t = player.GetComponent<Transform>();
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
        if (playerDist > playerMaxDist) {
            gameOver = true;
            levelOverTime = Time.time;

            hud.SetHud("GAME OVER");
        }
    }

    // HANDLE GAME OVER
    public void HandleGameOver()
    {
        // CHECK TIMER
        if ((Time.time - levelOverTime > levelOverFade) && (btn.WasPressedThisFrame())){
            // DELETE ALL SAVE DATA
            PlayerPrefs.DeleteAll();
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
            levelOverTime = Time.time;

            hud.SetHud("LEVEL " + gameLevel.ToString() + " WIN");
        }
    }

    // HANDLE NEW LEVEL
    public void HandleNewLevel()
    {
        gameLevel++;
        enemiesThisLevel += Mathf.RoundToInt(gameLevel * 2.5f);
        maxDistSpawn += gameLevel * 0.75f;
    }
}
