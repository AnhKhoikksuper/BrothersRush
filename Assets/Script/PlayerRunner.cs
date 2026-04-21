using Fusion;
using UnityEngine;
using System.Collections.Generic;

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

    private Dictionary<PlayerRef, NetworkObject> spawnedPlayers = new();

    private void Awake()
    {
        Instance = this;
        Debug.Log("✅ PlayerRunner READY (Shared Mode)");
    }

    // 🔥 SPAWN TỪ UI (DUY NHẤT)
    public void SpawnSelectedPlayer(int skinIndex, string playerName)
    {
        if (Runner == null || !Runner.IsRunning)
        {
            Debug.LogError("❌ Runner chưa sẵn sàng!");
            return;
        }

        PlayerRef player = Runner.LocalPlayer;

        Debug.Log($"🎮 LOCAL PLAYER: {player.PlayerId}");

        // ❌ Tránh spawn 2 lần
        if (spawnedPlayers.ContainsKey(player))
        {
            Debug.LogWarning("⚠️ Player đã spawn rồi!");
            return;
        }

        // ❌ Check index
        if (skinIndex < 0 || skinIndex >= playerSkinPrefabs.Length)
        {
            Debug.LogError("❌ Skin index không hợp lệ!");
            return;
        }

        GameObject prefab = playerSkinPrefabs[skinIndex];

        if (prefab == null)
        {
            Debug.LogError("❌ Prefab NULL!");
            return;
        }

        Vector3 spawnPos = GetValidSpawnPosition();

        Debug.Log($"📍 Spawn tại: {spawnPos}");

        NetworkObject obj = Runner.Spawn(
            prefab,
            spawnPos,
            Quaternion.identity,
            player, // 🔥 QUAN TRỌNG: authority
            (runner, spawnedObj) =>
            {
                Debug.Log("✅ Spawn callback chạy");

                var data = spawnedObj.GetComponent<PlayerData>();
                if (data != null)
                {
                    data.SetName(playerName);
                }
            }
        );

        if (obj == null)
        {
            Debug.LogError("❌ Spawn FAILED!");
            return;
        }

        // 🔍 DEBUG AUTHORITY
        Debug.Log($"🧠 ObjectID: {obj.Id}");
        Debug.Log($"🔍 InputAuthority: {obj.InputAuthority.PlayerId}");
        Debug.Log($"🔍 LocalPlayer: {Runner.LocalPlayer.PlayerId}");

        if (obj.HasInputAuthority)
            Debug.Log("✅ ĐÚNG: Đây là player của mình");
        else
            Debug.LogError("💥 SAI: Không có quyền điều khiển!");

        spawnedPlayers.Add(player, obj);
    }

    // 🔥 TÌM VỊ TRÍ SPAWN
    private Vector3 GetValidSpawnPosition()
    {
        int maxAttempts = 20;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 rand = Random.insideUnitCircle * spawnRadius;

            Vector3 pos = new Vector3(
                spawnCenter.position.x + rand.x,
                spawnHeightY,
                spawnCenter.position.z + rand.y
            );

            bool blocked = Physics.CheckSphere(
                pos,
                minDistanceBetweenPlayers,
                playerLayer
            );

            if (!blocked)
                return pos;
        }

        Debug.LogWarning("⚠️ Không tìm được vị trí trống → spawn giữa map");
        return spawnCenter.position + Vector3.up * spawnHeightY;
    }

    // 🔥 OPTIONAL: Despawn khi cần
    public void DespawnLocalPlayer()
    {
        PlayerRef player = Runner.LocalPlayer;

        if (spawnedPlayers.TryGetValue(player, out var obj))
        {
            Runner.Despawn(obj);
            spawnedPlayers.Remove(player);

            Debug.Log("🗑 Player đã bị despawn");
        }
    }
}