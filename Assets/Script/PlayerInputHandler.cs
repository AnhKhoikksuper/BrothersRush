using Fusion;
using UnityEngine;
using System;
using System.Collections.Generic;
using Fusion.Sockets;
public class PlayerInputHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    private const int BUTTON_JUMP = 0;
    private const int BUTTON_SPRINT = 1;
    private const int BUTTON_FIRE = 2;
    private const int BUTTON_RESPAWN = 3;
    private NetworkButtons previousButtons;

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // 🔥 BLOCK TOÀN BỘ INPUT KHI CHAT
        if (PlayerMovement.Local != null && !PlayerMovement.Local.allowControl)
        {
            input.Set(new PlayerInputData()); // gửi input rỗng
            return;
        }
        PlayerInputData data = new PlayerInputData();

        data.move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        var buttons = new NetworkButtons();

        buttons.Set(BUTTON_JUMP, Input.GetKey(KeyCode.Space));
        buttons.Set(BUTTON_SPRINT, Input.GetKey(KeyCode.LeftShift));
        buttons.Set(BUTTON_FIRE, Input.GetMouseButton(0));
        buttons.Set(BUTTON_RESPAWN, Input.GetKey(KeyCode.R));

        data.buttons = buttons;

        input.Set(data);

        previousButtons = buttons;
    }

    // =========================
    // FULL CALLBACK (Fusion NEW)
    // =========================

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

    public void OnConnectedToServer(NetworkRunner runner) { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    // 🔥 NEW RELIABLE DATA API
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}