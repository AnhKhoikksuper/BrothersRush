using Fusion;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TomatoPlayer : NetworkBehaviour
{
    [Networked] public int TomatoCount { get; set; }

    [Header("Shoot")]
    [SerializeField] private GameObject tomatoProjectilePrefab;
    [SerializeField] private Transform shootPoint;

    public TMP_Text tomatoText;
    public Image splatterImage;

    private bool isFading;

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            FindUI();
            UpdateUI();
        }
    }

    // 🔥 TỰ TÌM UI
    private void FindUI()
    {
        tomatoText = GameObject.Find("TomatoText")?.GetComponent<TMP_Text>();
        splatterImage = GameObject.Find("SplatterImage")?.GetComponent<Image>();

        if (splatterImage != null)
        {
            Color c = splatterImage.color;
            c.a = 0;
            splatterImage.color = c;
            splatterImage.gameObject.SetActive(false);
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
        if (splatterImage == null) return;

        splatterImage.gameObject.SetActive(true);

        Color c = splatterImage.color;
        c.a = 1;
        splatterImage.color = c;

        if (!isFading)
            StartCoroutine(FadeOut());
    }

    private System.Collections.IEnumerator FadeOut()
    {
        isFading = true;

        Color c = splatterImage.color;

        while (c.a > 0)
        {
            c.a -= Time.deltaTime * 1.5f;
            splatterImage.color = c;
            yield return null;
        }

        c.a = 0;
        splatterImage.color = c;

        splatterImage.gameObject.SetActive(false);

        isFading = false;
    }
}