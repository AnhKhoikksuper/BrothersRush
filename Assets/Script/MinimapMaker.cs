using UnityEngine;
using Fusion;

public class MiniMapMarker : NetworkBehaviour
{
    public Transform marker;

    void Update()
    {
        if (marker == null) return;

        Vector3 pos = transform.position;

        marker.position = new Vector3(
            pos.x,
            marker.position.y,
            pos.z
        );
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            marker.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            marker.GetComponent<Renderer>().material.color = Color.blue;
        }
    }
}