using Unity.Netcode;
using UnityEngine;

public class NPCMovement : NetworkBehaviour
{
    public Transform[] waypoints; // Waypoints for the NPC to follow
    public float moveSpeed = 5f;  // Movement speed

    private int currentWaypointIndex = 0;
    private bool isMoving = false; // Initially no movement

    private void Update()
    {
        if (!IsServer || !isMoving || waypoints.Length == 0) return;

        // Move toward the current waypoint ignoring Y value
        Transform target = waypoints[currentWaypointIndex];
        Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Check if the waypoint is reached
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleMovementServerRpc()
    {
        isMoving = !isMoving; // Toggle movement
        UpdateMovementStateClientRpc(isMoving);
    }

    [ClientRpc]
    private void UpdateMovementStateClientRpc(bool state)
    {
        isMoving = state;
    }
}
