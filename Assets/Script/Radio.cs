using UnityEngine;
using Fusion;

public class Radio : NetworkBehaviour
{
    [SerializeField] private AudioClip musicClip;

    public AudioClip GetClip()
    {
        return musicClip;
    }
}