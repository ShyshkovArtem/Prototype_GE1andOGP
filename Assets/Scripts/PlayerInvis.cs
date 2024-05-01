using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
    private bool isInvisible = false;
    private float invisibleTimer = 0f;
    private float invisibleDuration = 1.5f;
    private float cooldownTime = 7f;
    private bool canActivate = true;

    // Update is called once per frame
    void Update()
    {
        // Only allow local player to control visibility
        if (!IsLocalPlayer)
            return;

        // Check for input to make the player invisible
        if (Input.GetKeyDown(KeyCode.Q) && !isInvisible && canActivate)
        {
            CmdMakePlayerInvisible();
        }

        // Update the invisible timer
        if (isInvisible)
        {
            invisibleTimer += Time.deltaTime;
            if (invisibleTimer >= invisibleDuration)
            {
                CmdMakePlayerVisible();
            }
        }
    }

    private void CmdMakePlayerInvisible()
    {
        isInvisible = true;
        SyncInvisibilityStateClientRpc(isInvisible);
        StartCoroutine(MakePlayerInvisible());
        canActivate = false;
        Invoke(nameof(ResetCooldown), cooldownTime);
    }

    private void CmdMakePlayerVisible()
    {
        isInvisible = false;
        SyncInvisibilityStateClientRpc(isInvisible);
    }
    [ClientRpc]
    private void SyncInvisibilityStateClientRpc(bool state)
    {
        isInvisible = state;
        if (isInvisible)
        {
            StartCoroutine(MakePlayerInvisible());
        }
        else
        {
            MakePlayerVisible();
        }
    }

    private IEnumerator MakePlayerInvisible()
    {
        // Set player to invisible
        gameObject.GetComponent<Renderer>().enabled = false;

        // Wait for the duration of invisibility
        yield return new WaitForSeconds(invisibleDuration);

        // Set player to visible again
        MakePlayerVisible();
    }

    private void MakePlayerVisible()
    {
        gameObject.GetComponent<Renderer>().enabled = true;
        invisibleTimer = 0f;
    }

    private void OnInvisibilityChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            StartCoroutine(MakePlayerInvisible());
        }
        else
        {
            MakePlayerVisible();
        }
    }

    private void ResetCooldown()
    {
        canActivate = true;
    }
}


