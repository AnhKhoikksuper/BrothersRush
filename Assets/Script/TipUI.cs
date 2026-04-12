using UnityEngine;

public class TipUI : MonoBehaviour
{
    public static TipUI Instance;

    public GameObject tipPanel;

    private void Awake()
    {
        Instance = this;

        if (tipPanel != null)
            tipPanel.SetActive(false);
    }
}