using Fusion;
using UnityEngine;

public struct PlayerInputData : INetworkInput
{
    public Vector2 move;
    public NetworkButtons buttons;
}