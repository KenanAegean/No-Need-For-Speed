using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform[] spawnPoints; // Assign spawn points here

    [Header("Player Prefabs")]
    public GameObject[] playerPrefabs; // Array for multiple player prefabs

    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager is not initialized.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Spawn points are not assigned.");
            return;
        }

        if (playerPrefabs == null || playerPrefabs.Length == 0)
        {
            Debug.LogError("Player prefabs are not assigned.");
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // Server spawns the player
            SpawnPlayer(clientId);

            // Increment totalPlayers in GameCoreManager
            var gameCoreManager = Object.FindFirstObjectByType<GameCoreManager>(); // Updated to use the new method
            if (gameCoreManager != null)
            {
                gameCoreManager.totalPlayers.Value++;
            }
            else
            {
                Debug.LogError("GameCoreManager is not found in the scene.");
            }

            Debug.Log($"Client connected: {clientId}");
        }
    }


    private void SpawnPlayer(ulong clientId)
    {
        if (spawnPoints == null || spawnPoints.Length == 0 || playerPrefabs == null || playerPrefabs.Length == 0)
        {
            Debug.LogError("Spawn points or player prefabs are not assigned.");
            return;
        }

        // Determine the spawn point and prefab
        Transform spawnPoint = spawnPoints[(int)(clientId % (ulong)spawnPoints.Length)];
        GameObject playerPrefab = playerPrefabs[(int)(clientId % (ulong)playerPrefabs.Length)];

        // Instantiate the player and spawn it on the network
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

    }


    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}
