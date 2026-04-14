using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ChatSystem : NetworkBehaviour
{
    public TextMeshProUGUI textMessage;
    public TMP_InputField inputFieldMessage;
    public Button buttonSend;
    public Image panelMessageImage;
    public GameObject chatUIContent; // Kéo object cha chứa toàn bộ UI chat vào đây (hoặc dùng panelMessageImage)

    public float fadeDelay = 2f;
    public float fadeDuration = 1f;

    private float originalAlpha;
    private Coroutine fadeCoroutine;
    private bool isChatOpen = false; // Trạng thái đóng/mở

    public override void Spawned()
    {
        // Giữ nguyên các tìm kiếm UI của bạn
        textMessage = GameObject.Find("TextMessage")?.GetComponent<TextMeshProUGUI>();
        inputFieldMessage = GameObject.Find("InputFieldMessage")?.GetComponent<TMP_InputField>();
        buttonSend = GameObject.Find("ButtonSend")?.GetComponent<Button>();
        panelMessageImage = GameObject.Find("Panel Message")?.GetComponent<Image>();

        if (textMessage == null || inputFieldMessage == null || buttonSend == null || panelMessageImage == null)
        {
            Debug.LogError("Không tìm thấy UI Chat!");
            return;
        }

        originalAlpha = panelMessageImage.color.a;

        buttonSend.onClick.AddListener(SendMessageChat);
        inputFieldMessage.onSubmit.AddListener(delegate { SendMessageChat(); });

        // Mặc định lúc đầu nên ẩn InputField đi
        ToggleChat(false);
    }

    // Fusion sử dụng Render thay cho Update để xử lý Input mượt mà hơn
    public override void Render()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            isChatOpen = !isChatOpen;
            ToggleChat(isChatOpen);
        }
    }

    void ToggleChat(bool isOpen)
    {
        // Hiện/Ẩn InputField và Nút gửi
        inputFieldMessage.gameObject.SetActive(isOpen);
        buttonSend.gameObject.SetActive(isOpen);

        if (isOpen)
        {
            inputFieldMessage.ActivateInputField(); // Tự động focus vào ô nhập

            // Hiện panel lên ngay lập tức khi mở
            StopFade();
            SetPanelAlpha(originalAlpha);
        }
        else
        {
            inputFieldMessage.DeactivateInputField(); // Bỏ focus

            // Khi đóng chủ động, ta có thể bắt đầu cho nó fade out luôn
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOutPanel());
        }
    }

    public void SendMessageChat()
    {
        var message = inputFieldMessage.text;
        if (string.IsNullOrWhiteSpace(message)) return;

        var id = Runner.LocalPlayer.PlayerId;
        var text = $"Player {id}: {message}";

        RpcChat(text);

        inputFieldMessage.text = "";

        // Sau khi gửi, thường người chơi muốn gõ tiếp hoặc đóng chat. 
        // Ở đây mình giữ cho nó mở, nếu muốn gửi xong đóng luôn thì gọi ToggleChat(false)
        inputFieldMessage.ActivateInputField();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcChat(string message)
    {
        if (textMessage != null)
            textMessage.text += message + "\n";

        if (panelMessageImage != null)
        {
            StopFade();
            SetPanelAlpha(originalAlpha);
            fadeCoroutine = StartCoroutine(FadeOutPanel());
        }
    }

    private void StopFade()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
    }

    private void SetPanelAlpha(float alpha)
    {
        Color c = panelMessageImage.color;
        c.a = alpha;
        panelMessageImage.color = c;
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
            SetPanelAlpha(alpha);
            yield return null;
        }
        SetPanelAlpha(0f);
    }
}