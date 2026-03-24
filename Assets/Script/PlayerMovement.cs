using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController character;
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpForce = 8f;       // Độ cao khi nhảy
    [SerializeField] private float gravity = -20f;

    private Vector2 moveInput;
    private bool isSprinting; 
    private bool jumpRequested; // Biến tạm để ghi nhận lệnh nhảy
    private Transform mainCameraTransform;
    private float _verticalVelocity;

    [Networked] public float networkedSpeed { get; set; }
    [Networked] public bool isGroundedNetworked { get; set; } // Đồng bộ trạng thái chạm đất

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            if (Camera.main != null) mainCameraTransform = Camera.main.transform;

            ThirdPersonCamera camScript = FindFirstObjectByType<ThirdPersonCamera>();
            if (camScript != null)
            {
                Transform myTarget = transform.Find("CameraTarget");
                camScript.SetTarget(myTarget != null ? myTarget : this.transform);
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasInputAuthority)
        {
            Vector3 move = Vector3.zero;

            // 1. Tính hướng di chuyển
            if (moveInput != Vector2.zero && mainCameraTransform != null)
            {
                Vector3 forward = mainCameraTransform.forward;
                Vector3 right = mainCameraTransform.right;
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();
                move = (forward * moveInput.y + right * moveInput.x).normalized;
            }

            // 2. Tốc độ tức thời
            float currentSpeed = 0;
            float animValue = 0;

            if (moveInput != Vector2.zero)
            {
                currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
                animValue = isSprinting ? 1.0f : 0.5f;
            }

            // 3. XỬ LÝ NHẢY VÀ TRỌNG LỰC
            if (character.isGrounded)
            {
                // Khi chạm đất, reset vận tốc Y nhưng giữ một chút lực hút xuống để check grounded chuẩn
                if (_verticalVelocity < 0) _verticalVelocity = -2f;

                // Thực hiện nhảy nếu có yêu cầu
                if (jumpRequested)
                {
                    _verticalVelocity = jumpForce;
                    jumpRequested = false; // Reset ngay sau khi nhảy
                    // Nếu bạn có Animation nhảy, hãy trigger ở đây:
                    // animator.SetTrigger("Jump"); 
                }
            }
            else
            {
                // Áp dụng trọng lực khi đang rơi
                _verticalVelocity += gravity * Runner.DeltaTime;
                jumpRequested = false; // Không cho phép nhảy khi đang ở trên không
            }

            // 4. Thực hiện di chuyển
            Vector3 finalMove = (move * currentSpeed);
            finalMove.y = _verticalVelocity;
            character.Move(finalMove * Runner.DeltaTime);

            // 5. Xoay mặt
            if (move != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Runner.DeltaTime * rotationSpeed);
            }

            // 6. Đồng bộ trạng thái cho máy khác
            networkedSpeed = animValue;
            isGroundedNetworked = character.isGrounded;
        }
    }

    public override void Render()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", networkedSpeed);
            // Đồng bộ trạng thái trên không để chạy animation rơi/tiếp đất
            animator.SetBool("IsGrounded", isGroundedNetworked);
        }
    }

    // --- INPUT SYSTEM EVENTS ---

    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnSprint(InputValue value) => isSprinting = value.isPressed;
    
    // Hàm mới cho phím Space
    public void OnJump(InputValue value)
    {
        if (HasInputAuthority && value.isPressed)
        {
            jumpRequested = true;
        }
    }
}