using Fusion;
using UnityEngine;

public class TomatoProjectile : NetworkBehaviour
{
    public float speed = 15f;
    public float lifeTime = 3f;

    private TickTimer lifeTimer;

    public override void Spawned()
    {
        lifeTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
    }

    public override void FixedUpdateNetwork()
    {
        transform.position += transform.forward * speed * Runner.DeltaTime;

        if (lifeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;

        TomatoPlayer player = other.GetComponent<TomatoPlayer>();

        if (player != null)
        {
            player.RPC_Hit();
        }

        Runner.Despawn(Object);
    }
}