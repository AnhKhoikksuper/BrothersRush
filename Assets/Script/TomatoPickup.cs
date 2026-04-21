using Fusion;
using UnityEngine;

public class TomatoPickup : NetworkBehaviour
{
    public int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;

        TomatoPlayer player = other.GetComponent<TomatoPlayer>();

        if (player != null)
        {
            player.AddTomato(1);
            Runner.Despawn(Object);
        }
    }
}