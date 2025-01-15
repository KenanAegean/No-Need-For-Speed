using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;

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

        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // Ensure spawn points and prefabs are set
            if (spawnPoints == null || spawnPoints.Length == 0 || playerPrefabs == null || playerPrefabs.Length == 0)
            {
                Debug.LogError("Spawn points or player prefabs are not assigned.");
                return;
            }

            // Choose the spawn point and player prefab based on clientId
            Transform spawnPoint = spawnPoints[(int)(clientId % (ulong)spawnPoints.Length)];
            GameObject playerPrefab = playerPrefabs[(int)(clientId % (ulong)playerPrefabs.Length)];

            // Spawn the player
            GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

            // Set Camera
            var vcam = GetComponent<CinemachineCamera>();
            if (vcam != null)
            {
                var targets = GameObject.FindGameObjectsWithTag("Player");
                if (targets.Length > 0)
                    vcam.Target.TrackingTarget = targets[0].transform;
            }
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;
        }
    }
}
