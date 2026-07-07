using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Movable : MonoBehaviour
{
    protected CircleCollider2D collider;
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
    protected virtual void Start()
    {
        collider = GetComponent<CircleCollider2D>();
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

        // COLLISION CHECK
        if (collidable)
        {    
            hit = Physics2D.CircleCast(
                transform.position, 
                collider.radius, 
                moveDelta, 
                LayerMask.GetMask("Actor", "Blocking"));
            isColliding = hit.collider != null;
        }

        // MOVE
        transform.Translate(moveDelta * Time.deltaTime);
    }
}