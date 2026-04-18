using UnityEngine;

public class ChestTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
           UIGamePlayManager.Instance.ShowChestUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
           UIGamePlayManager.Instance.HideChestUI();
        }
    }
}