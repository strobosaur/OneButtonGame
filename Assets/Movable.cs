using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Movable : Collidable
{
    protected BoxCollider2D boxCollider;
    protected RaycastHit2D hit;

    protected Vector2 moveInput;
    protected Vector3 moveDelta;
    protected Vector2 movePos;
    protected Vector3 pushDirection;
    protected float ySpeed = 1.0f;
    protected float xSpeed = 1.0f;
    
    public bool collidable = false;
    public float moveSpd = 3f;
    public float spdBoost = 1.0f;
    public float pushRecoverySpeed = 0.005f;

    protected bool isColliding;

    // START
    protected override void Start()
    {
        base.Start();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // MOVEMENT
    protected virtual void UpdateMotor(Vector3 input)
    {        
        // RESET MOVE DELTA
        moveInput = new Vector2(input.x * xSpeed, input.y * ySpeed);
        movePos = Vector2.Lerp(movePos, moveInput * moveSpd * spdBoost, Globals.G_INERTIA);
        moveDelta = new Vector2(movePos.x, movePos.y);

        // SWAP SPRITE DIRECTION
        if (moveInput.x > 0)
            transform.localScale = Vector3.one;
        else if (moveInput.x < 0)
            transform.localScale = new Vector3(-1,1,1);

        // ADD PUSH VECTOR IF ANY
        moveDelta += pushDirection;

        // REDUCE PUSH FORCE
        pushDirection = Vector2.Lerp(pushDirection, Vector2.zero, pushRecoverySpeed);

        // COLLISION CHECK?
        if (collidable)
        {    
            // COLLISION CHECK Y
            hit = Physics2D.BoxCast(transform.position + new Vector3(boxCollider.offset.x,boxCollider.offset.y,input.z), boxCollider.size, 0, new Vector2(0,moveDelta.y), Mathf.Abs(moveDelta.y * Time.deltaTime), LayerMask.GetMask("Actor", "Blocking"));
            if (hit.collider == null)
            {
                // MAKE ACTOR MOVE
                transform.Translate(0, moveDelta.y * Time.deltaTime, 0);
            } else {
                movePos.y = 0;
                isColliding = true;
            }

            // COLLISION CHECK X
            hit = Physics2D.BoxCast(transform.position + new Vector3(boxCollider.offset.x,boxCollider.offset.y,input.z), boxCollider.size, 0, new Vector2(moveDelta.x,0), Mathf.Abs(moveDelta.x * Time.deltaTime), LayerMask.GetMask("Actor", "Blocking"));
            if (hit.collider == null)
            {
                // MAKE ACTOR MOVE
                transform.Translate(moveDelta.x * Time.deltaTime, 0, 0);
            } else {
                movePos.x = 0;
                isColliding = true;
            }
        } else {
            transform.Translate(moveDelta * Time.deltaTime);
        }
    }
}