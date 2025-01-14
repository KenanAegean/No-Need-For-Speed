using Unity.Netcode;
using UnityEngine;

public class GameCoreManager : NetworkBehaviour
{
    public GameSceneManager gameSceneManager; // Reference to GameSceneManager
    public int totalToursRequired = 3; // Total tours required to finish the game

    private NetworkVariable<int> currentTours = new NetworkVariable<int>(0); // Track completed tours across the network

    public void PlayerReachedFinishLine()
    {
        if (IsServer)
        {
            currentTours.Value++; // Update the tour count on the server
            Debug.Log($"Player reached finish line! Tour: {currentTours.Value}/{totalToursRequired}");

            // Update the UI on all clients
            UpdateInGamePanelClientRpc(currentTours.Value, totalToursRequired);

            // Check if the game should end
            if (currentTours.Value >= totalToursRequired)
            {
                EndGame();
            }
        }
    }

    private void EndGame()
    {
        if (gameSceneManager == null)
        {
            Debug.LogError("GameSceneManager reference is missing in GameCoreManager!");
            return;
        }

        Debug.Log("Game Over! All tours completed.");
        ShowEndGamePanelClientRpc();
    }

    [ClientRpc]
    private void UpdateInGamePanelClientRpc(int currentTours, int totalToursRequired)
    {
        if (gameSceneManager != null)
        {
            gameSceneManager.UpdateInGamePanelText(currentTours, totalToursRequired);
        }
        else
        {
            Debug.LogError("GameSceneManager reference is missing in GameCoreManager!");
        }
    }

    [ClientRpc]
    private void ShowEndGamePanelClientRpc()
    {
        if (gameSceneManager != null)
        {
            gameSceneManager.ShowEndGamePanel();
        }
        else
        {
            Debug.LogError("GameSceneManager reference is missing in GameCoreManager!");
        }
    }
}
