using UnityEngine;

public class BarrierController : MonoBehaviour
{
    [Header("Settings")]
    public bool enableBarrier = true;
    public bool isGameStarted = false;

    private GameObject[] barriers;

    void Start()
    {
        int childCount = transform.childCount;
        barriers = new GameObject[childCount];

        for (int i = 0; i < childCount; i++)
        {
            barriers[i] = transform.GetChild(i).gameObject;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // ❗ CHECK FUSION READY
        if (GameManager.Instance.Object == null || !GameManager.Instance.Object.IsValid)
            return;

        UpdateBarrierState();
    }

    void UpdateBarrierState()
    {
        if (!enableBarrier)
        {
            SetBarrierActive(false);
            return;
        }

        if (GameManager.Instance.isGameStarted)
        {
            SetBarrierActive(false);
        }
        else
        {
            SetBarrierActive(true);
        }
    }

    void SetBarrierActive(bool state)
    {
        foreach (var barrier in barriers)
        {
            if (barrier != null)
                barrier.SetActive(state);
        }
    }
}