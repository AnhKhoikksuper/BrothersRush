using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine; // Chuẩn namespace mới

public class ThirdPersonCamera : MonoBehaviour
{
    // Ở bản mới, FreeLook được tích hợp vào CinemachineCamera
    public CinemachineCamera _vcam;
    private bool _isCursorLocked = true;

    void Awake()
    {
        // Tự động tìm component camera mới
        _vcam = GetComponent<CinemachineCamera>();
        if (_vcam == null) _vcam = FindFirstObjectByType<CinemachineCamera>();
    }

    void Start()
    {
        SetCursorLock(false);
    }

    void Update()
    {
        if (Keyboard.current != null &&
           (Keyboard.current.leftCtrlKey.wasPressedThisFrame || Keyboard.current.rightCtrlKey.wasPressedThisFrame))
        {
            _isCursorLocked = !_isCursorLocked;
            SetCursorLock(_isCursorLocked);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        if (_vcam != null && newTarget != null)
        {
            // Cú pháp vẫn tương tự nhưng dùng cho component mới
            _vcam.Follow = newTarget;
            _vcam.LookAt = newTarget;

            Debug.Log($"[Cinemachine 3] Đã bám theo mục tiêu: {newTarget.name}");
        }
    }

    public void SetCursorLock(bool isLocked)
    {
        _isCursorLocked = isLocked;

        // 🔥 Quản lý cursor
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;

        // 🔥 Bật/tắt input Cinemachine
        if (_vcam != null)
        {
            var inputHandler = _vcam.GetComponent<CinemachineInputAxisController>();
            if (inputHandler != null)
                inputHandler.enabled = isLocked;
        }
    }
}