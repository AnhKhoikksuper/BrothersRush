using Fusion;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Networked] public int readyCount { get; set; }
    [Networked] public int totalPlayers { get; set; }
    [Networked] public bool isGameStarted { get; set; }
    [Networked] public TickTimer countdownTimer { get; set; }
    [Networked] public bool isCountdownStarted { get; set; }
    [Networked, Capacity(10)]
    public NetworkDictionary<PlayerRef, bool> readyPlayers => default;
    private void Awake()
    {
        Instance = this;
    }
    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            totalPlayers = Runner.ActivePlayers.ToList().Count;
        }
    }

    // 🔥 Player nhấn ready
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetReady(PlayerRef player)
    {
        if (readyPlayers.ContainsKey(player)) return;

        readyPlayers.Add(player, true);
        readyCount = readyPlayers.Count;

        Debug.Log("ReadyCount = " + readyCount);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        // ✅ 1. Update total player realtime
        totalPlayers = Runner.ActivePlayers.Count();

        // ✅ 2. Remove player đã rời game
        var toRemove = new List<PlayerRef>();

        foreach (var kvp in readyPlayers)
        {
            if (!Runner.ActivePlayers.Contains(kvp.Key))
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var player in toRemove)
        {
            readyPlayers.Remove(player);
        }

        // ✅ 3. Sync lại readyCount
        readyCount = readyPlayers.Count;

        // ✅ 4. Bắt đầu countdown khi tất cả ready
        if (!isGameStarted && !isCountdownStarted && readyPlayers.Count == totalPlayers && totalPlayers > 0)
        {
            countdownTimer = TickTimer.CreateFromSeconds(Runner, 3f);
            isCountdownStarted = true;

            Debug.Log("Countdown started!");
        }

        // ✅ 5. Khi countdown xong → start game
        if (!isGameStarted && isCountdownStarted && countdownTimer.Expired(Runner))
        {
            isGameStarted = true;

            Debug.Log("Game Started!");

            // 🔥 RESET READY để tránh bug khi chơi lại
            readyPlayers.Clear();
            readyCount = 0;
            isCountdownStarted = false;
        }
    }
}