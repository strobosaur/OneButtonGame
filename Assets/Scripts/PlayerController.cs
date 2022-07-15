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
    private Vector2 nearestEnemyDir;
    public GameObject grapplingEnemy;
    public float nearestEnemyDist;
    public bool enemyInRange = false;
    protected float maxGrapplingRange;
    protected float minGrapplingRange;
    public GrappleSystem grappleSystem;

    public bool isAttacking;
    private float lastAttack;
    private float attackDuration = 1f;
    private float attackDamping = 5f;
    private Vector2 attackTarget;
    public GameObject attackingEnemy;
    public AttackHitbox attackHitbox;

    public float p1Spd = 5.0f;

    private float jumpForce;
    private float jumpBuildSpd = 0.2f;
    private float maxJumpForce = 20.0f;
    public bool isJumping = false;
    public bool isGrappling = false;
    protected float distToGround;

    public ParticleSystem flashPS;

    void Awake()
    {
        playerControls = new InputController();
        rb = GetComponent<Rigidbody2D>();
        maxGrapplingRange = 6f;
        minGrapplingRange = 3f;
        flashPS.Stop();
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
        //moveInput = move.ReadValue<Vector2>();
        
        if (!isGrappling && !isAttacking) {
            if (btn.WasReleasedThisFrame()) {
                PlayerJump(nearestEnemyDir);
            } else if (btn.IsPressed()) {
                PrepareJump(jumpBuildSpd);
            }
        }
        
        if (!isGrappling) 
            SetNearestEnemy();

        CheckNearestEnemy();
        if (enemyInRange)
            GrappleNearestEnemy();

        if (isAttacking){
            Attacking();
        }

        if ((Time.time - lastAttack > attackDuration)) {
            flashPS.Stop();
            StopAttack();
        }
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
        rb.AddRelativeForce(dir * jumpForce, ForceMode2D.Impulse);
        jumpForce = 0f;
        isJumping = true;
        isColliding = false;
    }

    protected void SetNearestEnemy()
    {
        var nearest = GameManager.instance.FindNearestEnemy();
        nearestEnemy = nearest.Item1;
        nearestEnemyDist = nearest.Item2;
        nearestEnemyDir = (nearestEnemy.transform.position - transform.position).normalized;
    }

    protected void CheckNearestEnemy()
    {
        if ((nearestEnemyDist < maxGrapplingRange) && (nearestEnemyDist > minGrapplingRange))
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

            grappleSystem.UpdateGrappleSystem(grapplingEnemy.transform.position);

            isGrappling = true;
            Debug.DrawLine(grapplingEnemy.transform.position, transform.position, Color.red, 1f);
            // lineRenderer.SetPosition(0,rb.transform.localPosition);            
            // lineRenderer.SetPosition(1,nearestEnemy.transform.localPosition);
            
            //rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x,0f,Globals.G_INERTIA),-0.1f);
            PrepareJump(jumpBuildSpd);
        } else if (enemyInRange && btn.WasReleasedThisFrame()) {
            Vector2 dir = (grapplingEnemy.transform.position - transform.position).normalized;
            Attack();
            PlayerJump(dir);
        }
    }

    protected void Attack()
    {
        isAttacking = true;
        lastAttack = Time.time;
        attackingEnemy = grapplingEnemy;
        flashPS.Play();
        grappleSystem.ResetRope();
        isGrappling = false;
    }

    protected void Attacking()
    {
        attackHitbox.Attack();
        Vector2 dir = attackingEnemy.transform.position - transform.position;
        dir.Normalize();
        float cross = Vector3.Cross(dir, transform.right).z;
        rb.angularVelocity = 360 * cross;
    }

    public void AttackHit(Vector2 dir)
    {
        rb.AddRelativeForce(dir * attackDamping, ForceMode2D.Impulse);
        StopAttack();
    }

    private void StopAttack()
    {
        attackHitbox.Passive();
        isAttacking = false;
        attackingEnemy = null;
    }
}
