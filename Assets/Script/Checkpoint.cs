using UnityEngine;
using Fusion;

public class Checkpoint : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerMovement>();

        if (player != null && player.HasStateAuthority)
        {
            Vector3 safePos = transform.position + Vector3.up * 10f; // 🔥 nâng lên

            player.SetCheckpoint(safePos);

            Debug.Log("Checkpoint saved: " + safePos);
            Debug.Log("đã save");
        }
    }
}