//using Fusion;
//using UnityEngine;
//using System.Collections.Generic;
//using Fusion.Sockets;

//public class NetworkGameManager : MonoBehaviour, INetworkRunnerCallbacks
//{
//    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
//    {
//        if (runner.IsServer)
//        {
//            int skinIndex = PlayerPrefs.GetInt("SelectedSkin", 0);
//            string playerName = PlayerPrefs.GetString("PlayerName", "Player");

//            PlayerRunner.Instance.SpawnSelectedPlayer(player, skinIndex, playerName);
//        }
//    }

//    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
//    public void OnInput(NetworkRunner runner, NetworkInput input) { }
//    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
//    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
//    public void OnConnectedToServer(NetworkRunner runner) { }
//    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
//    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
//    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
//    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
//    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
//    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
//    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
//    public void OnSceneLoadDone(NetworkRunner runner) { }
//    public void OnSceneLoadStart(NetworkRunner runner) { }

//    // ? 4 cái b?n ?ang thi?u
//    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
//    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
//    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
//    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
//}