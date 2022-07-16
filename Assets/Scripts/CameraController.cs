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
        if (CameraController.instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

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
        if (GameManager.instance.playerDist < GameManager.instance.playerMaxDist) {
            isFollowing = false;
        }
    }

    // LATE UPDATE
    public void LateUpdate()
    {
        targetDist = Vector2.Distance(transform.position, lookAt.position);
        // CHECK DISTANCE TO TARGET OBJECT
        if (Vector3.Distance(transform.position, lookAt.position) > camBounds){
            if (isFollowing)
            {
                camTarget = lookAt.position * camDistance;
                moveDelta = new Vector2(camTarget.x, camTarget.y);
            }

            movePos = Vector2.Lerp(movePos, moveDelta, camSpd + (targetDist / 1000f));
            transform.position = new Vector3(movePos.x, movePos.y, transform.position.z);
        }        
    }
}