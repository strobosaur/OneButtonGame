using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Movable
{
    public Rigidbody2D rb;
    public InputController playerControls;
    private InputAction move;
    private InputAction look;
    private InputAction btn;

    public GameObject nearestEnemy;
    public float nearestEnemyDist;
    public bool enemyInRange = false;

    public float p1Spd = 5.0f;

    private Vector2 jumpForce;
    private float jumpBuildSpd = 10f;
    private float maxJumpForce = 1000.0f;
    public bool isJumping = false;
    public bool isGrappling = false;
    protected float distToGround;

    void Awake()
    {
        playerControls = new InputController();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        move = playerControls.Player.Move;
        move.Enable();

        look = playerControls.Player.Look;
        look.Enable();

        btn = playerControls.Player.Button;
        btn.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
        look.Disable();
        btn.Disable();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        moveSpd = p1Spd;
    }

    protected void Update()
    {        
        moveInput = move.ReadValue<Vector2>();
        
        if (!isGrappling) {
            if (btn.WasReleasedThisFrame()) {
                PlayerJump();
            } else if (btn.IsPressed()) {
                PrepareJump(new Vector2(0,jumpBuildSpd));
            }
        }
        
        SetNearestEnemy();
        CheckNearestEnemy();
        if (enemyInRange)
            GrappleNearestEnemy();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateMotor(new Vector2(moveInput.x,0));
    }

    private void PrepareJump(Vector2 input)
    {
        //float forceX = Globals.Approach(jumpForce.x, maxJumpForce, input.x);
        //float forceY = Globals.Approach(jumpForce.y, maxJumpForce, input.y);
        if (Vector2.Distance(Vector2.zero, jumpForce) < maxJumpForce)
            jumpForce += input;
        //Debug.Log(jumpForce);
    }

    private void PlayerJump()
    {
        rb.AddForce(jumpForce);
        jumpForce = Vector2.zero;
        isJumping = true;
        isColliding = false;
    }

    protected bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, 0.1f);
    }

    protected void SetNearestEnemy()
    {
        var nearest = GameManager.instance.FindNearestEnemy();
        nearestEnemy = nearest.Item1;
        nearestEnemyDist = nearest.Item2;
    }

    protected void CheckNearestEnemy()
    {
        if (nearestEnemyDist < 3f)
            enemyInRange = true;
        else {
            enemyInRange = false;
            isGrappling = false;
        }
    }

    protected void GrappleNearestEnemy()
    {
        if (enemyInRange && btn.IsPressed())
        {
            isGrappling = true;
            Vector2 dir = (nearestEnemy.transform.position - transform.position).normalized;
            Debug.Log(dir);
            rb.velocity = new Vector2(rb.velocity.x,-0.1f);
            PrepareJump(dir * jumpBuildSpd);
        } else if (enemyInRange && btn.WasReleasedThisFrame()) {
            PlayerJump();
            isGrappling = false;
        }
    }
}
