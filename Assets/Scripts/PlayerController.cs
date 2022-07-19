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
    private float attackDuration = 0.75f;
    private float attackDamping = 5f;
    private float attackVelocityLimit = 27.5f;
    private Vector2 attackTarget;
    public GameObject attackingEnemy;
    public AttackHitbox attackHitbox;

    public float p1Spd = 5.0f;

    private float jumpForce;
    private float jumpBuildSpd = 0.1f;
    private float maxJumpForce = 30.0f;
    private float lastJump;
    private float jumpCooldown = 0.5f;
    public bool isJumping = false;
    public bool isGrappling = false;
    protected float distToGround;

    public ParticleSystem flashPS;
    public ParticleSystem jumpPS;

    // AWAKE
    void Awake()
    {
        playerControls = new InputController();
        rb = GetComponent<Rigidbody2D>();
        maxGrapplingRange = 5f;
        minGrapplingRange = 0.5f;
        flashPS.Stop();
        jumpPS.Stop();
    }

    // ON ENABLE
    private void OnEnable()
    {
        move = playerControls.Player.Move;
        move.Enable();

        look = playerControls.Player.Look;
        look.Enable();

        btn = playerControls.Player.Button;
        btn.Enable();
    }

    // ON DISABLE
    private void OnDisable()
    {
        move.Disable();
        look.Disable();
        btn.Disable();
    }

    // START
    protected override void Start()
    {
        base.Start();
        moveSpd = p1Spd;
        lastJump = Time.time;
        Physics.IgnoreLayerCollision(8,7, true);
    }

    // UPDATE
    protected void Update()
    {
        // ONLY ACTIVE IF GAME STARTED
        if (!GameManager.instance.levelWon && !GameManager.instance.gameOver)
        {        
            if (!isGrappling && !isAttacking) {
                if (btn.WasReleasedThisFrame()) {
                    PlayerJump(nearestEnemyDir);
                } else if (btn.IsPressed()) {
                    PrepareJump(jumpBuildSpd);
                }
            }
            
            // CHECK IF GRAPPLING
            if (!isGrappling) 
                SetNearestEnemy();

            // GET DISTANCE NEAREST ENEMY
            CheckNearestEnemy();
            if (enemyInRange)
                GrappleNearestEnemy();

            // CHECK FOR ATTACK STATE
            if (isAttacking){
                Attacking();
            }

            // CHECK FOR ATTACK TIMER END
            if ((Time.time - lastAttack > attackDuration)) {
                flashPS.Stop();
                StopAttack();
            }

            // KEEP ATTACKING IF OVER CERTAIN VELOCITY
            if (rb.velocity.magnitude > attackVelocityLimit){                
                attackHitbox.Attack();
            }
        } else {
            // RESET PLAYER IF NOT ACTIVE
            ResetPlayer();
        }
    }

    // RESET PLAYER
    private void ResetPlayer()
    {
        flashPS.Stop();
        jumpPS.Stop();

        isColliding = false;
        isGrappling = false;
        isJumping = false;
        isAttacking = false;
        enemyInRange = false;
        
        jumpForce = 0;

        nearestEnemy = null;
        grapplingEnemy = null;
        attackingEnemy = null;

        grappleSystem.ResetRope();
    }

    // PREPARE JUMP & ADD FORCE
    private void PrepareJump(float input)
    {
        jumpForce = Globals.Approach(jumpForce, maxJumpForce, input + (jumpForce * 0.1f));
    }

    // PERFORM JUMP WITH BUILT FORCE
    private void PlayerJump(Vector2 dir)
    {
        if (Time.time - lastJump > jumpCooldown)
        {
            rb.AddRelativeForce(dir * jumpForce, ForceMode2D.Impulse);

            jumpForce = 0f;
            lastJump = Time.time;
            isJumping = true;
            isColliding = false;

            AudioManager.instance.Play("jump");
            jumpPS.Play();
        } else {
            jumpForce = 0f;
        }
    }

    // SET NEAREST ENEMY FROM PLAYER POSITION
    protected void SetNearestEnemy()
    {
        if (GameManager.instance.enemyList.Count > 0)
        {
            var nearest = GameManager.instance.FindNearestEnemy();
            nearestEnemy = nearest.Item1;
            nearestEnemyDist = nearest.Item2;
            nearestEnemyDir = (nearestEnemy.transform.position - transform.position).normalized;
        }
    }

    // CHECK IF NEAREST ENEMY IN RANGE
    protected void CheckNearestEnemy()
    {
        if ((nearestEnemyDist < maxGrapplingRange) && (nearestEnemyDist > minGrapplingRange))
            enemyInRange = true;
        else {
            enemyInRange = false;
            isGrappling = false;
        }
    }

    // TRY TO GRAPPLE NEAREST ENEMY
    protected void GrappleNearestEnemy()
    {
        if (enemyInRange && btn.WasPerformedThisFrame())
        {
            if (!isGrappling)
                grapplingEnemy = nearestEnemy;

            if (grapplingEnemy == null) {
                grappleSystem.ResetRope();
                isGrappling = false;
                return;
            }

            grappleSystem.UpdateGrappleSystem(grapplingEnemy.transform.position);

            isGrappling = true;
            
            AudioManager.instance.Play("grapple");
        }
           
        if (isGrappling && btn.IsPressed()) {
            PrepareJump(jumpBuildSpd);
        } else if (enemyInRange && btn.WasReleasedThisFrame()) {

            if (grapplingEnemy == null) {
                grappleSystem.ResetRope();
                isGrappling = false;
                return;
            }

            Vector2 dir = (grapplingEnemy.transform.position - transform.position).normalized;
            Attack();
            PlayerJump(dir);
        }
    }

    // INITIATE ATTACK
    protected void Attack()
    {
        isAttacking = true;
        lastAttack = Time.time;
        attackingEnemy = grapplingEnemy;
        flashPS.Play();
        grappleSystem.ResetRope();
        isGrappling = false;
    }

    // PERFORM ATTACK SEQUENCE
    protected void Attacking()
    {
        attackHitbox.Attack();
        Vector2 dir = attackingEnemy.transform.position - transform.position;
        dir.Normalize();
        float cross = Vector3.Cross(dir, transform.right).z;
        rb.angularVelocity = 360 * cross;
    }

    // ATTACK HIT ENEMY
    public void AttackHit(Vector2 dir)
    {
        rb.AddRelativeForce(dir * attackDamping, ForceMode2D.Impulse);
        StopAttack();
    }

    // STOP ATTACK SEQUENCE
    private void StopAttack()
    {
        attackHitbox.Passive();
        isAttacking = false;
        attackingEnemy = null;
    }
}
