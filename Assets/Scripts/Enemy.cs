using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Movable
{
    public Vector3 startPos;
    public Vector3 targetPos;
    private float distRadius;
    public float minTimer = 2f;
    public float maxTimer = 5f;

    public Timer timer;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        startPos = transform.position;
        distRadius = 1.5f;
        ChooseNewTarget();
        timer = Instantiate(timer);
        timer.SetNewTime(Random.Range(minTimer, maxTimer));
        moveSpd = 2.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer.timerEnd) {
            ChooseNewTarget();
            timer.SetNewTime(Random.Range(minTimer, maxTimer));
        }
    }

    // FIXED UPDATE
    private void FixedUpdate()
    {
        float targetDist = Vector3.Distance(targetPos, transform.position);

        // PREVENT JITTERING
        if (targetDist < 0.25f)
            moveDelta = Vector3.zero;
        else
            moveDelta = (targetPos - transform.position).normalized;

        UpdateMotor(moveDelta);
    }

    private void ChooseNewTarget()
    {
        targetPos = new Vector2(startPos.x, startPos.y) + (Random.insideUnitCircle * distRadius);
    }

    // ALL FIGHTERS CAN RECEIVE DAMAGE
    protected virtual void ReceiveDamage(DoDamage dmg)
    {
        // SHOW DAMAGE MESSAGE
        GameManager.instance.ShowText("+1 KILL!", transform.position, Color.white, 2, 15, 16);
        GameManager.instance.enemyList.Remove(gameObject);
        GameManager.instance.SpawnBlood(transform.position);
        GameManager.instance.scoreManager.NewKill(transform.position + Vector3.up);

        AudioManager.instance.Play("hit");

        Destroy(gameObject);
    }
}
