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
        ToggleCursor(true);
    }

    void Update()
    {
        if (Keyboard.current != null && 
           (Keyboard.current.leftCtrlKey.wasPressedThisFrame || Keyboard.current.rightCtrlKey.wasPressedThisFrame))
        {
            _isCursorLocked = !_isCursorLocked;
            ToggleCursor(_isCursorLocked);
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

    private void ToggleCursor(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
        
        if (_vcam != null)
        {
            // Cách tắt input ở bản mới
            var inputHandler = _vcam.GetComponent<CinemachineInputAxisController>();
            if (inputHandler != null) inputHandler.enabled = isLocked;
        }
    }
}