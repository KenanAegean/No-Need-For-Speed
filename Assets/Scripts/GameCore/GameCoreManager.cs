using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class GameCoreManager : NetworkBehaviour
{
    public GameSceneManager gameSceneManager; // Reference to GameSceneManager
    public int totalToursRequired = 3; // Total tours required to finish the game

    private NetworkVariable<int> currentTours = new NetworkVariable<int>(0); // Track completed tours across the network

    public NetworkVariable<int> readyCount = new NetworkVariable<int>(0);
    public NetworkVariable<int> totalPlayers = new NetworkVariable<int>(0);

    private Dictionary<ulong, int> playerTours = new Dictionary<ulong, int>();

    [SerializeField] private TMP_Text readyText;
    [SerializeField] private TMP_Text countdownText;

    private bool isCountdownStarted = false;
    private bool hasPressedReady = false;

    private void Start()
    {
        if (IsServer)
        {
            totalPlayers.Value = 1; // Add the host to the player count
        }

        UpdateReadyCountClientRpc(readyCount.Value, totalPlayers.Value);
        readyText.text = "Press R To Be Ready";
        countdownText.text = "";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !hasPressedReady && !isCountdownStarted)
        {
            hasPressedReady = true; // Prevent pressing "R" again
            SetReadyServerRpc();
        }

        if (IsServer && readyCount.Value == totalPlayers.Value && !isCountdownStarted)
        {
            StartCountdown();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        if (readyCount.Value < totalPlayers.Value)
        {
            readyCount.Value++;
            UpdateReadyCountClientRpc(readyCount.Value, totalPlayers.Value);

            Debug.Log($"Player {rpcParams.Receive.SenderClientId} is ready. Ready count: {readyCount.Value}/{totalPlayers.Value}");
        }
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
    private void UpdateReadyCountClientRpc(int ready, int total)
    {
        readyText.text = $"Ready: {ready}/{total}";
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
            Debug.Log($"Starting movement for NPC: {npc.gameObject.name}"); // Debug log
            npc.ToggleMovementServerRpc(true);
        }
    }

    public void PlayerReachedFinishLine(ulong clientId)
    {
        if (!playerTours.ContainsKey(clientId))
        {
            playerTours[clientId] = 0;
        }

        playerTours[clientId]++;
        Debug.Log($"Player {clientId} completed a tour! ({playerTours[clientId]}/{totalToursRequired})");

        // Update UI for all clients
        UpdatePlayerToursClientRpc(clientId, playerTours[clientId]);

        // Check for game over
        if (playerTours[clientId] >= totalToursRequired)
        {
            Debug.Log($"Game over! Player {clientId} wins!");
            ShowWinnerClientRpc(clientId);
        }
    }

    [ClientRpc]
    private void UpdatePlayerToursClientRpc(ulong clientId, int toursCompleted)
    {
        // Update in-game panel for this player
        if (gameSceneManager != null)
        {
            gameSceneManager.UpdatePlayerTourText(clientId, toursCompleted, totalToursRequired);
        }
    }

    [ClientRpc]
    private void ShowWinnerClientRpc(ulong clientId)
    {
        if (gameSceneManager != null)
        {
            gameSceneManager.ShowWinner(clientId);
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
