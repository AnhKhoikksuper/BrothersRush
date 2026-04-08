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

    [Header("Inertia Settings")]
    [SerializeField] private float acceleration = 10f;  // Tốc độ tăng tốc
    [SerializeField] private float deceleration = 10f;  // Tốc độ giảm tốc
    [SerializeField] private float airControl = 0.2f;   // Khả năng bẻ lái khi đang nhảy

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float maxFallSpeed = -50f;
    [SerializeField] private float jumpForwardBoost = 3f; // Lực đẩy thêm về phía trước khi nhảy

    [Tooltip("Độ cao rơi/nhảy tối thiểu để kích hoạt hoạt ảnh")]
    public float jumpHeightThreshold = 0.3f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;

    private Vector2 moveInput;
    private bool isSprinting;
    private bool jumpRequested;
    private Transform mainCameraTransform;

    // --- NETWORKED PROPERTIES ---
    [Networked] private Vector3 _currentHorizontalVelocity { get; set; }
    [Networked] private float _verticalVelocity { get; set; }
    [Networked] public float networkedSpeed { get; set; }
    [Networked] public bool isGroundedNetworked { get; set; }
    [Networked] private float _lastGroundedY { get; set; }
    [Networked] public bool isInMidAirAnim { get; set; }

    [Header("Debug Info")]
    [Networked] public float currentDistToGround { get; set; }

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
        if (!HasInputAuthority) return;

        TryAssignCamera();

        // 1. Tính hướng di chuyển từ Input
        Vector3 moveDirection = CalculateMoveDirection();

        // 2. Tính tốc độ mục tiêu
        float targetSpeed = 0f;
        if (moveInput != Vector2.zero)
        {
            targetSpeed = isSprinting ? sprintSpeed : moveSpeed;
        }
        Vector3 targetVelocity = moveDirection * targetSpeed;

        // 3. XỬ LÝ QUÁN TÍNH: Nội suy vận tốc hiện tại sang vận tốc mục tiêu
        float lerpFactor = (targetSpeed > 0) ? acceleration : deceleration;
        if (!character.isGrounded) lerpFactor *= airControl;

        _currentHorizontalVelocity = Vector3.Lerp(_currentHorizontalVelocity, targetVelocity, Runner.DeltaTime * lerpFactor);

        // 4. Xử lý Trọng lực và Nhảy
        HandleGravityAndJumping(moveDirection);

        // 5. THỰC HIỆN DI CHUYỂN
        Vector3 finalVelocity = _currentHorizontalVelocity;
        finalVelocity.y = _verticalVelocity;

        character.Move(finalVelocity * Runner.DeltaTime);

        // 6. Xoay nhân vật
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Runner.DeltaTime * rotationSpeed);
        }

        // 7. CẬP NHẬT ANIMATION THEO INPUT (Không theo quán tính)
        // Khi thả phím (moveInput == zero), networkedSpeed về 0 ngay lập tức
        if (moveInput == Vector2.zero)
        {
            networkedSpeed = 0f;
        }
        else
        {
            networkedSpeed = isSprinting ? 1f : 0.5f;
        }

        isGroundedNetworked = character.isGrounded && !isInMidAirAnim;
    }

    private void HandleGravityAndJumping(Vector3 targetDirection)
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            currentDistToGround = hit.distance - 0.1f;
        else
            currentDistToGround = 99f;

        if (character.isGrounded)
        {
            if (_verticalVelocity < 0) _verticalVelocity = -2f;
            _lastGroundedY = transform.position.y;
            isInMidAirAnim = false;

            if (jumpRequested)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                // CỘNG THÊM LỰC ĐẨY TIẾN KHI NHẢY
                if (targetDirection != Vector3.zero)
                {
                    _currentHorizontalVelocity += targetDirection * jumpForwardBoost;
                }

                jumpRequested = false;
                isInMidAirAnim = true;
            }
        }
        else
        {
            _verticalVelocity += gravity * Runner.DeltaTime;
            if (_verticalVelocity < maxFallSpeed) _verticalVelocity = maxFallSpeed;

            jumpRequested = false;

            float fallDistance = _lastGroundedY - transform.position.y;
            float jumpDistance = transform.position.y - _lastGroundedY;

            if (jumpDistance > jumpHeightThreshold || fallDistance > jumpHeightThreshold)
                isInMidAirAnim = true;
        }
    }

    private void TryAssignCamera()
    {
        if (mainCameraTransform == null && Camera.main != null)
            mainCameraTransform = Camera.main.transform;
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

    public override void Render()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", networkedSpeed);
            animator.SetBool("IsGrounded", isGroundedNetworked);
            animator.SetBool("InAir", isInMidAirAnim);
        }
    }

    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnSprint(InputValue value) => isSprinting = value.isPressed;

    public void OnJump(InputValue value)
    {
        if (value.isPressed && character.isGrounded)
        {
            jumpRequested = true;
        }
    }
}