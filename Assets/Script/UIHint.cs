using UnityEngine;
using TMPro;

public class UIHint : MonoBehaviour
{
    public static UIHint Instance;

    public GameObject hintPanel;
    public TextMeshProUGUI hintText;

    private void Awake()
    {
        Instance = this;

        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    public void Show(string text)
    {
        if (hintPanel != null)
            hintPanel.SetActive(true);

        if (hintText != null)
            hintText.text = text;
    }

    public void Hide()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }
}