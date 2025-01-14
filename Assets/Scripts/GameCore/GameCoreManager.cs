using UnityEngine;

public class GameCoreManager : MonoBehaviour
{
    public GameSceneManager gameSceneManager; // Reference to GameSceneManager
    public int totalToursRequired = 3; // Total tours required to finish the game
    private int currentTours = 0; // Track completed tours

    public void PlayerReachedFinishLine()
    {
        currentTours++;
        Debug.Log($"Player reached finish line! Tour: {currentTours}/{totalToursRequired}");

        if (gameSceneManager != null)
        {
            // Pass both currentTours and totalToursRequired as arguments
            gameSceneManager.UpdateInGamePanelText(currentTours, totalToursRequired);
        }
        else
        {
            Debug.LogError("GameSceneManager reference is missing in GameCoreManager!");
        }

        if (currentTours >= totalToursRequired)
        {
            gameSceneManager.ShowEndGamePanel();
        }
    }
}
