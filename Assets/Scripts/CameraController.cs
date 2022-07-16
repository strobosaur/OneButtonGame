using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    public Transform lookAt;
    public float camSpd = 0.00625f;
    public float camBounds = 0.00016f;
    public float camDistance = 1.0f;

    private Vector3 camTarget;
    private Vector2 moveDelta;
    private Vector2 movePos;
    private float targetDist;

    public bool isFollowing;

    public void Awake()
    {
        isFollowing = true;
    }

    // START
    public void Start()
    {
        //DontDestroyOnLoad(gameObject);
        lookAt = GameObject.Find(Globals.G_PLAYERNAME).transform;
    }

    private void Update()
    {
        if (GameManager.instance.levelWon || GameManager.instance.gameOver) {
            isFollowing = false;
        }
    }

    private void FixedUpdate()
    {
        targetDist = Vector2.Distance(transform.position, lookAt.position);
        // CHECK DISTANCE TO TARGET OBJECT
        if (Vector3.Distance(transform.position, lookAt.position) > camBounds){
            if (isFollowing)
            {
                camTarget = lookAt.position * camDistance;
                moveDelta = new Vector2(camTarget.x, camTarget.y);
            }

            movePos = Vector2.Lerp(movePos, moveDelta, camSpd + (targetDist / 100f));
        }
    }

    // LATE UPDATE
    public void LateUpdate()
    {
        transform.position = new Vector3(movePos.x, movePos.y, transform.position.z);
    }
}