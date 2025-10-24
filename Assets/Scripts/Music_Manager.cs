using UnityEngine;

public class Music_Manager : MonoBehaviour
{
    public AudioSource bgmSource;
    public AudioClip[] tracks;

    void Start()
    {
        PlayNextTrack();
    }

    void Update()
    {
        if (!bgmSource.isPlaying)
        {
            PlayNextTrack();
        }
    }
    void PlayNextTrack()
    {
        if (tracks.Length == 0) return;
        int randIndex = Random.Range(0, tracks.Length);
        bgmSource.clip = tracks[randIndex];
        bgmSource.Play();
    }
}
