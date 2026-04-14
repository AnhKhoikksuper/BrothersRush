using UnityEngine;
using Fusion;

public class MinimapFollow : MonoBehaviour
{
    public string targetChildName = "CameraTarget"; // Tên object con trong Player
    public float height = 25f; // Độ cao của camera minimap

    private Transform _target;

    void LateUpdate()
    {
        // 1. Tìm Local Player nếu chưa có
        if (_target == null)
        {
            foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
            {
                var networkObj = player.GetComponent<NetworkObject>();
                if (networkObj != null && networkObj.HasInputAuthority)
                {
                    _target = player.transform.Find(targetChildName);
                    break;
                }
            }
            return;
        }

        // 2. Follow Player (X, Z + Y động theo Player)
        float dynamicHeight = _target.position.y + height;

        transform.position = new Vector3(
            _target.position.x,
            dynamicHeight,
            _target.position.z
        );

        // 3. Xoay theo Player
        transform.rotation = Quaternion.Euler(90f, _target.eulerAngles.y, 0f);
    }
}