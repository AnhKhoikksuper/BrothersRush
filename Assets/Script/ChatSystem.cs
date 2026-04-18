using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;   // Thêm để dùng List

public class ChatSystem : NetworkBehaviour
{
    public TextMeshProUGUI textMessage;
    public TMP_InputField inputFieldMessage;
    public Button buttonSend;
    public Image panelMessageImage;
    public TMP_Text chatDisplay; // Khung hiển thị text chat
    public TMP_InputField chatInput; // Ô nhập tin nhắn
    public float fadeDelay = 2f;
    public float fadeDuration = 1f;
    private bool allowFocusFromHotkey = false;
    private float originalAlpha;
    private Coroutine fadeCoroutine;

    private InputAction toggleChatAction;

    // Placeholder
    public TextMeshProUGUI placeholderText;

    // === THÊM: Danh sách lưu tin nhắn để giới hạn số lượng ===
    private List<string> chatLines = new List<string>();
    private const int MAX_CHAT_LINES = 10;

    public override void Spawned()
    {
        // Tìm UI
        textMessage = GameObject.Find("TextMessage")?.GetComponent<TextMeshProUGUI>();
        inputFieldMessage = GameObject.Find("InputFieldMessage")?.GetComponent<TMP_InputField>();
        buttonSend = GameObject.Find("ButtonSend")?.GetComponent<Button>();
        panelMessageImage = GameObject.Find("Panel Message")?.GetComponent<Image>();
        placeholderText = GameObject.Find("PlaceholderChat")?.GetComponent<TextMeshProUGUI>();

        if (textMessage == null || inputFieldMessage == null || buttonSend == null || panelMessageImage == null)
        {
            Debug.LogError("Không tìm thấy UI Chat! Kiểm tra lại tên object trong Hierarchy");
            return;
        }

        originalAlpha = panelMessageImage.color.a;

        // === THÊM LISTENER ===
        inputFieldMessage.onSelect.AddListener(OnChatFocus);
        inputFieldMessage.onDeselect.AddListener(OnChatUnfocus);
        buttonSend.onClick.AddListener(SendMessageChat);
        inputFieldMessage.onSubmit.AddListener((_) =>
        {
            if (inputFieldMessage.isFocused)
            SendMessageChat();
        });

        // === PHÍM C NHANH ===
        toggleChatAction = new InputAction("ToggleChat", InputActionType.Button, "<Keyboard>/c");
        toggleChatAction.performed += ctx => FocusChatInput();
        toggleChatAction.Enable();
    }

    /// <summary>
    /// Luôn focus vào InputField khi nhấn C
    /// </summary>
    private void FocusChatInput()
    {
        if (inputFieldMessage == null) return;

        // 🔥 Đánh dấu là do phím C
        allowFocusFromHotkey = true;

        inputFieldMessage.ActivateInputField();

        if (placeholderText != null)
            placeholderText.gameObject.SetActive(false);

        if (PlayerMovement.Local != null)
            PlayerMovement.Local.allowControl = false;
    }

    private void OnChatFocus(string text)
    {
        // ❌ Nếu không phải do nhấn C → hủy focus ngay
        if (!allowFocusFromHotkey)
        {
            inputFieldMessage.DeactivateInputField();
            return;
        }

        // 🔥 Reset lại flag
        allowFocusFromHotkey = false;

        if (PlayerMovement.Local != null)
            PlayerMovement.Local.allowControl = false;

        if (placeholderText != null)
            placeholderText.gameObject.SetActive(false);
    }

    private void OnChatUnfocus(string text)
    {
        if (PlayerMovement.Local != null)
            PlayerMovement.Local.allowControl = true;

        if (placeholderText != null && string.IsNullOrEmpty(inputFieldMessage.text))
            placeholderText.gameObject.SetActive(true);
    }

    public void SendMessageChat()
    {
        var message = inputFieldMessage.text;

        if (string.IsNullOrWhiteSpace(message)) return;

        string senderName = "Player";

        if (PlayerData.Local != null)
        {
            senderName = PlayerData.Local.PlayerName.ToString();
        }

        var formattedMessage = $"<b>{senderName}:</b> {message}";

        RpcChat(formattedMessage);

        inputFieldMessage.text = "";
        inputFieldMessage.DeactivateInputField();

        if (PlayerMovement.Local != null)
            PlayerMovement.Local.allowControl = true;

        if (placeholderText != null)
            placeholderText.gameObject.SetActive(true);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcChat(string message)
    {
        // Thêm tin nhắn mới vào danh sách
        chatLines.Add(message);

        // Giới hạn tối đa 8 tin nhắn
        if (chatLines.Count > MAX_CHAT_LINES)
        {
            chatLines.RemoveAt(0);
        }

        // Cập nhật lại TextMeshPro
        UpdateChatDisplay();

        // Hiển thị panel và bắt đầu fade
        if (panelMessageImage != null)
        {
            Color c = panelMessageImage.color;
            c.a = originalAlpha;
            panelMessageImage.color = c;

            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeOutPanel());
        }
    }

    /// Cập nhật nội dung hiển thị của textMessage từ danh sách chatLines

    private void UpdateChatDisplay()
    {
        if (textMessage == null) return;

        textMessage.text = string.Join("\n", chatLines);
    }

    IEnumerator FadeOutPanel()
    {
        yield return new WaitForSeconds(fadeDelay);

        float time = 0f;
        Color c = panelMessageImage.color;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(originalAlpha, 0f, time / fadeDuration);
            panelMessageImage.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        panelMessageImage.color = new Color(c.r, c.g, c.b, 0f);
    }

    private void OnDestroy()
    {
        if (toggleChatAction != null)
        {
            toggleChatAction.Disable();
            toggleChatAction.Dispose();
        }
    }
}