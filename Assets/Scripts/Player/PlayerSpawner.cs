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
            Debug.Log($"Client Connceted");
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

        // Handle camera assignment on the local client
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
           // StartCoroutine(AssignCameraToPlayer(player));
        }
        else
        {
            Debug.Log($"Problem for {player.name}.");
            //StartCoroutine(AssignCameraToPlayer(player));
        }
    }

    private IEnumerator AssignCameraToPlayer(GameObject player)
    {
        yield return new WaitForEndOfFrame(); // Ensure the player object is fully initialized

        // Find the Cinemachine Virtual Camera
        var vcam = FindObjectOfType<CinemachineCamera>();
        if (vcam == null)
        {
            Debug.LogError("No CinemachineCamera found in the scene!");
            yield break;
        }

        // Check if this is the local player
        var networkObject = player.GetComponent<NetworkObject>();
        if (networkObject != null && networkObject.IsLocalPlayer)
        {
            Transform cameraTarget = player.transform;
            vcam.Target.TrackingTarget = cameraTarget; // Assign target
            Debug.Log($"Camera target assigned to {player.name}.");
        }
        else
        {
            Transform cameraTarget = player.transform;
            vcam.Target.TrackingTarget = cameraTarget; // Assign target
            Debug.LogError("Player not found or not the local player .");
            Debug.Log($"{networkObject.name}.");
            Debug.Log($"{player.name}.");
        }
    }


    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}
