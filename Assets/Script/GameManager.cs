using Fusion;
using UnityEngine;
using System.Linq;
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Networked] public int readyCount { get; set; }
    [Networked] public int totalPlayers { get; set; }
    [Networked] public bool isGameStarted { get; set; }
    [Networked] public TickTimer countdownTimer { get; set; }
    [Networked] public bool isCountdownStarted { get; set; }
    private void Awake()
    {
        Instance = this;
    }
    public void IncreaseReady()
    {
        if (!Object.HasStateAuthority) return;

        readyCount++;
    }
    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            totalPlayers = Runner.ActivePlayers.ToList().Count;
        }
    }

    // 🔥 Player nhấn ready
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetReady()
    {
        readyCount++;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        // 👉 Chỉ start countdown 1 lần
        if (!isGameStarted && !isCountdownStarted && readyCount >= totalPlayers)
        {
            countdownTimer = TickTimer.CreateFromSeconds(Runner, 3f);
            isCountdownStarted = true;
        }

        // 👉 Khi countdown xong
        if (!isGameStarted && isCountdownStarted && countdownTimer.Expired(Runner))
        {
            isGameStarted = true;
        }
    }
}