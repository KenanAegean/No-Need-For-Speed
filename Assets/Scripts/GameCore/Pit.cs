using UnityEngine;

public class Pit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to a player
        if (other.CompareTag("Player") || other.CompareTag("Player2"))
        {
            // Get the PlayerController from the collided player
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.ChangeModelOnPit();
            }
        }
    }
}
