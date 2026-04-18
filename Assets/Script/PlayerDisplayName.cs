using UnityEngine;
using Fusion;
using TMPro;

public class PlayerDisplayName : NetworkBehaviour
{
    public static PlayerDisplayName Local;

    [Header("UI References")]
    [SerializeField] private GameObject worldSpaceCanvas;
    [SerializeField] private TextMeshProUGUI nameText;

    private PlayerData playerData;
    private string _lastName = "";

    public override void Spawned()
    {
        // 🔥 Lấy PlayerData từ cùng object
        playerData = GetComponent<PlayerData>();

        if (HasInputAuthority)
        {
            Local = this;

            // 🔥 Ẩn tên bản thân
            if (worldSpaceCanvas != null)
                worldSpaceCanvas.SetActive(false);
        }
    }

    public override void Render()
    {
        if (playerData == null) return;

        // 🔥 Lấy tên từ PlayerData
        string currentName = playerData.PlayerName.ToString();

        if (_lastName != currentName)
        {
            _lastName = currentName;

            if (nameText != null)
                nameText.text = currentName;
        }

        // 🔥 Billboard
        if (worldSpaceCanvas != null && worldSpaceCanvas.activeSelf)
        {
            if (Camera.main != null)
                worldSpaceCanvas.transform.rotation = Camera.main.transform.rotation;
        }
    }
}