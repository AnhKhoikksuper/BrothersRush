using UnityEngine;
using Fusion;

public class PuzzleZone : NetworkBehaviour
{
    [SerializeField] private string question;
    [SerializeField] private string answerA;
    [SerializeField] private string answerB;
    [SerializeField] private bool isACorrect;
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerMovement>();

        if (player != null && player.HasInputAuthority)
        {
            player.RPC_Lock(true);

            // 🔥 truyền chính nó vào UI
            UIManager.Instance.ShowPuzzle(this);
        }
    }

    // ✅ trả lời đúng
    public void CorrectAnswer(PlayerMovement player)
    {
        player.RPC_Lock(false);

        // 🔥 mở Double Jump lần đầu
        player.RPC_EnableDoubleJump();

        UIManager.Instance.HidePuzzle();
    }

    // ❌ trả lời sai
    public void WrongAnswer(PlayerMovement player)
    {
        player.RPC_Lock(false);
        player.RPC_Respawn();

        UIManager.Instance.HidePuzzle();
    }
    public void ChooseA(PlayerMovement player)
    {
        if (isACorrect)
            CorrectAnswer(player);
        else
            WrongAnswer(player);
    }

    public void ChooseB(PlayerMovement player)
    {
        if (!isACorrect)
            CorrectAnswer(player);
        else
            WrongAnswer(player);
    }
    public string GetQuestion() => question;
    public string GetAnswerA() => answerA;
    public string GetAnswerB() => answerB;
}