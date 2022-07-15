using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : Collidable
{
    public PlayerController player;
    private float lastAttack;
    private float attackDuration = 1f;
    private bool endAttack = false;

    protected override void Start()
    {
        base.Start();
        boxCollider.enabled = false;
    }

    protected override void Update()
    {
        base.Update();
        if (endAttack) {
            if (Time.time - lastAttack > attackDuration) {
                boxCollider.enabled = false;
            }
        }
    }

    public void Attack()
    {
        boxCollider.enabled = true;
        endAttack = false;
    }

    public void Passive()
    {
        //boxCollider.enabled = false;
        if (!endAttack) {
            endAttack = true;
            lastAttack = Time.time;
        }
    }

    protected override void OnCollide(Collider2D collided)
    {
        if (collided.tag == "Enemy")
        {
            DoDamage dmg = new DoDamage(){
                damageAmount = 1,
                origin = player.transform.position,
                impactForce = player.rb.velocity.magnitude
            };

            collided.SendMessage("ReceiveDamage", dmg);

            player.AttackHit((player.transform.position - collided.transform.position).normalized);
        }
    }
}
