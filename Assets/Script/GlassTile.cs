using UnityEngine;
using Fusion;

public class GlassTile : NetworkBehaviour
{
    [Networked] public bool isSafe { get; set; }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger v?i: " + other.name); // ?? debug

        if (!Object.HasStateAuthority) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player ?ã ch?m kính");

            if (!isSafe)
            {
                Debug.Log("Kính sai!");

                var player = other.GetComponent<PlayerMovement>();

                if (player != null)
                {
                    player.RpcHitWrongGlass();
                }
            }
        }
    }
}