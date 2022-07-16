using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GrappleSystem : MonoBehaviour
{
    public GameObject grappleHingeAnchor;
    public DistanceJoint2D grappleJoint;
    public Transform crosshair;
    public SpriteRenderer crosshairSprite;
    public PlayerController playerController;
    private bool grappleAttached;
    private Vector2 playerPosition;
    private Rigidbody2D grappleHingeAnchorRb;
    private SpriteRenderer grappleHingeAnchorSprite;
    private float aimAngle;
    private Vector2 aimDirection;

    public LineRenderer grappleRenderer;
    public LayerMask grappleLayerMask;
    private float grappleMaxCastDistance = 20f;
    private List<Vector2> grapplePositions = new List<Vector2>();
    private bool distanceSet;

    public InputController inputController;

    void Awake()
    {
        // 2
        grappleJoint.enabled = false;
        playerPosition = transform.position;
        grappleHingeAnchorRb = grappleHingeAnchor.GetComponent<Rigidbody2D>();
        grappleHingeAnchorSprite = grappleHingeAnchor.GetComponent<SpriteRenderer>();
        inputController = playerController.playerControls;
        crosshairSprite.enabled = false;
        grappleAttached = false;
    }

    void Update()
    {
        if (playerController.isGrappling)
            UpdateGrappleSystem(playerController.grapplingEnemy.transform.position);

        UpdateGrapplePositions();
    }

    private void GetAimDirection(Vector3 target)
    {
        var facingDirection = target - transform.position;
        aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);
        if (aimAngle < 0f)
        {
            aimAngle = Mathf.PI * 2 + aimAngle;
        }

        // 4
        aimDirection = Quaternion.Euler(0, 0, aimAngle * Mathf.Rad2Deg) * Vector2.right;
        // 5
        playerPosition = transform.position;
    }

    private void SetAimPosition(float aimAngle)
    {
        if (!crosshairSprite.enabled)
        {
            crosshairSprite.enabled = true;
        }

        var x = transform.position.x + 1f * Mathf.Cos(aimAngle);
        var y = transform.position.y + 1f * Mathf.Sin(aimAngle);

        var crossHairPosition = new Vector3(x, y, 0);
        crosshair.transform.position = crossHairPosition;
    }

    public void AttachGrapple(Vector2 target)
    {
        if (playerController.isGrappling)
        {
            // 2
            if (grappleAttached) return;
            grappleRenderer.enabled = true;

            var hit = Physics2D.Raycast(playerPosition, target, grappleMaxCastDistance, grappleLayerMask);
            
            // 3
            if (hit.collider != null)
            {
                grappleAttached = true;
                if (!grapplePositions.Contains(hit.point))
                {
                    // 4
                    // Jump slightly to distance the player a little from the ground after grappling to something.
                    //transform.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, 2f), ForceMode2D.Impulse);
                    grapplePositions.Add(hit.point);
                    grappleJoint.distance = Mathf.Max(3f, Vector2.Distance(playerPosition, hit.point));
                    grappleJoint.enabled = true;
                    grappleHingeAnchorSprite.enabled = true;
                }
            }
            // 5
            else
            {
                grappleRenderer.enabled = false;
                grappleAttached = false;
                grappleJoint.enabled = false;
            }
        }
    }

    // 6
    public void ResetRope()
    {
        grappleJoint.enabled = false;
        grappleAttached = false;
        playerController.isGrappling = false;
        grappleRenderer.positionCount = 2;
        grappleRenderer.SetPosition(0, transform.position);
        grappleRenderer.SetPosition(1, transform.position);
        grapplePositions.Clear();
        grappleHingeAnchorSprite.enabled = false;
    }

    public void UpdateGrappleSystem(Vector2 target)
    {
        GetAimDirection(target);

        // 6
        if (!grappleAttached)
        {
            SetAimPosition(aimAngle);
        }
        else 
        {
            UpdateGrapplePoint();
            crosshairSprite.enabled = false;
        }

        AttachGrapple(aimDirection);
    }

    private void UpdateGrapplePositions()
    {
        // 1
        if (!grappleAttached)
        {
            return;
        }

        // 2
        grappleRenderer.positionCount = grapplePositions.Count + 1;

        // 3
        for (var i = grappleRenderer.positionCount - 1; i >= 0; i--)
        {
            if (i != grappleRenderer.positionCount - 1) // if not the Last point of line renderer
            {
                grappleRenderer.SetPosition(i, grapplePositions[i]);
                    
                // 4
                if (i == grapplePositions.Count - 1 || grapplePositions.Count == 1)
                {
                    var ropePosition = grapplePositions[grapplePositions.Count - 1];
                    if (grapplePositions.Count == 1)
                    {
                        grappleHingeAnchorRb.transform.position = ropePosition;
                        if (!distanceSet)
                        {
                            grappleJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            distanceSet = true;
                        }
                    }
                    else
                    {
                        grappleHingeAnchorRb.transform.position = ropePosition;
                        if (!distanceSet)
                        {
                            grappleJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            distanceSet = true;
                        }
                    }
                }
                // 5
                else if (i - 1 == grapplePositions.IndexOf(grapplePositions.Last()))
                {
                    var ropePosition = grapplePositions.Last();
                    grappleHingeAnchorRb.transform.position = ropePosition;
                    if (!distanceSet)
                    {
                        grappleJoint.distance = Vector2.Distance(transform.position, ropePosition);
                        distanceSet = true;
                    }
                }
            }
            else
            {
                // 6
                grappleRenderer.SetPosition(i, transform.position);
            }
        }
    }

    public void UpdateGrapplePoint()
    {
        grapplePositions[0] = playerController.grapplingEnemy.transform.position;
    }
}
