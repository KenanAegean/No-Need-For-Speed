using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Linq;

public class GameCoreManager : NetworkBehaviour
{
    public GameSceneManager gameSceneManager; // Reference to GameSceneManager
    public int totalToursRequired = 3; // Total tours required to finish the game

    private NetworkVariable<int> currentTours = new NetworkVariable<int>(0); // Track completed tours across the network

    public NetworkVariable<int> readyCount = new NetworkVariable<int>(0);
    public NetworkVariable<int> totalPlayers = new NetworkVariable<int>(0);

    [SerializeField] private TMP_Text readyText;
    [SerializeField] private TMP_Text countdownText;

    private bool isCountdownStarted = false;

    private void Start()
    {
        if (IsServer)
        {
            totalPlayers.Value = 1; // Host is automatically added as a player
        }

        readyText.text = "Press R To Be Ready";
        countdownText.text = "";
    }

    private void Update()
    {
        // Allow only the local player to mark themselves as ready
        if (IsOwner && Input.GetKeyDown(KeyCode.R) && !isCountdownStarted)
        {
            SetReadyServerRpc();
        }

        // Check if all players are ready on the server
        if (IsServer && readyCount.Value == totalPlayers.Value && !isCountdownStarted)
        {
            StartCountdown();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReadyServerRpc()
    {
        if (readyCount.Value < totalPlayers.Value)
        {
            readyCount.Value++; // Increment ready count
        }
        else if (readyCount.Value > 0)
        {
            readyCount.Value--; // Decrement ready count
        }
        UpdateReadyCountClientRpc(readyCount.Value, totalPlayers.Value);
        Debug.Log($"Ready count updated. Ready count: {readyCount.Value}/{totalPlayers.Value}");
    }

    [ClientRpc]
    private void UpdateReadyCountClientRpc(int ready, int total)
    {
        readyText.text = $"Ready: {ready}/{total}";
    }

    private void StartCountdown()
    {
        isCountdownStarted = true;
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        int countdown = 3;
        while (countdown > 0)
        {
            UpdateCountdownClientRpc(countdown.ToString());
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        UpdateCountdownClientRpc("Go!");
        EnableMovementServerRpc();
        yield return new WaitForSeconds(1f);
        HideCountdownClientRpc();
    }

    [ClientRpc]
    private void UpdateCountdownClientRpc(string message)
    {
        countdownText.text = message;
    }

    [ClientRpc]
    private void HideCountdownClientRpc()
    {
        countdownText.text = "";
        readyText.text = "";
    }

    [ServerRpc(RequireOwnership = false)]
    private void EnableMovementServerRpc()
    {
        EnableMovementClientRpc();
    }

    [ClientRpc]
    private void EnableMovementClientRpc()
    {
        foreach (var player in Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            player.EnableMovement();
        }

        foreach (var npc in Object.FindObjectsByType<NPCMovement>(FindObjectsSortMode.None))
        {
            npc.ToggleMovementServerRpc();
        }

    }

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
