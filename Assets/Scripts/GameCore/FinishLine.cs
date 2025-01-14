using UnityEngine;

public class FinishLine : MonoBehaviour
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
        if (other.CompareTag("Player"))
        {
            if (!useSpecificDirection || allowedDirections == AllowedDirection.Any)
            {
                gameCoreManager.PlayerReachedFinishLine();
                return;
            }

            Vector3 playerDirection = (other.transform.position - transform.position).normalized;
            if (IsDirectionValid(playerDirection))
            {
                gameCoreManager.PlayerReachedFinishLine();
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
}