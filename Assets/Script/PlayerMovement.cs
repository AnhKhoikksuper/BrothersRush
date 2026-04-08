using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController character;
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float sprintSpeed = 10f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float maxFallSpeed = -50f;
    
    [Tooltip("Độ cao rơi/nhảy tối thiểu để kích hoạt hoạt ảnh (giúp tránh giật khi đi cầu thang)")]
    public float jumpHeightThreshold = 0.3f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;

    private Vector2 moveInput;
    private bool isSprinting;
    private bool jumpRequested;
    private Transform mainCameraTransform;

    // --- NETWORKED PROPERTIES ---
    [Networked] private float _verticalVelocity { get; set; }
    [Networked] public float networkedSpeed { get; set; }
    [Networked] public bool isGroundedNetworked { get; set; }
    [Networked] private float _lastGroundedY { get; set; }
    [Networked] public bool isInMidAirAnim { get; set; }
    
    [Header("Debug Info")]
    [Networked] public float currentDistToGround { get; set; } // Quan sát khoảng cách đất trong Inspector

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            TryAssignCamera();

            ThirdPersonCamera camScript = FindFirstObjectByType<ThirdPersonCamera>();
            if (camScript != null)
            {
                Transform target = transform.Find("CameraTarget");
                camScript.SetTarget(target != null ? target : transform);
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Chỉ xử lý logic di chuyển phía người chơi có quyền điều khiển
        if (!HasInputAuthority) return;

        TryAssignCamera();

        // 1. Tính hướng di chuyển
        Vector3 moveDirection = CalculateMoveDirection();

        // 2. Tính tốc độ và giá trị Animation
        float currentSpeed = 0f;
        float animValue = 0f;

        if (moveInput != Vector2.zero)
        {
            currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
            animValue = isSprinting ? 1f : 0.5f;
        }

        // 3. Xử lý Trọng lực, Nhảy và đo khoảng cách đất
        HandleGravityAndJumping();

        // 4. Thực hiện di chuyển vật lý
        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = _verticalVelocity;

        character.Move(velocity * Runner.DeltaTime);

        // 5. Xoay nhân vật theo hướng di chuyển
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Runner.DeltaTime * rotationSpeed
            );
        }

        // 6. Đồng bộ hóa trạng thái cho toàn mạng
        networkedSpeed = animValue;
        
        // CẬP NHẬT QUAN TRỌNG: 
        // Grounded chỉ bằng true khi vật lý báo chạm đất VÀ logic không xác nhận đang bay (MidAir)
        isGroundedNetworked = character.isGrounded && !isInMidAirAnim;
        Debug.Log("Do cao hien tai:" + currentDistToGround);
    }

    private void TryAssignCamera()
    {
        if (mainCameraTransform == null && Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    private Vector3 CalculateMoveDirection()
    {
        if (moveInput == Vector2.zero || mainCameraTransform == null)
            return Vector3.zero;

        Vector3 forward = mainCameraTransform.forward;
        Vector3 right = mainCameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        return (forward * moveInput.y + right * moveInput.x).normalized;
    }

    private void HandleGravityAndJumping()
    {
        // ĐO KHOẢNG CÁCH ĐẤT THỰC TẾ (Raycast)
        // Bắn tia từ chân nhân vật xuống dưới
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            currentDistToGround = hit.distance - 0.1f;
        }
        else
        {
            currentDistToGround = 99f;
        }

        if (character.isGrounded)
        {
            // Reset vận tốc rơi nhưng giữ lực hút nhẹ để bám dốc
            if (_verticalVelocity < 0)
                _verticalVelocity = -2f;

            _lastGroundedY = transform.position.y;
            
            // Khi thực sự chạm đất bền vững, tắt trạng thái InAir
            isInMidAirAnim = false;

            if (jumpRequested)
            {
                // Công thức nhảy: v = sqrt(h * -2 * g)
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpRequested = false;
                
                // Nhảy chủ động -> Bật hoạt ảnh ngay lập tức
                isInMidAirAnim = true; 
            }
        }
        else
        {
            // Áp dụng trọng lực khi rơi
            _verticalVelocity += gravity * Runner.DeltaTime;

            if (_verticalVelocity < maxFallSpeed)
                _verticalVelocity = maxFallSpeed;

            jumpRequested = false;

            // KIỂM TRA ĐỘ CAO SO VỚI ĐIỂM RỜI ĐẤT
            float fallDistance = _lastGroundedY - transform.position.y;
            float jumpDistance = transform.position.y - _lastGroundedY;

            // Nếu vượt ngưỡng quy định (ví dụ 0.3m) thì mới xác nhận là đang nhảy/rơi thực sự
            if (jumpDistance > jumpHeightThreshold || fallDistance > jumpHeightThreshold)
            {
                isInMidAirAnim = true;
            }
        }
    }

    public override void Render()
    {
        // Cập nhật Animator trên tất cả các máy dựa trên dữ liệu đã đồng bộ
        if (animator != null)
        {
            animator.SetFloat("Speed", networkedSpeed);
            
            // Sử dụng biến đã qua bộ lọc "Cầu thang" để tránh giật hình
            animator.SetBool("IsGrounded", isGroundedNetworked);
            
            // Nếu animator của bạn cần biến InAir riêng:
            animator.SetBool("InAir", isInMidAirAnim);
        }
    }

    // --- INPUT SYSTEM EVENTS ---
    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnSprint(InputValue value) => isSprinting = value.isPressed;

    public void OnJump(InputValue value)
    {
        // Chỉ nhận lệnh nhảy khi phím được nhấn và nhân vật đang đứng trên đất
        if (value.isPressed && character.isGrounded)
        {
            jumpRequested = true;
        }
    }
}