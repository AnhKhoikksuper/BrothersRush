using UnityEngine;
using System.Collections.Generic;

public class MusicLibrary : MonoBehaviour
{
    public static MusicLibrary Instance;

    public List<AudioClip> unlockedMusic = new List<AudioClip>();

    private void Awake()
    {
        Instance = this;
    }

    public void AddMusic(AudioClip clip)
    {
        if (!unlockedMusic.Contains(clip))
        {
            unlockedMusic.Add(clip);
            Debug.Log("Đã lưu bài: " + clip.name);
        }
    }
}