using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Respawn UI")]
    public GameObject respawnPanel;
    
    

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
    public GameObject puzzlePanel;


public TextMeshProUGUI questionText;
public TextMeshProUGUI answerAText;
public TextMeshProUGUI answerBText;

    private PuzzleZone currentPuzzle;

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
}