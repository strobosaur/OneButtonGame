using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : Collidable
{
    public PlayerController player;
    public TrailRenderer attackPS1;

    private float lastAttack;
    private float attackDuration = 0.85f;
    private bool endAttack = false;

    protected override void Start()
    {
        base.Start();
        collider.enabled = false;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (endAttack) {
            if (Time.time - lastAttack > attackDuration) {
                collider.enabled = false;
            }
        }

        if (collider.enabled) {
            attackPS1.emitting = true;
        } else {
            attackPS1.emitting = false;
        }
    }

    public void Attack()
    {
        //boxCollider.enabled = true;
        collider.enabled = true;
        endAttack = false;
    }

    public void Passive()
    {
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
                impactForce = player.rb.linearVelocity.magnitude
            };

            collided.SendMessage("ReceiveDamage", dmg);

            player.AttackHit((player.transform.position - collided.transform.position).normalized);
        }
    }
}
