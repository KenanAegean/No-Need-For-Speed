using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    [Header("Car Settings")]
    public float acceleration = 10000f; // Forward acceleration force.
    public float maxSpeed = 20f; // Maximum speed.
    public float turnSpeed = 50f; // Turning speed.
    public float brakeForce = 10f; // Braking force.

    private float moveInput; // Forward/backward input.
    private float turnInput; // Left/right input.

    private Rigidbody rb;

    [Header("HELLO Text")]
    public GameObject helloText; // Assign the "HELLO" text GameObject in the Inspector.

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody is missing from PlayerController.");
            return;
        }

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        // Allow only the owner of this player object to control it
        if (!IsOwner) return;

        // Get input from the old input system.
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        // Show "HELLO" text when pressing "E"
        if (Input.GetKeyDown(KeyCode.E))
        {
            ShowHelloTextServerRpc();
        }
    }

    private void FixedUpdate()
    {
        // Allow only the owner of this player object to control it
        if (!IsOwner) return;

        HandleMovement();
    }

    private void HandleMovement()
    {
        // Forward/Backward movement.
        if (Mathf.Abs(moveInput) > 0.1f) // Only apply force if there is input.
        {
            Vector3 force = transform.forward * (moveInput * acceleration);
            rb.AddForce(force);

            // Clamp speed.
            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }

        // Steering.
        if (Mathf.Abs(turnInput) > 0.1f) // Only turn if there is input.
        {
            float steerAmount = turnInput * turnSpeed * Time.fixedDeltaTime;
            transform.Rotate(0f, steerAmount, 0f);
        }

        // Apply brakes if no forward/backward input is given.
        if (Mathf.Approximately(moveInput, 0))
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, brakeForce * Time.fixedDeltaTime);
        }
    }

    [ServerRpc]
    private void ShowHelloTextServerRpc()
    {
        ShowHelloTextClientRpc();
    }

    [ClientRpc]
    private void ShowHelloTextClientRpc()
    {
        if (helloText != null)
        {
            helloText.SetActive(true);
            Invoke(nameof(HideHelloText), 2f); // Hide after 2 seconds
        }
        else
        {
            Debug.LogWarning("Hello Text is not assigned to PlayerController.");
        }
    }

    private void HideHelloText()
    {
        if (helloText != null)
        {
            helloText.SetActive(false);
        }
    }
}
