using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public PlayerController player;
    public FloatingTextManager floatingTextManager;
    public List<GameObject> enemyList;
    public GameObject enemyPrefab;
    public ParticleSystem bloodPS;

    public int money;

    private void Awake()
    {
        if (GameManager.instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        SpawnEnemies(20, 14, new Vector2(0, 3));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowText(string msg, Vector3 position, Color color, float duration = 2.0f, float motion = 15.0f, int fontSize = 16)
    {
        floatingTextManager.Show(msg, fontSize, color, position, new Vector3(0,motion,0), duration);
    }

    private void SpawnEnemies(int amount, float minDist, Vector2 center)
    {
        Vector3 spawnPoint;
        for (int i = 0; i < amount; i++)
        {
            do {
                spawnPoint = center + Random.insideUnitCircle * minDist;
            } while (spawnPoint.y < center.y);

            var ob = Instantiate(enemyPrefab, new Vector3(spawnPoint.x, spawnPoint.y, 0), Quaternion.identity);
            enemyList.Add(ob);
            
            center.y += 1f;
        }
    }

    public (GameObject, float) FindNearestEnemy()
    {
        Transform t = player.GetComponent<Transform>();
        float distance = 10000f;
        float tempDist;
        int index = 0;
        
        for(int i = 0; i < enemyList.Count; i++)
        {
            tempDist = Vector3.Distance(t.localPosition, enemyList[i].transform.localPosition);
            if (tempDist < distance) {
                distance = tempDist;
                index = i;
            }
        }

        return (enemyList[index], distance);
    }

    public float DistanceNearestEnemy()
    {
        Transform t = player.GetComponent<Transform>();
        float distance = Mathf.Infinity;
        float tempDist;
        
        foreach (var enemy in enemyList)
        {
            tempDist = Vector3.Distance(t.position, enemy.GetComponent<Transform>().position);
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
}
