using UnityEngine;
using System.Collections;
using Fusion;

public class PortalTeleport : MonoBehaviour
{

    [Header("Teleport Target")]
    public Transform targetPoint;

    [Header("Effect")]
    public GameObject effectObject;

    [Header("Hold To Teleport")]
    public float holdTime = 1.5f;

    [Header("Portal Settings")]
    public bool enableDoubleJump = false; // Portal 1 = false, Portal 2 = true

    private float currentHoldTime = 0f;
    private bool isTeleporting = false;

    private void Start()
    {
        if (effectObject != null)
            effectObject.SetActive(false);
    }

    // ? Ch?m là b?t effect ngay
    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null) return;
        if (!player.HasInputAuthority) return;

        if (effectObject != null)
            effectObject.SetActive(true);
    }

    private void OnTriggerStay(Collider other)
    {
        if (isTeleporting) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null) return;
        if (!player.HasInputAuthority) return;

        currentHoldTime += Time.deltaTime;

        if (currentHoldTime >= holdTime)
        {
            // ?? x? lý double jump NGAY TR??C khi teleport
            if (enableDoubleJump)
            {
                player.RPC_EnableDoubleJump();
            }
            else
            {
                player.RPC_DisableDoubleJump();
            }

            StartCoroutine(TeleportRoutine(player));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null) return;

        // reset khi r?i portal
        currentHoldTime = 0f;

        // ? n?u r?i s?m thì t?t effect
        if (!isTeleporting && effectObject != null)
            effectObject.SetActive(false);
    }

    private IEnumerator TeleportRoutine(PlayerMovement player)
    {
        isTeleporting = true;

        yield return new WaitForSeconds(0.5f);

        CharacterController character = player.GetComponent<CharacterController>();
        NetworkTransform networkTransform = player.GetComponent<NetworkTransform>();

        if (character != null) character.enabled = false;
        if (networkTransform != null) networkTransform.enabled = false;

        Vector3 pos = targetPoint.position;
        if (character != null)
            pos.y += character.height / 2f;

        player.transform.position = pos;

        player.CurrentHorizontalVelocity = Vector3.zero;
        player.VerticalVelocity = 0f;

        if (character != null) character.enabled = true;

        if (networkTransform != null)
        {
            networkTransform.enabled = true;
            networkTransform.Teleport(player.transform.position, player.transform.rotation);
        }

        // ? Sau khi teleport ? t?t effect
        if (effectObject != null)
            effectObject.SetActive(false);

        currentHoldTime = 0f;
        isTeleporting = false;
    }
}