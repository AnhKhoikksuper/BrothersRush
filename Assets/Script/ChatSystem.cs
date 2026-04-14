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

    public float fadeDelay = 2f;
    public float fadeDuration = 1f;

    private float originalAlpha;
    private Coroutine fadeCoroutine;

    public override void Spawned()
    {
        textMessage = GameObject.Find("TextMessage")?.GetComponent<TextMeshProUGUI>();
        inputFieldMessage = GameObject.Find("InputFieldMessage")?.GetComponent<TMP_InputField>();
        buttonSend = GameObject.Find("ButtonSend")?.GetComponent<Button>();
        panelMessageImage = GameObject.Find("Panel Message")?.GetComponent<Image>();

        // Check null
        if (textMessage == null || inputFieldMessage == null || buttonSend == null || panelMessageImage == null)
        {
            Debug.LogError("Kh�ng t�m th?y UI Chat! Ki?m tra l?i t�n object trong Hierarchy");
            return;
        }

        originalAlpha = panelMessageImage.color.a;

        buttonSend.onClick.AddListener(SendMessageChat);
        inputFieldMessage.onSubmit.AddListener(delegate { SendMessageChat(); });
    }

    public void SendMessageChat()
    {
        var message = inputFieldMessage.text;

        if (string.IsNullOrWhiteSpace(message)) return;

        var id = Runner.LocalPlayer.PlayerId;
        var text = $"Player {id}: {message}";

        RpcChat(text);

        inputFieldMessage.text = "";
        inputFieldMessage.ActivateInputField();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcChat(string message)
    {
        if (textMessage != null)
            textMessage.text += message + "\n";

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
}