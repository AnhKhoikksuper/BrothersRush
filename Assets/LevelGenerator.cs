using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Danh sách các loại Block")]
    public List<GameObject> platformPrefabs; // Kéo 10-20 cái vào đây

    [Header("Cấu hình tháp")]
    public int numberOfPlatforms = 50;  // Tổng số bậc muốn xây
    public float heightStep = 2.5f;     // Khoảng cách độ cao giữa các bậc
    public float radius = 5f;           // Độ rộng của vòng xoáy tháp
    public float angleStep = 0.5f;      // Độ xoay mỗi bậc (càng nhỏ tháp càng thẳng)

    [ContextMenu("Generate Random Tower")]
    void Generate()
    {
        if (platformPrefabs == null || platformPrefabs.Count == 0)
        {
            Debug.LogError("Vui lòng kéo các Prefab vào danh sách!");
            return;
        }

        for (int i = 0; i < numberOfPlatforms; i++)
        {
            // 1. Tính toán vị trí xoắn ốc
            float angle = i * angleStep;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * radius,
                i * heightStep,
                Mathf.Sin(angle) * radius
            );

            // 2. Chọn NGẪU NHIÊN 1 prefab trong danh sách bạn đã kéo vào
            int randomIndex = Random.Range(0, platformPrefabs.Count);
            GameObject selectedPrefab = platformPrefabs[randomIndex];

            // 3. Tạo Object
            GameObject newPlatform = Instantiate(selectedPrefab, pos, Quaternion.identity, transform);

            // 4. (Tùy chọn) Làm cho các bậc hướng mặt vào tâm tháp
            newPlatform.transform.LookAt(new Vector3(0, newPlatform.transform.position.y, 0));
        }
    }
}