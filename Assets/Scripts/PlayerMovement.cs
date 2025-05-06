using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    // Movement
    public float moveSpeed;
    public float sprintMultiplier = 1.5f;
    public float groundDrag;

    public float jumpForce;
    public float airMultiplier;

    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    // Double Jump
    private int jumpCount = 0;
    public int maxJumpCount = 2;

    // Keybinds
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode slowMotionKey = KeyCode.Q;

    // Crouch & Slide
    public float crouchYScale = 0.5f;
    private float startYScale;
    public float slideSpeed = 7f;
    public float slideCooldown = 1f;
    private bool sliding;
    private bool slideReady = true;

    // Ground Check
    public float playerHeight;
    public LayerMask GroundLayer;
    bool grounded;

    // Wall Jumping
    public LayerMask whatIsWall;  // Layer mask for walls
    public float wallCheckDistance = 1f;  // Distance to check for walls
    public float wallJumpForce = 10f;  // Force for the wall jump
    private bool isTouchingWall;  // Whether the player is touching a wall

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    // Slow Motion Variables
    public float slowMotionFactor = 0.3f;
    public float slowMotionDuration = 3f;
    private bool isInSlowMotion = false;

    // Vaulting
    public float vaultRange = 1f;
    public float vaultHeight = 1.2f;
    public float vaultDuration = 0.4f;
    public Animation vaultAnim;
    private bool isVaulting = false;
    private Vector3 vaultStartPos;
    private Vector3 vaultEndPos;
    private float vaultTime;

    // Dash
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public KeyCode dashKey = KeyCode.LeftAlt;
    private bool canDash = true;
    private bool isDashing = false;

    // Dash Effects
    public ParticleSystem dashParticleSystem;  // Particle system for dash effect

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYScale = transform.localScale.y; // Save original Y scale for crouch reset
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.1f, GroundLayer);
        isTouchingWall = Physics.Raycast(transform.position, orientation.forward, wallCheckDistance, whatIsWall);

        MyInput(); // Handle player input
        SpeedControl(); // Clamp speed if needed

        // Handle gravity and drag
        if (!isVaulting)
        {
            if (rb.velocity.y < 0)
            {
                // Apply extra gravity when falling
                rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
            }
            else if (rb.velocity.y > 0)
            {
                // Apply less gravity for low jumps
                if (!Input.GetKey(jumpKey))
                {
                    rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1f) * Time.deltaTime;
                }
                else
                {
                    rb.velocity += Vector3.up * Physics.gravity.y * (1.5f - 1f) * Time.deltaTime;
                }
            }
            // Apply drag only when on the floor
            if (grounded)
                rb.drag = groundDrag;
            else
                rb.drag = 0;
        }
        // Vaulting movement 
        if (isVaulting)
        {
            vaultTime += Time.deltaTime;
            float t = vaultTime / vaultDuration;
            transform.position = Vector3.Lerp(vaultStartPos, vaultEndPos, t);

            if (t >= 1f)
            {
                isVaulting = false;
                rb.isKinematic = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isVaulting)
            MovePlayer(); // Apply movement
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        // jump input
        if (Input.GetKeyDown(jumpKey))
        {
            if (CheckVault())
            {
                StartVault();
                return;
            }

            if (grounded)
            {
                Jump();
            }
            else if (isTouchingWall && !grounded)
            {
                WallJump();
            }
            else if (jumpCount < maxJumpCount)
            {
                DoubleJump();
            }
        }
        // Crouch and slide input
        if (Input.GetKeyDown(crouchKey))
        {
            StartCrouch();

            if (grounded && Input.GetKey(sprintKey) && moveDirection.magnitude > 0.1f && slideReady)
            {
                if (!sliding && slideReady)
                {
                    StartCoroutine(Slide());
                }
            }
        }
        if (Input.GetKeyUp(crouchKey))
        {
            StopCrouch();
        }
        // Slow motion input
        if (Input.GetKeyDown(slowMotionKey) && !isInSlowMotion)
        {
            StartCoroutine(SlowMotion());
        }
        // Dash input
        if (Input.GetKeyDown(dashKey) && canDash && !isDashing && !isVaulting)
        {
            StartCoroutine(Dash());
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        float currentSpeed = moveSpeed;
        if (Input.GetKey(sprintKey) && grounded)
        {
            currentSpeed *= sprintMultiplier;
        }
        // Apply movement force
        if (grounded)
            rb.AddForce(moveDirection.normalized * currentSpeed * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * currentSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        float maxSpeed = moveSpeed;
        if (Input.GetKey(sprintKey) && grounded)
        {
            maxSpeed *= sprintMultiplier;
        }

        if (flatVel.magnitude > maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // Reset vertical velocity and jump
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        // Add forward momentum
        Vector3 momentum = orientation.forward * moveSpeed * 0.5f;
        rb.AddForce(momentum, ForceMode.Impulse);
        // How may time the player can jump after jumping
        jumpCount = 1;
    }

    private void DoubleJump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        Vector3 momentum = orientation.forward * moveSpeed * 0.5f;
        rb.AddForce(momentum, ForceMode.Impulse);
        jumpCount++;
    }

    private void WallJump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Stop any vertical momentum
        rb.AddForce(transform.up * wallJumpForce, ForceMode.Impulse); // Jump upward
        Vector3 wallJumpDirection = transform.up + -orientation.forward; // Push player away from wall
        rb.AddForce(wallJumpDirection.normalized * wallJumpForce, ForceMode.Impulse);
        StartCoroutine(WallJumpCooldown());
    }

    private IEnumerator WallJumpCooldown()
    {
        yield return new WaitForSeconds(0.5f); // Adjust the time for wall jump cooldown
    }

    private void StartCrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void StopCrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }

    private IEnumerator Slide()
    {
        slideReady = false;
        sliding = true;

        rb.drag = 0;

        // Calculate slide direction and apply it
        Vector3 slideDirection = moveDirection.normalized * slideSpeed;
        rb.velocity = new Vector3(slideDirection.x, rb.velocity.y, slideDirection.z);

        yield return new WaitForSeconds(1f);

        sliding = false;
        rb.drag = groundDrag;

        yield return new WaitForSeconds(slideCooldown);
        slideReady = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Reset jump count on ground contact
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpCount = 0;
        }
    }

    private IEnumerator SlowMotion()
    {
        isInSlowMotion = true;
        Time.timeScale = slowMotionFactor;
        yield return new WaitForSecondsRealtime(slowMotionDuration);
        Time.timeScale = 1f;
        isInSlowMotion = false;
    }

    private bool CheckVault()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 direction = orientation.forward;

        if (Physics.Raycast(origin, direction, out hit, vaultRange))
        {
            float obstacleHeight = hit.point.y - transform.position.y;
            if (obstacleHeight < vaultHeight)
            {
                return true;
            }
        }
        return false;
    }

    private void StartVault()
    {
        isVaulting = true;
        vaultTime = 0f;
        rb.isKinematic = true;

        vaultStartPos = transform.position;
        vaultEndPos = vaultStartPos + orientation.forward * 1f + Vector3.up * 1f;

        if (vaultAnim != null)
        {
            vaultAnim.Play("Vault");
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        // Activate particle effects
        if (dashParticleSystem != null)
        {
            dashParticleSystem.Play();
        }

        float originalDrag = rb.drag;
        rb.drag = 0;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        Vector3 dashDir = moveDirection.magnitude > 0.1f
            ? moveDirection.normalized
            : orientation.forward;

        rb.AddForce(dashDir * dashForce, ForceMode.Impulse);

        yield return new WaitForSeconds(dashDuration);

        rb.drag = originalDrag;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
        isDashing = false;
    }
}
