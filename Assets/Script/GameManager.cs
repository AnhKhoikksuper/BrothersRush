using Fusion;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    [Networked] public int readyCount { get; set; }
    [Networked] public int totalPlayers { get; set; }
    [Networked] public bool isGameStarted { get; set; }
    [Networked] public bool isCountdownStarted { get; set; }
    [Networked] public TickTimer countdownTimer { get; set; }

    [Header("Ready Players")]
    [Networked, Capacity(20)]
    public NetworkDictionary<PlayerRef, bool> readyPlayers => default;

    private void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            totalPlayers = Runner.ActivePlayers.Count();
        }
    }

    // =========================
    // 🔥 PLAYER READY (SAFE RPC)
    // =========================
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetReady(RpcInfo info = default)
    {
        PlayerRef player = info.Source;

        if (readyPlayers.ContainsKey(player))
        {
            Debug.Log($"⚠️ Player {player.PlayerId} đã ready rồi");
            return;
        }

        readyPlayers.Add(player, true);
        readyCount = readyPlayers.Count;

        Debug.Log($"✅ Player {player.PlayerId} READY | Total Ready: {readyCount}");
    }

    // =========================
    // 🔄 MAIN GAME LOOP
    // =========================
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        // 🔥 1. Update total players realtime
        totalPlayers = Runner.ActivePlayers.Count();

        // 🔥 2. Remove player đã rời
        RemoveDisconnectedPlayers();

        // 🔥 3. Sync lại ready count
        readyCount = readyPlayers.Count;

        // 🔥 4. Start countdown nếu đủ người
        if (!isGameStarted &&
            !isCountdownStarted &&
            totalPlayers > 0 &&
            readyPlayers.Count >= totalPlayers)
        {
            countdownTimer = TickTimer.CreateFromSeconds(Runner, 3f);
            isCountdownStarted = true;

            Debug.Log("⏳ Countdown START (3s)");
        }

        // 🔥 5. Countdown xong → start game
        if (!isGameStarted &&
            isCountdownStarted &&
            countdownTimer.Expired(Runner))
        {
            StartGame();
        }
    }

    // =========================
    // 🧹 REMOVE PLAYER RỜI GAME
    // =========================
    private void RemoveDisconnectedPlayers()
    {
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
            Debug.Log($"❌ Removed disconnected player {player.PlayerId}");
        }
    }

    // =========================
    // 🎮 START GAME
    // =========================
    private void StartGame()
    {
        isGameStarted = true;
        isCountdownStarted = false;

        Debug.Log("🚀 GAME STARTED!");

        // 🔥 Reset ready system
        ResetReadyPlayers();
    }

    // =========================
    // 🔄 RESET READY
    // =========================
    private void ResetReadyPlayers()
    {
        var toRemove = new List<PlayerRef>();

        foreach (var kvp in readyPlayers)
        {
            toRemove.Add(kvp.Key);
        }

        foreach (var k in toRemove)
        {
            readyPlayers.Remove(k);
        }

        readyCount = 0;
    }

    // =========================
    // 🔁 RESET GAME (REPLAY)
    // =========================
    public void ResetGame()
    {
        if (!Object.HasStateAuthority) return;

        isGameStarted = false;
        isCountdownStarted = false;
        readyCount = 0;

        ResetReadyPlayers();

        Debug.Log("🔄 Game RESET");
    }

    // =========================
    // 📊 DEBUG UI SUPPORT
    // =========================
    public float GetCountdownTime()
    {
        if (!isCountdownStarted) return -1f;

        return countdownTimer.RemainingTime(Runner) ?? 0f;
    }
}