using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject respawnPanel;

    void Awake()
    {
        Instance = this;
        respawnPanel.SetActive(false);
    }

    public void ShowRespawn()
    {
        respawnPanel.SetActive(true);
    }

    public void HideRespawn()
    {
        respawnPanel.SetActive(false);
    }
    public void OnClickRespawn()
{
    PlayerMovement.Local.RPC_Respawn();
    HideRespawn();
}
}