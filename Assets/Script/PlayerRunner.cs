using Fusion;
using UnityEngine;

public class PlayerRunner : SimulationBehaviour
{
    public static PlayerRunner Instance;

    [Header("Skin Prefabs")]
    [SerializeField] private GameObject[] playerSkinPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnCenter;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float spawnHeightY = 1f;

    [Header("Collision Settings")]
    [SerializeField] private float minDistanceBetweenPlayers = 1.5f;
    [SerializeField] private LayerMask playerLayer;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnSelectedPlayer(int skinIndex, string playerName)
    {
        if (skinIndex < 0 || skinIndex >= playerSkinPrefabs.Length)
        {
            Debug.LogError("Skin index không hợp lệ!");
            return;
        }

        Vector3 spawnPosition = GetValidSpawnPosition();
        GameObject prefabToSpawn = playerSkinPrefabs[skinIndex];

        Runner.Spawn(prefabToSpawn, spawnPosition, Quaternion.identity, Runner.LocalPlayer,
        (runner, obj) =>
        {
            var data = obj.GetComponent<PlayerData>();
            if (data != null)
            {
                data.SetName(playerName);
            }
        });
    }

    private Vector3 GetValidSpawnPosition()
    {
        int maxAttempts = 20;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

            Vector3 candidate = new Vector3(
                spawnCenter.position.x + randomCircle.x,
                spawnHeightY,
                spawnCenter.position.z + randomCircle.y
            );

            bool isOccupied = Physics.CheckSphere(
                candidate,
                minDistanceBetweenPlayers,
                playerLayer
            );

            if (!isOccupied)
            {
                return candidate;
            }
        }

        Debug.LogWarning("Không tìm được vị trí trống, spawn tạm!");
        return spawnCenter.position + Vector3.up * spawnHeightY;
    }
}