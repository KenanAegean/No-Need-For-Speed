using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FinishLine : NetworkBehaviour
{
    public GameCoreManager gameCoreManager;
    public bool useSpecificDirection = false;
    public AllowedDirection allowedDirections = AllowedDirection.Any;
    private Dictionary<ulong, bool> hasCrossedFinishLine = new Dictionary<ulong, bool>();

    [System.Flags]
    public enum AllowedDirection
    {
        None = 0,
        Forward = 1 << 0,   // Positive Z
        Backward = 1 << 1,  // Negative Z
        Right = 1 << 2,     // Positive X
        Left = 1 << 3,      // Negative X
        Up = 1 << 4,        // Positive Y
        Down = 1 << 5,      // Negative Y
        Any = Forward | Backward | Right | Left | Up | Down
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && (other.CompareTag("Player") || other.CompareTag("Player2") || other.CompareTag("NPC")))
        {
            var networkObject = other.GetComponent<NetworkObject>();
            if (networkObject == null) return;

            ulong networkObjectId = networkObject.NetworkObjectId;

            // Calculate player's relative position to the finish line
            Vector3 relativePosition = other.transform.position - transform.position;
            Vector3 finishLineForward = transform.forward;

            // Check if the player is on the forward or backward side
            bool isBehindFinishLine = Vector3.Dot(relativePosition, finishLineForward) < 0;

            // If the player is on the forward side of the finish line, ignore the crossing
            if (!isBehindFinishLine)
            {
                Debug.Log($"Entity {networkObjectId} attempted to cross from the forward side. Crossing is invalid.");
                return;
            }

            // Optional: Validate specific movement direction
            Vector3 playerDirection = other.transform.forward.normalized; // Movement direction
            if (useSpecificDirection && Vector3.Dot(playerDirection, finishLineForward) < 0)
            {
                Debug.Log($"Entity {networkObjectId} is moving backward but is behind the finish line. Crossing counts.");
            }
            else if (useSpecificDirection && !IsDirectionValid(playerDirection))
            {
                Debug.Log($"Entity {networkObjectId} is behind the finish line but moving in an invalid direction.");
                return;
            }

            // Ensure only valid crossings increment the tour count
            if (!hasCrossedFinishLine.ContainsKey(networkObjectId))
            {
                hasCrossedFinishLine[networkObjectId] = true;
                Debug.Log($"Entity {networkObjectId} crossed the finish line for the first time.");
                return; // Skip updating tour count for first-time crossing
            }

            gameCoreManager.PlayerReachedFinishLine(networkObjectId, other.CompareTag("NPC"));
        }
    }




    private bool IsDirectionValid(Vector3 playerDirection)
    {
        if ((allowedDirections & AllowedDirection.Forward) == AllowedDirection.Forward && Vector3.Dot(playerDirection, Vector3.forward) > 0.5f)
            return true;
        if ((allowedDirections & AllowedDirection.Backward) == AllowedDirection.Backward && Vector3.Dot(playerDirection, Vector3.back) > 0.5f)
            return true;
        if ((allowedDirections & AllowedDirection.Right) == AllowedDirection.Right && Vector3.Dot(playerDirection, Vector3.right) > 0.5f)
            return true;
        if ((allowedDirections & AllowedDirection.Left) == AllowedDirection.Left && Vector3.Dot(playerDirection, Vector3.left) > 0.5f)
            return true;
        if ((allowedDirections & AllowedDirection.Up) == AllowedDirection.Up && Vector3.Dot(playerDirection, Vector3.up) > 0.5f)
            return true;
        if ((allowedDirections & AllowedDirection.Down) == AllowedDirection.Down && Vector3.Dot(playerDirection, Vector3.down) > 0.5f)
            return true;

        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyFinishLineServerRpc(ulong clientId)
    {
        if (gameCoreManager == null)
        {
            Debug.LogError("GameCoreManager reference is missing in FinishLine!");
            return;
        }
    }
}
