using Fusion;
using UnityEngine;

public class TomatoProjectile : NetworkBehaviour
{
    public float speed = 15f;
    public float lifeTime = 3f;

    private TickTimer lifeTimer;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            lifeTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // 🔥 CHỈ STATE AUTHORITY CHẠY LOGIC
        if (!Object.HasStateAuthority)
            return;

        transform.position += transform.forward * speed * Runner.DeltaTime;

        if (lifeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
            return;
        }

        // 🔥 CHECK HIT (CHỈ SERVER)
        var hits = Physics.OverlapSphere(transform.position, 0.3f);

        foreach (var hit in hits)
        {
            var player = hit.GetComponent<TomatoPlayer>();

            if (player != null && player.Object.InputAuthority != Object.InputAuthority)
            {
                player.RPC_Hit();
                Runner.Despawn(Object);
                break;
            }
        }
    }
}