using Fusion;
using UnityEngine;
using TMPro;

public class TomatoPlayer : NetworkBehaviour
{
    [Networked] public int TomatoCount { get; set; }

    [Header("Shoot")]
    [SerializeField] private GameObject tomatoProjectilePrefab;
    [SerializeField] private Transform shootPoint;

    private TMP_Text tomatoText;
    private CanvasGroup splatterPanel;

    private bool isFading;

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            FindUI();
            UpdateUI();
        }
    }

    // 🔥 TỰ TÌM UI TRONG SCENE
    private void FindUI()
    {
        tomatoText = GameObject.Find("TomatoText")?.GetComponent<TMP_Text>();
        splatterPanel = GameObject.Find("SplatterPanel")?.GetComponent<CanvasGroup>();

        if (splatterPanel != null)
        {
            splatterPanel.alpha = 0;
            splatterPanel.gameObject.SetActive(false);
        }
    }

    // =====================
    // 🍅 NHẶT
    // =====================
    public void AddTomato(int amount)
    {
        if (!HasStateAuthority) return;

        TomatoCount += amount;
        RPC_UpdateUI(TomatoCount);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_UpdateUI(int value)
    {
        TomatoCount = value;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (tomatoText != null)
            tomatoText.text = "x" + TomatoCount;
    }

    // =====================
    // 🔫 BẮN
    // =====================
    public void OnShoot()
    {
        if (!HasInputAuthority) return;
        if (TomatoCount <= 0) return;

        RPC_Shoot();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_Shoot()
    {
        if (TomatoCount <= 0) return;

        TomatoCount--;

        Runner.Spawn(
            tomatoProjectilePrefab,
            shootPoint.position,
            shootPoint.rotation,
            Object.InputAuthority
        );

        RPC_UpdateUI(TomatoCount);
    }

    // =====================
    // 💥 HIT
    // =====================
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_Hit()
    {
        ShowSplatter();
    }

    private void ShowSplatter()
    {
        if (!HasInputAuthority) return;
        if (splatterPanel == null) return;

        splatterPanel.gameObject.SetActive(true);
        splatterPanel.alpha = 1;

        if (!isFading)
            StartCoroutine(FadeOut());
    }

    private System.Collections.IEnumerator FadeOut()
    {
        isFading = true;

        while (splatterPanel.alpha > 0)
        {
            splatterPanel.alpha -= Time.deltaTime * 1.5f;
            yield return null;
        }

        splatterPanel.alpha = 0;
        splatterPanel.gameObject.SetActive(false);

        isFading = false;
    }
}