using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkinSelectionUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject selectionPanel;
    public TMP_InputField nameInputField;
    public Button confirmButton;

    [Header("Skin Buttons")]
    [SerializeField] private Button[] skinButtons;

    private SkinButtonUI[] skinButtonUIs;

    private int selectedSkinIndex = -1;

    private void Start()
    {
        skinButtonUIs = new SkinButtonUI[skinButtons.Length];

        for (int i = 0; i < skinButtons.Length; i++)
        {
            int index = i;

            skinButtons[i].onClick.AddListener(() => SelectSkin(index));
            skinButtonUIs[i] = skinButtons[i].GetComponent<SkinButtonUI>();
        }

        confirmButton.onClick.AddListener(OnConfirmClicked);

        // 🔥 CHỌN SẴN ELEMENT 0
        SelectSkin(0);
    }

    public void SelectSkin(int index)
    {
        // ❗ Nếu bấm lại nút đang chọn → bỏ qua
        if (selectedSkinIndex == index) return;

        selectedSkinIndex = index;

        UpdateSelectionUI();

        Debug.Log("Chọn skin: " + index);
    }

    private void UpdateSelectionUI()
    {
        for (int i = 0; i < skinButtonUIs.Length; i++)
        {
            if (skinButtonUIs[i] != null)
            {
                skinButtonUIs[i].SetSelected(i == selectedSkinIndex);
            }
        }
    }

    private void OnConfirmClicked()
    {
        if (selectedSkinIndex == -1)
        {
            Debug.LogWarning("Bạn chưa chọn skin!");
            return;
        }

        string playerName = nameInputField.text;

        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Player_" + Random.Range(1000, 9999);
        }

        PlayerRunner.Instance.SpawnSelectedPlayer(selectedSkinIndex, playerName);

        selectionPanel.SetActive(false);
    }
}