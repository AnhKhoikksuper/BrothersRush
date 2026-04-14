using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    public static PlayerMovement Local;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioSource audioSource;
    [Header("Components")]
    [SerializeField] private CharacterController character;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource footstepAudio;

    [Header("Speed")]
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float sprintSpeed = 10f;

    [Header("Inertia Settings")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float airControl = 0.2f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float maxFallSpeed = -50f;
    [SerializeField] private float jumpForwardBoost = 3f;

    [Tooltip("Độ cao rơi/nhảy tối thiểu để kích hoạt hoạt ảnh")]
    public float jumpHeightThreshold = 0.3f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;

    private Vector2 moveInput;
    private bool isSprinting;
    private bool jumpRequested;
    private Transform mainCameraTransform;

    // === NETWORKED PROPERTIES (phải là auto-property) ===
    [Networked] public Vector3 CurrentHorizontalVelocity { get; set; }
    [Networked] public float VerticalVelocity { get; set; }
    [Networked] public float NetworkedSpeed { get; set; }
    [Networked] public bool IsGroundedNetworked { get; set; }
    [Networked] private float LastGroundedY { get; set; }
    [Networked] public bool IsInMidAirAnim { get; set; }
    [Networked] public Vector3 CheckpointPos { get; set; }
    [Networked] public float CurrentDistToGround { get; set; }
    [Networked] public bool IsLocked { get; set; }
    [Networked] public bool HasDoubleJumped { get; set; }
    [Networked] public bool HasUnlockedDoubleJump { get; set; }
    [Networked] private bool TriggerDoubleJump { get; set; }
    // NetworkTransform (không [Networked])
    private NetworkTransform networkTransform;

    public override void Spawned()
    {
        networkTransform = GetComponent<NetworkTransform>();

        if (HasInputAuthority)
        {
            Local = this;
            TryAssignCamera();
            HasUnlockedDoubleJump = false;
            ThirdPersonCamera camScript = FindFirstObjectByType<ThirdPersonCamera>();
            if (camScript != null)
            {
                Transform target = transform.Find("CameraTarget");
                camScript.SetTarget(target != null ? target : transform);
            }
        }
    }

    public void SetCheckpoint(Vector3 pos)
    {
        if (!HasStateAuthority) return;

        CheckpointPos = pos;
        Debug.Log("Đã lưu checkpoint: " + pos);
    }

    public void Respawn()
    {
        if (!HasStateAuthority) return;

        if (CheckpointPos == Vector3.zero)
        {
            Debug.Log("Chưa có checkpoint!");
            return;
        }

        // === TẮT CÁC COMPONENT TRƯỚC KHI TELEPORT ===
        if (character != null) character.enabled = false;
        if (networkTransform != null) networkTransform.enabled = false;

        // Set vị trí mới (nhấc lên một chút để tránh dính đất)
        Vector3 pos = CheckpointPos;
        pos.y += character.height / 2f;

        transform.position = pos;

        // Reset toàn bộ vận tốc
        CurrentHorizontalVelocity = Vector3.zero;
        VerticalVelocity = 0f;

        // === BẬT LẠI VÀ TELEPORT ĐÚNG CÁCH ===
        if (character != null) character.enabled = true;
        if (networkTransform != null)
        {
            networkTransform.enabled = true;
            networkTransform.Teleport(transform.position, transform.rotation); // Quan trọng: thông báo teleport cho client
        }
        IsLocked = false;

        if (character != null) character.enabled = true;
        if (animator != null) animator.enabled = true;

        // hiện lại model
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = true;
        }

        // mở khóa điều khiển
        IsLocked = false;

        Debug.Log("Respawn tại checkpoint thành công!");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Respawn()
    {
        Respawn();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority || IsLocked) return;

        if (HasInputAuthority)
        {
            Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

            // 🎵 FOOTSTEP
            if (move.magnitude > 0.1f && character.isGrounded)
            {
                if (!footstepAudio.isPlaying)
                {
                    footstepAudio.Play();
                }
            }
            else
            {
                if (footstepAudio.isPlaying)
                {
                    footstepAudio.Stop();
                }
            }
        }

        Vector3 moveDirection = CalculateMoveDirection();

        // Tính tốc độ mục tiêu
        float targetSpeed = (moveInput != Vector2.zero)
            ? (isSprinting ? sprintSpeed : moveSpeed)
            : 0f;

        Vector3 targetVelocity = moveDirection * targetSpeed;

        // Xử lý quán tính
        float lerpFactor = (targetSpeed > 0) ? acceleration : deceleration;
        if (!character.isGrounded) lerpFactor *= airControl;

        CurrentHorizontalVelocity = Vector3.Lerp(
            CurrentHorizontalVelocity,
            targetVelocity,
            Runner.DeltaTime * lerpFactor
        );

        // Gravity + Jump
        HandleGravityAndJumping(moveDirection);

        // Di chuyển
        Vector3 finalVelocity = CurrentHorizontalVelocity;
        finalVelocity.y = VerticalVelocity;

        character.Move(finalVelocity * Runner.DeltaTime);

        // Xoay nhân vật
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Runner.DeltaTime * rotationSpeed);
        }

        // Update animation (dựa vào input, không phải vận tốc thực)
        NetworkedSpeed = (moveInput == Vector2.zero) ? 0f : (isSprinting ? 1f : 0.5f);

        IsGroundedNetworked = character.isGrounded && !IsInMidAirAnim;

        if (transform.position.y < -5f)
        {
            UIManager.Instance.ShowRespawn();
        }
    }


    private void HandleGravityAndJumping(Vector3 targetDirection)
    {
        // Ground distance
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            CurrentDistToGround = hit.distance - 0.1f;
        else
            CurrentDistToGround = 99f;

        if (character.isGrounded)
        {
            if (VerticalVelocity < 0) VerticalVelocity = -2f;
            LastGroundedY = transform.position.y;
            IsInMidAirAnim = false;

            HasDoubleJumped = false;                    // Reset double jump
        }
        else
        {
            VerticalVelocity += gravity * Runner.DeltaTime;
            if (VerticalVelocity < maxFallSpeed) VerticalVelocity = maxFallSpeed;
        }

        // === XỬ LÝ NHẢY ===
        // Bỏ "&& AllowDoubleJump" ở đây vì OnJump đã check rồi. 
        // Nếu jumpRequested = true, nghĩa là nó đã vượt qua bài kiểm tra ở OnJump.
        if (jumpRequested)
        {
            VerticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (targetDirection != Vector3.zero)
            {
                CurrentHorizontalVelocity += targetDirection * jumpForwardBoost;
            }

            // Nếu đang ở trên không thì mới tính là Double Jump
            if (!character.isGrounded)
            {
                HasDoubleJumped = true;
                TriggerDoubleJump = true;
            }

            jumpRequested = false;
            IsInMidAirAnim = true;
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
            animator.SetFloat("Speed", NetworkedSpeed);
            animator.SetBool("IsGrounded", IsGroundedNetworked);
            animator.SetBool("InAir", IsInMidAirAnim);

            // === DOUBLE JUMP TRIGGER ===
            if (TriggerDoubleJump && HasUnlockedDoubleJump)
            {
                animator.SetTrigger("DoubleJump");     // Trigger animation trên TẤT CẢ client
                // Chỉ InputAuthority mới được reset (sẽ replicate sang các client khác)
                if (HasInputAuthority)
                    TriggerDoubleJump = false;
            }
        }
    }

    // === INPUT CALLBACKS ===
    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnSprint(InputValue value) => isSprinting = value.isPressed;

    // === INPUT CALLBACKS ===
    // === CẬP NHẬT QUAN TRỌNG NHẤT TRONG ONJUMP ===
    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            // 1. Nếu đang đứng trên đất -> Nhảy bình thường
            if (character.isGrounded)
            {
                jumpRequested = true;
            }
            // 2. Nếu đang ở trên không -> Chỉ cho nhảy nếu AllowDoubleJump là true VÀ chưa nhảy lần 2
            else if (HasUnlockedDoubleJump && !HasDoubleJumped)
            {
                Debug.Log("Double Jump On Jump");
                jumpRequested = true;
            }
        }
    }

    public void OnRespawn(InputValue value)
    {
        if (!HasInputAuthority) return;
        if (value.isPressed)
        {
            RPC_Respawn();
            UIManager.Instance?.HideRespawn();
        }
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Lock(bool value)
    {
        IsLocked = value;
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_EnableDoubleJump()
    {
        if (!HasStateAuthority) return;
        if (HasUnlockedDoubleJump) return;
        HasUnlockedDoubleJump = true;
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcHitWrongGlass()
    {
        // 💥 spawn hiệu ứng nổ
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        // 👤 Ẩn player
        character.enabled = false;

        if (animator != null) animator.enabled = false;

        // ẩn model (nếu có mesh)
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }

        // khóa điều khiển
        IsLocked = true;

        // UI
        if (HasInputAuthority)
        {
            UIManager.Instance.ShowRespawn();
        }

        Debug.Log("Nổ!");
    }
}