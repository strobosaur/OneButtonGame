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

    public List<GameObject> enemyList;
    public GameObject enemyPrefab;
    public ParticleSystem bloodPS;

    public Vector2 gameCenterPoint;
    public float minDistBetweenEnemies;
    public float maxDistSpawn;
    public int enemiesThisLevel;
    public int gameLevel;

    public float playerDist;
    public float playerMaxDist;
    public bool gameOver;
    public float levelOverFade = 2f;
    public float levelOverTime;
    public bool levelWon;

    public int money;

    private void Awake()
    {
        if (GameManager.instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        playerControls = new InputController();

        DontDestroyOnLoad(gameObject);

        ResetGame();
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
        cam = Camera.main.GetComponent<CameraController>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        SpawnEnemies(enemiesThisLevel, maxDistSpawn, gameCenterPoint, minDistBetweenEnemies);
        hud.StartLevel("LEVEL " + gameLevel);
    }

    // Update is called once per frame
    void Update()
    {
        playerDist = Vector2.Distance(player.transform.position, gameCenterPoint);

        if (!gameOver) {
            CheckWinCondition();
            CheckPlayerDeath();
        } else {
            HandleGameOver();
        }        
    }

    public void ResetGame()
    {
        gameLevel = 1;

        gameCenterPoint = Vector2.up * 10f;
        minDistBetweenEnemies = 2f;
        maxDistSpawn = 14f;
        enemiesThisLevel = 10;
        playerMaxDist = 80f;
        levelWon = false;
        gameOver = false;
    }

    public void ShowText(string msg, Vector3 position, Color color, float duration = 2.0f, float motion = 15.0f, int fontSize = 16)
    {
        floatingTextManager.Show(msg, fontSize, color, position, new Vector3(0,motion,0), duration);
    }

    private void SpawnEnemies(int amount, float maxDist, Vector2 center, float minDistEnemy)
    {
        int overload;
        Vector3 spawnPoint;
        for (int i = 0; i < amount; i++)
        {
            overload = 1000;
            do {
                spawnPoint = center + Random.insideUnitCircle * maxDist;
                overload--;
            } while ((DistanceNearestEnemy(spawnPoint) < minDistEnemy) || (Vector2.Distance(spawnPoint, player.transform.position) < minDistEnemy));

            var ob = Instantiate(enemyPrefab, new Vector3(spawnPoint.x, spawnPoint.y, 0), Quaternion.identity);
            enemyList.Add(ob);
            
            center.y += 1f;
        }

        gameCenterPoint = (center / 2);
    }

    public (GameObject, float) FindNearestEnemy()
    {
        Transform t = player.GetComponent<Transform>();
        float distance = 10000f;
        float tempDist;
        int index = 0;
        GameObject output = null;
        
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

    public void SpawnBlood(Vector2 position, Vector2 angle = new Vector2())
    {
        var ob = Instantiate(bloodPS, position, Quaternion.identity);
        if (angle.magnitude > 0) {
            var bs = ob.GetComponent<BloodPS>();
            bs.SetAngle(angle);
        }
    }

    public void CheckPlayerDeath()
    {
        if (playerDist > playerMaxDist) {
            gameOver = true;
            levelOverTime = Time.time;

            hud.SetHud("GAME OVER");
        }
    }

    public void HandleGameOver()
    {
        if ((Time.time - levelOverTime > levelOverFade) && (btn.WasPressedThisFrame())){
            SceneManager.LoadScene("Level_01");
        }
    }

    public void CheckWinCondition()
    {
        if (enemyList.Count <= 0) {
            cam.isFollowing = false;
            levelWon = true;

            hud.SetHud("LEVEL " + gameLevel.ToString() + " WIN");
        }
    }

    public void HandleLevelWon()
    {

    }
}
