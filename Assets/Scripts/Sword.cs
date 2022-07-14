using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public PlayerController player;
    public GameObject target;
    public Animator anim;
    public float turnSpd = 0.5f;
    public float lastSwing;
    public float swingCooldown = 5f;
    public bool canSwing;
    public bool isAttacking;

    private void Start()
    {
        lastSwing = Time.time;
        isAttacking = false;
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        target = player.nearestEnemy;
        if (isAttacking) {
            Swing();
            isAttacking = false;
        } else if (anim.GetCurrentAnimatorStateInfo(0).IsName("sword_idle")) {
            RotateToTarget();
        }
    }

    private void RotateToTarget()
    {        
        if (player.enemyInRange)
        {
            Vector3 targ = target.transform.position;
            targ.z = 0f;

            Vector3 objectPos = transform.position;
            targ.x = targ.x - objectPos.x;
            targ.y = targ.y - objectPos.y;

            float angle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle+90f));
        } else {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
    }    

    // ON COLLIDE
    private void OnCollide(Collider2D collided)
    {
        // CHECK FOR COLLISION WITH A FIGHTING ACTOR
        if (collided.tag == "Enemy"){
            collided.SendMessage("Destroy");
        }
    }

    // SWING FUNCTION
    public void Swing()
    {
        anim.SetTrigger("Swing");
    }
}
