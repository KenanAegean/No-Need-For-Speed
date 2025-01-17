using Unity.Netcode;
using UnityEngine;

public class FinishLine : NetworkBehaviour
{
    public GameCoreManager gameCoreManager; // Reference to GameCoreManager
    public bool useSpecificDirection = false; // Toggle to enable/disable direction checking
    public AllowedDirection allowedDirections = AllowedDirection.Any; // Default to all directions

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
        // Check if the object that triggered the collider is a player
        if (IsServer && (other.CompareTag("Player") || other.CompareTag("Player2") || other.CompareTag("NPC")))
        {
            // Direction validation logic
            if (!useSpecificDirection || allowedDirections == AllowedDirection.Any)
            {
                NotifyFinishLineServerRpc(other.GetComponent<NetworkObject>().OwnerClientId);
                return;
            }

            Vector3 playerDirection = (other.transform.position - transform.position).normalized;
            if (IsDirectionValid(playerDirection))
            {
                NotifyFinishLineServerRpc(other.GetComponent<NetworkObject>().OwnerClientId);
            }
            else
            {
                Debug.Log("Player crossed the finish line from the wrong direction.");
            }
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

        gameCoreManager.PlayerReachedFinishLine();
    }
}
