using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    public ParticleSystem dustPS;

    public Transform lookAt;
    public PlayerController player;
    public float camSpd = 0.00625f;
    public float camBounds = 0.00016f;
    public float camDistance = 1.0f;

    private Vector3 camTarget;
    private Vector2 moveDelta;
    private Vector2 movePos;
    private float targetDist;

    public bool isFollowing;

    // AWAKE
    public void Awake()
    {
        isFollowing = true;
        player = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    // START
    public void Start()
    {
        lookAt = GameObject.Find(Globals.G_PLAYERNAME).transform;
    }

    // UPDATE
    private void Update()
    {
        if (GameManager.instance.levelWon || GameManager.instance.gameOver) {
            isFollowing = false;
        } else {
            isFollowing = true;
        }
    }

    // FIXED UPDATE
    private void FixedUpdate()
    {
        // CHECK DISTANCE TO TARGET OBJECT
        targetDist = Vector2.Distance(transform.position, lookAt.position);
        
        if (Vector3.Distance(transform.position, lookAt.position) > camBounds){
            if (isFollowing)
            {
                camTarget = lookAt.position * camDistance;
                //camTarget = lookAt.position + (new Vector3(player.rb.velocity.x, player.rb.velocity.y, 0) * 0.5f);
                moveDelta = new Vector2(camTarget.x, camTarget.y);
                targetDist = Vector2.Distance(transform.position, moveDelta);
                dustPS.transform.position = camTarget;
            } else {
                dustPS.transform.position = transform.position;
            }

            //movePos = Vector2.Lerp(movePos, moveDelta, camSpd + (targetDist / 300f));
            movePos = Vector2.Lerp(movePos, moveDelta, camSpd + (targetDist / 75f));
            movePos.x = Mathf.FloorToInt(movePos.x * Globals.G_CELLSIZE) / Globals.G_CELLSIZE;
            movePos.y = Mathf.FloorToInt(movePos.y * Globals.G_CELLSIZE) / Globals.G_CELLSIZE;
        }
    }

    // LATE UPDATE
    public void LateUpdate()
    {
        transform.position = new Vector3(movePos.x, movePos.y, transform.position.z);
    }
}