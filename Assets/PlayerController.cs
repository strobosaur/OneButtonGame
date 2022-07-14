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

    private LineRenderer lineRenderer;
    public GameObject nearestEnemy;
    public GameObject grapplingEnemy;
    public float nearestEnemyDist;
    public bool enemyInRange = false;

    public float p1Spd = 5.0f;

    private float jumpForce;
    private float jumpBuildSpd = 10f;
    private float maxJumpForce = 1000.0f;
    public bool isJumping = false;
    public bool isGrappling = false;
    protected float distToGround;

    void Awake()
    {
        playerControls = new InputController();
        rb = GetComponent<Rigidbody2D>();
        // lineRenderer = new LineRenderer();
        // lineRenderer.startColor = Color.cyan;
        // lineRenderer.endColor = Color.red;
        // lineRenderer.startWidth = 2f;
        // lineRenderer.endWidth = 2f;
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
                PlayerJump(new Vector2(0,1));
            } else if (btn.IsPressed()) {
                PrepareJump(jumpBuildSpd);
            }
        }
        
        if (!isGrappling) 
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

    private void PrepareJump(float input)
    {
        jumpForce = Globals.Approach(jumpForce, maxJumpForce, input);
    }

    private void PlayerJump(Vector2 dir)
    {
        rb.AddForce(dir * jumpForce);
        jumpForce = 0f;
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
        Debug.DrawLine(nearestEnemy.transform.position, transform.position, Color.red, 1f);
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
            if (!isGrappling)
                grapplingEnemy = nearestEnemy;

            isGrappling = true;
            Debug.DrawLine(grapplingEnemy.transform.position, transform.position, Color.red, 1f);
            // lineRenderer.SetPosition(0,rb.transform.localPosition);            
            // lineRenderer.SetPosition(1,nearestEnemy.transform.localPosition);
            
            rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x,0f,Globals.G_INERTIA),-0.1f);
            PrepareJump(jumpBuildSpd);
        } else if (enemyInRange && btn.WasReleasedThisFrame()) {
            Vector2 dir = (grapplingEnemy.transform.position - transform.position).normalized;
            PlayerJump(dir);
            isGrappling = false;
        }
    }
}
