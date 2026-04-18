using Fusion;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    public static PlayerData Local;

    [Networked] public NetworkString<_32> PlayerName { get; set; }

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            Local = this;
        }
    }

    public void SetName(string playerName)
    {
        if (HasStateAuthority)
        {
            PlayerName = playerName;
        }
    }
}