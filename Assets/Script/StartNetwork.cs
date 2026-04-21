//using Fusion;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class StartNetwork : MonoBehaviour
//{
//    private NetworkRunner runner;

//    private async void Start()
//    {
//        runner = gameObject.AddComponent<NetworkRunner>();
//        var sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

//        runner.AddCallbacks(FindObjectOfType<NetworkGameManager>());

//        await runner.StartGame(new StartGameArgs()
//        {
//            GameMode = GameMode.Host,
//            SessionName = "TestRoom",
//            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
//            SceneManager = sceneManager
//        });

//        Debug.Log("NETWORK STARTED");
//    }
//}