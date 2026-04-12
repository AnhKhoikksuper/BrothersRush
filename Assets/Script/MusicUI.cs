using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class MusicUI : MonoBehaviour
{
    public GameObject panel;
    public Transform content; // nơi chứa button
    public GameObject buttonPrefab;

    private bool isOpen = false;

    void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            isOpen = !isOpen;
            panel.SetActive(isOpen);

            if (isOpen)
            {
                RefreshUI();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    void RefreshUI()
    {
        // xoá button cũ
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // tạo button mới
        foreach (var clip in MusicLibrary.Instance.unlockedMusic)
        {
            GameObject btn = Instantiate(buttonPrefab, content);

            btn.GetComponentInChildren<TextMeshProUGUI>().text = clip.name;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                PlayMusic(clip);
            });
        }
    }

AudioClip currentClip;
AudioSource source;

void Start()
{
    source = GetComponent<AudioSource>();
}

void PlayMusic(AudioClip clip)
{
    // 🔥 nếu đang phát đúng bài → TẮT
    if (source.isPlaying && currentClip == clip)
    {
        source.Stop();
        currentClip = null;

        AudioManager.Instance.PlayBGM();
        return;
    }

    // 🔥 nếu đang phát bài khác → đổi bài
    currentClip = clip;

    AudioManager.Instance.StopBGM();

    source.clip = clip;
    source.Play();
}
}