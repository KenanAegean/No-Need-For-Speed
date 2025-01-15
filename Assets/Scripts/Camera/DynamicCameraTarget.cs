using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;

public class DynamicCameraTarget : MonoBehaviour
{
    [Header("Cinemachine Camera")]
    public CinemachineVirtualCamera cinemachineCamera; // Reference to the Cinemachine Virtual Camera

    private void OnEnable()
    {
        // Subscribe to NetworkManager's player spawn events
        NetworkManager.Singleton.OnClientConnectedCallback += AssignCameraToPlayer;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid errors when this object is destroyed
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= AssignCameraToPlayer;
        }
    }

    private void AssignCameraToPlayer(ulong clientId)
    {
        // Only assign the camera to the local player
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            var localPlayerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            if (localPlayerObject != null)
            {
                // Set the Cinemachine camera to follow and look at the local player
                cinemachineCamera.Follow = localPlayerObject.transform;
                cinemachineCamera.LookAt = localPlayerObject.transform;
                Debug.Log($"Cinemachine camera assigned to local player: {clientId}");
            }
            else
            {
                Debug.LogError("Local player object is null!");
            }
        }
    }
}
