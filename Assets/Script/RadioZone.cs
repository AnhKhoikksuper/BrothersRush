using UnityEngine;
using UnityEngine.InputSystem;

public class RadioZone : MonoBehaviour
{
    private Radio currentRadio;
    private bool isInside = false;

    private void Update()
    {
        if (!isInside || currentRadio == null) return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            Debug.Log("Unlock nhạc");

            // 🔥 chỉ lưu
            MusicLibrary.Instance.AddMusic(currentRadio.GetClip());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerMovement>();

        if (player != null && player.HasInputAuthority)
        {
            currentRadio = GetComponentInParent<Radio>();
            isInside = true;

            UIHint.Instance.Show("Press Q to unlock music");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var player = other.GetComponent<PlayerMovement>();

        if (player != null && player.HasInputAuthority)
        {
            isInside = false;
            currentRadio = null;

            UIHint.Instance.Hide();
        }
    }
}