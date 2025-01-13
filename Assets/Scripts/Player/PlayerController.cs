using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Car Settings")]
    public float acceleration = 10000f; // Forward acceleration force.
    public float maxSpeed = 20f; // Maximum speed.
    public float turnSpeed = 50f; // Turning speed.
    public float brakeForce = 10f; // Braking force.

    private float moveInput; // Forward/backward input.
    private float turnInput; // Left/right input.

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // Prevent flipping.
    }

    private void Update()
    {
        // Get input from the old input system.
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
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
}
