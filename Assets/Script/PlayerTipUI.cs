using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

public class PlayerTipUI : NetworkBehaviour
{
    private GameObject tipPanel;
    private bool isOpen = false;

    public override void Spawned()
    {
        if (!HasInputAuthority) return;

        // 🔥 LẤY PANEL TỪ UI
        tipPanel = TipUI.Instance.tipPanel;
    }

    void Update()
    {
        if (!HasInputAuthority) return;

        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            ToggleTip();
        }
    }

    void ToggleTip()
    {
        isOpen = !isOpen;

        if (tipPanel != null)
            tipPanel.SetActive(isOpen);

        // mở / khóa chuột
        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }
}