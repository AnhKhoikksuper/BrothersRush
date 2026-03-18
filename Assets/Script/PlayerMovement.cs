using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController character;
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -20f; // Trọng lực mạnh cho cảm giác chắc chắn

    private Vector2 moveInput;
    private Transform mainCameraTransform;
    private float _verticalVelocity;

    // Biến này được đồng bộ qua mạng để mọi người đều thấy animation của nhau
    [Networked] public float networkedSpeed { get; set; }

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            // Đảm bảo lấy được Camera ngay khi Spawn
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
        // CHỈ máy sở hữu mới tính toán di chuyển vật lý
        if (HasInputAuthority)
        {
            Vector3 move = Vector3.zero;

            // 1. Tính hướng di chuyển theo Camera (Roblox Style)
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

            // 2. Xử lý Trọng lực
            if (character.isGrounded)
            {
                if (_verticalVelocity < 0) _verticalVelocity = -2f;
            }
            else
            {
                _verticalVelocity += gravity * Runner.DeltaTime;
            }

            // 3. Thực hiện di chuyển
            Vector3 finalMove = (move * moveSpeed);
            finalMove.y = _verticalVelocity;
            character.Move(finalMove * Runner.DeltaTime);

            // 4. Xoay mặt nhân vật
            if (move != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Runner.DeltaTime * rotationSpeed);
            }

            // Cập nhật giá trị mạng để truyền đi cho các máy khác
            networkedSpeed = moveInput.magnitude;
        }
    }

    public override void Render()
    {
        // Hàm này chạy trên TẤT CẢ các máy (máy mình và máy bạn bè)
        // Dùng giá trị networkedSpeed đã đồng bộ để chạy Animation
        if (animator != null)
        {
            animator.SetFloat("Speed", networkedSpeed);
        }
    }

    public void OnMove(InputValue value)
    {
        if (HasInputAuthority)
        {
            moveInput = value.Get<Vector2>();
        }
    }
    public void OnFootstep(AnimationEvent animationEvent) { }
}