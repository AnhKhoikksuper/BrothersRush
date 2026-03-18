using UnityEngine;
using Fusion;

public class PlayerRunner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject playerPrefab;
    
    [Header("Spawn Settings")]
    [Tooltip("Độ cao cố định khi nhân vật xuất hiện")]
    [SerializeField] private float spawnHeightY = 1.0f; 
    
    [SerializeField] private float minRange = -2f;
    [SerializeField] private float maxRange = 10f;

    public void PlayerJoined(PlayerRef player)
    {
        // Kiểm tra nếu là người chơi cục bộ (Local Player)
        if (player == Runner.LocalPlayer)
        {
            // Tạo vị trí ngẫu nhiên trên mặt phẳng XZ, nhưng giữ nguyên độ cao Y theo biến đã set
            Vector3 spawnPosition = new Vector3(
                Random.Range(minRange, maxRange), 
                spawnHeightY, 
                Random.Range(minRange, maxRange)
            );

            // Spawn nhân vật với quyền Input Authority cho người chơi
            Runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        }
    }
}