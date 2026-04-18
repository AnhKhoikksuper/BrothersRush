using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGamePlayManager : MonoBehaviour
{
    public static UIGamePlayManager Instance;
    [Header("Ready UI")]
    public GameObject readyPanel;
    public TextMeshProUGUI readyText;
    public TextMeshProUGUI countdownText;
    public GameObject readyButton;
    int lastReadyCount = -1;
    int lastTotalPlayers = -1;
    bool lastGameState = false;
    int lastCountdown = -1;
    [Header("Respawn UI")]
    public GameObject respawnPanel;
    public GameObject puzzlePanel;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI answerAText;
    public TextMeshProUGUI answerBText;

    [Header("Chest UI")]
    public GameObject chestPanel;

    [Header("End Game UI")]
    public GameObject endGamePanel;
    [SerializeField] private AudioClip openChestSound;
    [SerializeField] private AudioSource audioSource;

    private PuzzleZone currentPuzzle;


    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Tắt panel respawn khi khởi tạo
        if (respawnPanel != null)
        {
            respawnPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Respawn Panel chưa được gán trong UIManager!");
        }
        if (chestPanel != null)
        {
            chestPanel.SetActive(false);
        }

        if (endGamePanel != null)
        {
            endGamePanel.SetActive(false);
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.Object || !GameManager.Instance.Object.IsValid)
            return;

        var gm = GameManager.Instance;

        // 🔥 1. Chỉ update text khi thay đổi
        if (gm.readyCount != lastReadyCount || gm.totalPlayers != lastTotalPlayers)
        {
            lastReadyCount = gm.readyCount;
            lastTotalPlayers = gm.totalPlayers;

            readyText.text = $"{gm.readyCount} / {gm.totalPlayers} Ready";
        }

        // 🔥 2. Countdown (chỉ update khi số thay đổi)
        var timer = gm.countdownTimer;

        if (timer.IsRunning)
        {
            float? timeLeft = timer.RemainingTime(gm.Runner);

            if (timeLeft.HasValue)
            {
                int currentTime = Mathf.CeilToInt(timeLeft.Value);

                if (currentTime != lastCountdown)
                {
                    lastCountdown = currentTime;
                    countdownText.text = currentTime.ToString();
                }
            }
        }

        // 🔥 3. Game state (chỉ chạy khi đổi trạng thái)
        if (gm.isGameStarted != lastGameState)
        {
            lastGameState = gm.isGameStarted;

            if (gm.isGameStarted)
            {
                countdownText.text = "BẮT ĐẦU!";
                HideReady();
            }
            else
            {
                ShowReady();
            }
        }
    }
    public void ShowReady()
    {
        // 🔥 Chưa bắt đầu → hiện lại panel (trường hợp reset game)
        if (!readyPanel.activeSelf)
            readyPanel.SetActive(true);
        // Hiện chuột để người chơi có thể click nút
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void HideReady()
    {
        // 🔥 Ẩn Ready Panel
        if (readyPanel.activeSelf)
            readyPanel.SetActive(false);
    }
    /// <summary>
    /// Hiển thị panel Respawn và hiện chuột
    /// </summary>
    public void ShowRespawn()
    {
        if (respawnPanel != null)
        {
            respawnPanel.SetActive(true);
        }

        // Hiện chuột để người chơi có thể click nút
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Ẩn panel Respawn và khóa chuột lại
    /// </summary>
    public void HideRespawn()
    {
        if (respawnPanel != null)
        {
            respawnPanel.SetActive(false);
        }

        // Khóa chuột lại (trở về chế độ chơi bình thường)
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Gọi từ nút Respawn trên UI (Button OnClick)
    /// </summary>
    public void OnClickRespawn()
    {
        if (PlayerMovement.Local != null)
        {
            PlayerMovement.Local.RPC_Respawn();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy PlayerMovement.Local!");
        }

        HideRespawn();
    }

    // Optional: Reset cursor khi object bị destroy (an toàn)
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    public void OnClickReady()
    {
        if (GameManager.Instance != null && GameManager.Instance.Object != null && GameManager.Instance.Object.IsValid)
        {
            GameManager.Instance.RPC_SetReady(GameManager.Instance.Runner.LocalPlayer); // ✅ FIX
        }
        else
        {
            Debug.LogWarning("GameManager chưa sẵn sàng!");
            return;
        }

        var cam = FindFirstObjectByType<ThirdPersonCamera>();
        if (cam != null)
        {
            cam.SetCursorLock(true);
        }

        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }
    public void ShowPuzzle(PuzzleZone puzzle)
    {
        currentPuzzle = puzzle;

        // 🔥 GÁN TEXT TẠI ĐÂY
        questionText.text = puzzle.GetQuestion();
        answerAText.text = puzzle.GetAnswerA();
        answerBText.text = puzzle.GetAnswerB();

        puzzlePanel.SetActive(true);
    }

    public void HidePuzzle()
    {
        puzzlePanel.SetActive(false);
    }
    public void OnChooseA()
    {
        currentPuzzle.ChooseA(PlayerMovement.Local);
    }

    public void OnChooseB()
    {
        currentPuzzle.ChooseB(PlayerMovement.Local);
    }
    public void ShowChestUI()
    {
        if (chestPanel != null)
        {
            chestPanel.SetActive(true);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HideChestUI()
    {
        if (chestPanel != null)
        {
            chestPanel.SetActive(false);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnClickOpenChest()
    {
        if (audioSource != null && openChestSound != null)
        {
            audioSource.PlayOneShot(openChestSound);
        }
        // Ẩn UI rương
        if (chestPanel != null)
            chestPanel.SetActive(false);

        // Hiện màn hình thắng
        if (endGamePanel != null)
            endGamePanel.SetActive(true);

        // 🔥 KHÓA PLAYER
        if (PlayerMovement.Local != null)
        {
            PlayerMovement.Local.IsLocked = true;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}