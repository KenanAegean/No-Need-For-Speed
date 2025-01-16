using Unity.Netcode;
using UnityEngine;

public class NPCManager : NetworkBehaviour
{
    public GameObject npcPrefab;      // NPC prefab
    public Transform spawnPoint;      // Spawn point for the NPC
    public Transform[] waypoints;     // Waypoints for the NPC to follow

    private GameObject npcInstance;   // Instance of the spawned NPC

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N) && IsServer)
        {
            SpawnNPC();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleNPCMovement();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnNPCServerRpc()
    {
        if (npcInstance != null) return;

        npcInstance = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);
        npcInstance.GetComponent<NPCMovement>().waypoints = waypoints;
        npcInstance.GetComponent<NetworkObject>().Spawn();
    }

    private void SpawnNPC()
    {
        if (IsServer)
        {
            SpawnNPCServerRpc();
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
