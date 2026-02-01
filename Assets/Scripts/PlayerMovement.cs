using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 14f;
    public float groundDrag = 4f;
    public float jumpForce = 8f;
    public float jumpCoolDown = 0.25f;
    public float airMultiplier = 0.4f;
    bool readyToJump = true;

    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight = 1f;
    public LayerMask whatIsGround;
    public LayerMask water;
    bool grounded;

    [Header("Respawn")]
    public Transform respawnPoint;
    public Transform orientation;

    [Header("Animation")]
    public float moveAnimThreshold = 0.1f;
    public string moveBoolName = "IsMoving";

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;

    Rigidbody rb;
    Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("No Animator found on Player or its children.");
        }
    }

    void Update()
    {
        grounded = Physics.SphereCast(
            transform.position,
            0.3f,
            Vector3.down,
            out RaycastHit hit,
            playerHeight * 0.5f + 0.3f,
            whatIsGround,
            QueryTriggerInteraction.Ignore
        );

        Debug.DrawRay(transform.position, Vector3.down * (playerHeight * 0.5f + 0.3f), Color.red);

        HandleInput();
        SpeedControl();

        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0f;

        if (animator != null)
        {
            Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float speed = flatVelocity.magnitude;

            bool isMoving = speed > moveAnimThreshold && grounded;
            animator.SetBool(moveBoolName, isMoving);
        }
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCoolDown);
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVelocity.magnitude > moveSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & water) != 0)
        {
            Debug.Log("Player touched water! Respawning...");
            Respawn();
        }
    }

    private void Respawn()
    {
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            Debug.LogError("Respawn point not assigned!");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float radius = 0.3f;
        float distance = playerHeight * 0.5f + 0.3f;
        Vector3 origin = transform.position;

        Gizmos.DrawLine(origin, origin + Vector3.down * distance);
        Gizmos.DrawWireSphere(origin, radius);
        Gizmos.DrawWireSphere(origin + Vector3.down * distance, radius);

        // Step rays visualization (optional)
        Gizmos.color = Color.yellow;
        Vector3 forward = (orientation != null) ? orientation.forward : transform.forward;
    }
}
