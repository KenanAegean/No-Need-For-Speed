using Unity.Netcode;
using UnityEngine;

public class NPCMovement : NetworkBehaviour
{
    public Transform waypointParent; // Parent object containing waypoints
    public float moveSpeed = 5f;     // Movement speed
    public float rotationSpeed = 5f; // Rotation speed

    private Transform[] waypoints;  // Array of waypoints
    private int currentWaypointIndex = 0;
    private bool isMoving = false;  // Initially no movement

    private void Start()
    {
        // Populate waypoints array from the parent's children
        if (waypointParent != null)
        {
            waypoints = new Transform[waypointParent.childCount];
            for (int i = 0; i < waypointParent.childCount; i++)
            {
                waypoints[i] = waypointParent.GetChild(i);
            }
        }
    }

    private void Update()
    {
        if (!IsServer || !isMoving || waypoints == null || waypoints.Length == 0) return;

        // Move toward the current waypoint ignoring Y value
        Transform target = waypoints[currentWaypointIndex];
        Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        Vector3 direction = targetPosition - transform.position;

        // Move toward the waypoint
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Rotate to face the waypoint
        if (direction.sqrMagnitude > 0.01f) // Ensure direction vector is not zero
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Check if the waypoint is reached
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length; // Loop back to first waypoint
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleMovementServerRpc(bool forceStart = false)
    {
        if (forceStart)
        {
            isMoving = true;
        }
        else
        {
            isMoving = !isMoving;
        }

        UpdateMovementStateClientRpc(isMoving);
    }

    [ClientRpc]
    private void UpdateMovementStateClientRpc(bool state)
    {
        isMoving = state;
    }
}
