using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class TeleportAbility : NetworkBehaviour
{
    public float teleportDistance = 5f; // Distance to teleport
    public float cooldownTime = 3f; // Cooldown time in seconds
    public LayerMask teleportCollisionLayers; // Layers to consider for collision detection

    private float remainingCooldown;

    private float lastTeleportTime;

    void Start()
    {
        remainingCooldown = 0f;
    }

    void Update()
    {
        if (!IsLocalPlayer)
            return;

        // If cooldown is active, update remaining time
        if (remainingCooldown > 0)
        {
            remainingCooldown -= Time.deltaTime;
            remainingCooldown = Mathf.Max(remainingCooldown, 0f);
        }

        // Check if the player presses the designated key (E) and cooldown is over
        if (Keyboard.current.eKey.wasPressedThisFrame && remainingCooldown <= 0)
        {
            Vector3 teleportDirection = transform.forward * teleportDistance;
            Vector3 teleportPosition = transform.position + teleportDirection;

            TeleportPlayer(teleportPosition);
        }
    }

    void TeleportPlayer(Vector3 teleportPosition)
    {
        RaycastHit hit;
        if (Physics.Raycast(teleportPosition, Vector3.down, out hit, Mathf.Infinity, teleportCollisionLayers))
        {
            // Adjust the teleport position to be just above the ground
            teleportPosition = hit.point + Vector3.up * 0.1f; // You can adjust the height as needed
        }

        // Teleport the player
        transform.position = teleportPosition;

        // Update last teleport time
        lastTeleportTime = Time.time;

        // Start cooldown
        remainingCooldown = cooldownTime;

        // Notify clients about cooldown start
        CooldownClientRpc();
    }

    void CmdTeleportPlayer(Vector3 teleportPosition)
    {
        TeleportPlayer(teleportPosition);
    }

    [ClientRpc]
    void CooldownClientRpc()
    {
        // This function is called on all clients to start cooldown
        lastTeleportTime = Time.time;
    }
}



