using Fusion;
using UnityEngine;

public class PlayerRunner : SimulationBehaviour
{
    public static PlayerRunner Instance;

    [Header("Skin Prefabs")]
    [SerializeField] private GameObject[] playerSkinPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnHeightY = 10f;
    [SerializeField] private float minRange = -2f;
    [SerializeField] private float maxRange = 10f;

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

        Vector3 spawnPosition = new Vector3(
            Random.Range(minRange, maxRange),
            spawnHeightY,
            Random.Range(minRange, maxRange)
        );

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
}