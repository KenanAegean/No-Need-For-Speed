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
    private Dictionary<ulong, int> npcTours = new Dictionary<ulong, int>();

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

    [ClientRpc]
    private void UpdatePlayerToursClientRpc(ulong networkObjectId, int toursCompleted, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"[ClientRpc] Updating tours for Player {networkObjectId}: {toursCompleted} tours completed.");
        if (NetworkManager.Singleton.LocalClientId == networkObjectId) // Ensure only the local client updates their UI
        {
            string playerName = GetNetworkName(networkObjectId);
            gameSceneManager.UpdatePlayerTourText(playerName, toursCompleted, totalToursRequired);
        }
    }

    [ClientRpc]
    private void UpdateTourTextClientRpc(ulong networkObjectId, int currentTours, int totalTours)
    {
        var networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        if (networkObject != null)
        {
            // Find the TextMeshPro component
            var tourText = networkObject.transform.Find("TourCounter")?.GetComponent<TMPro.TextMeshPro>();
            if (tourText != null)
            {
                tourText.text = $"Tour: {currentTours}/{totalTours}";
            }
        }
    }

    public void PlayerReachedFinishLine(ulong networkObjectId, bool isNpc)
    {
        if (isNpc)
        {
            if (!npcTours.ContainsKey(networkObjectId))
            {
                npcTours[networkObjectId] = 0;
            }

            npcTours[networkObjectId]++;
            Debug.Log($"NPC {networkObjectId} completed a tour! ({npcTours[networkObjectId]}/{totalToursRequired})");

            // Update the NPC's text
            UpdateTourTextClientRpc(networkObjectId, npcTours[networkObjectId], totalToursRequired);
            
            // Check for game over
            if (npcTours[networkObjectId] >= totalToursRequired)
            {
                Debug.Log($"Game over! NPC {networkObjectId} wins!");
                ShowWinnerClientRpc(networkObjectId, true);
            }
        }
        else
        {
            if (!playerTours.ContainsKey(networkObjectId))
            {
                playerTours[networkObjectId] = 0;
            }

            playerTours[networkObjectId]++;
            Debug.Log($"Player {networkObjectId} completed a tour! ({playerTours[networkObjectId]}/{totalToursRequired})");

            // Update the player's text
            UpdateTourTextClientRpc(networkObjectId, playerTours[networkObjectId], totalToursRequired);

            // Check for game over
            if (playerTours[networkObjectId] >= totalToursRequired)
            {
                Debug.Log($"Game over! {networkObjectId} wins!");
                ShowWinnerClientRpc(networkObjectId, false);
            }
        }
    }


    [ClientRpc]
    private void ShowWinnerClientRpc(ulong winnerId, bool isNpc)
    {
        if (gameSceneManager != null)
        {
            string winnerName = GetNetworkName(winnerId);
            string winnerType = isNpc ? "NPC" : "Player";
            gameSceneManager.ShowWinner($"{winnerType}: {winnerName}");
        }
    }

    private string GetNetworkName(ulong networkObjectId)
    {
        var networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        if (networkObject != null)
        {
            var nameComponent = networkObject.transform.Find("NetworkName")?.GetComponent<TextMeshPro>();
            if (nameComponent != null)
            {
                return nameComponent.text;
            }
        }
        return "Unknown"; // Fallback if no name is found
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
