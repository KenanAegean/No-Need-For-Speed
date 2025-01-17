using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    [Header("Car Settings")]
    public float acceleration = 10000f;
    public float maxSpeed = 20f;
    public float turnSpeed = 50f;
    public float brakeForce = 10f;

    private float moveInput;
    private float turnInput;

    private Rigidbody rb;

    [Header("Emote System")]
    public Transform emoteContainer; // Assign the EmoteContainer in the Inspector.

    [Header("Model Management")]
    public Transform modelContainer;
    private int lastModelIndex = -1;

    private bool canMove = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody is missing from PlayerController.");
            return;
        }

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        // Allow only the owner of this player object to control it
        if (!IsOwner) return;

        // Get input
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        // Show random emote on "E" press
        if (Input.GetKeyDown(KeyCode.E))
        {
            ShowRandomEmoteServerRpc();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ChangeModelServerRpc();
        }
    }

    private void FixedUpdate()
    {
        // Allow only the owner of this player object to control it
        if (!IsOwner) return;

        HandleMovement();
    }
    
    private void HandleMovement()
    {
        if (!canMove) return;

        if (Mathf.Abs(moveInput) > 0.1f)
        {
            Vector3 force = transform.forward * (moveInput * acceleration);
            rb.AddForce(force);

            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }

        if (Mathf.Abs(turnInput) > 0.1f)
        {
            float steerAmount = turnInput * turnSpeed * Time.fixedDeltaTime;
            transform.Rotate(0f, steerAmount, 0f);
        }

        if (Mathf.Approximately(moveInput, 0))
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, brakeForce * Time.fixedDeltaTime);
        }
    }

    public void EnableMovement()
    {
        canMove = true;
    }

    public void ChangeModelOnPit()
    {
        if (IsOwner)
        {
            ChangeModelServerRpc();
        }
    }

    [ServerRpc]
    private void ChangeModelServerRpc()
    {
        int newModelIndex = GetRandomModelIndex();
        ActivateModelClientRpc(newModelIndex);
    }

    private int GetRandomModelIndex()
    {
        int randomIndex;

        do
        {
            randomIndex = Random.Range(0, modelContainer.childCount);
        } while (randomIndex == lastModelIndex); // Ensure a different model is selected

        lastModelIndex = randomIndex; // Update the last selected model index
        return randomIndex;
    }

    [ClientRpc]
    private void ActivateModelClientRpc(int modelIndex)
    {
        // Deactivate all models
        foreach (Transform model in modelContainer)
        {
            model.gameObject.SetActive(false);
        }

        // Activate the selected model
        if (modelContainer.childCount > modelIndex)
        {
            Transform selectedModel = modelContainer.GetChild(modelIndex);
            selectedModel.gameObject.SetActive(true);
        }
    }

    [ServerRpc]
    private void ShowRandomEmoteServerRpc()
    {
        int emoteIndex = Random.Range(0, emoteContainer.childCount);
        ShowEmoteClientRpc(emoteIndex);
    }

    [ClientRpc]
    private void ShowEmoteClientRpc(int emoteIndex)
    {
        if (emoteContainer != null)
        {
            // Deactivate all emotes
            foreach (Transform emote in emoteContainer)
            {
                emote.gameObject.SetActive(false);
            }

            // Activate the selected emote
            Transform selectedEmote = emoteContainer.GetChild(emoteIndex);
            selectedEmote.gameObject.SetActive(true);

            // Hide the emote after 2 seconds
            Invoke(nameof(HideEmotes), 2f);
        }
    }

    private void HideEmotes()
    {
        if (emoteContainer != null)
        {
            foreach (Transform emote in emoteContainer)
            {
                emote.gameObject.SetActive(false);
            }
        }
    }
}
