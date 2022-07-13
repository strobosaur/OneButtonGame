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

    public float p1Spd = 5.0f;

    private float jumpForce;
    private float jumpBuildSpd = 10f;
    private float maxJumpForce = 1000.0f;
    public bool isJumping = false;
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
        
        //if (IsGrounded()){
            if (btn.WasReleasedThisFrame()) {
                Debug.Log("Button released : " + jumpForce);
                PlayerJump();
            } else if (btn.IsPressed()) {
                PrepareJump(jumpBuildSpd);
            }
        //}
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateMotor(new Vector2(moveInput.x,0));
    }

    private void PrepareJump(float input)
    {
        jumpForce = Globals.Approach(jumpForce, maxJumpForce, input);
        Debug.Log(jumpForce);
    }

    private void PlayerJump()
    {
        rb.AddForce(new Vector2(0f,jumpForce));
        jumpForce = 0f;
        isJumping = true;
        isColliding = false;
    }

    protected bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, 0.1f);
    }

    // protected GameObject FindNearestEnemy()
    // {

    // }
}
