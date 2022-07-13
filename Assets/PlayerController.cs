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
    private float jumpBuildSpd = 0.05f;
    private float maxJumpForce = 15.0f;
    private bool isJumping = false;

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

    protected override void Update()
    {
        base.Update();
        moveInput = move.ReadValue<Vector2>();
        if (btn.IsPressed()) {
            PrepareJump(jumpBuildSpd);
        } else if (btn.WasReleasedThisFrame()) {
            PlayerJump();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateMotor(new Vector2(moveInput.x,0));
    }

    protected override void OnCollide(Collider2D collider)
    {
        isJumping = false;
    }

    private void PrepareJump(float input)
    {
        jumpForce = Globals.Approach(jumpForce, maxJumpForce, input);
    }

    private void PlayerJump()
    {
        rb.AddForce(new Vector2(0,jumpForce));
        jumpForce = 0;
        isJumping = true;
    }
}
