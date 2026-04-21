using Fusion;
using UnityEngine;

public class TomatoPickup : NetworkBehaviour
{
    public int amount = 1;

    private void OnTriggerEnter(Collider other)
    {

        TomatoPlayer player = other.GetComponent<TomatoPlayer>();

        if (player != null)
        {
            player.AddTomato(amount);
            Runner.Despawn(Object);
        }
    }
}