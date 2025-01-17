using Unity.Netcode;
using UnityEngine;

public class NPCManager : NetworkBehaviour
{
    public GameObject npcPrefab;      // NPC prefab
    public Transform spawnPoint;      // Spawn point for the NPC
    public Transform waypointParent;  // Parent of waypoints

    private GameObject npcInstance;   // Instance of the spawned NPC

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N) && IsServer)
        {
            ResetOrSpawnNPC();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleNPCMovement();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetOrSpawnNPCServerRpc()
    {
        if (npcInstance != null)
        {
            // Reset NPC position and stop movement
            npcInstance.transform.position = spawnPoint.position;
            npcInstance.transform.rotation = spawnPoint.rotation;
            var npcMovement = npcInstance.GetComponent<NPCMovement>();
            npcMovement.ToggleMovementServerRpc(); // Stop movement
        }
        else
        {
            // Spawn a new NPC
            npcInstance = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);
            var npcMovement = npcInstance.GetComponent<NPCMovement>();
            npcMovement.waypointParent = waypointParent; // Assign waypoints
            npcInstance.GetComponent<NetworkObject>().Spawn();
        }
    }

    private void ResetOrSpawnNPC()
    {
        ResetOrSpawnNPCServerRpc();

        if (IsServer)
        {
            FindFirstObjectByType<GameCoreManager>().readyCount.Value++;
            FindFirstObjectByType<GameCoreManager>().totalPlayers.Value++;

        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleNPCMovementServerRpc()
    {
        if (npcInstance != null)
        {
            npcInstance.GetComponent<NPCMovement>().ToggleMovementServerRpc();
        }
    }

    private void ToggleNPCMovement()
    {
        ToggleNPCMovementServerRpc();
    }
}