using System.Linq;
using UnityEngine;

public class LoopingMusic : MonoBehaviour {

    private bool _fastMusic;
    private int bahIndex = 0;
    private bool needToBah = true;
    public bool FastMusic {
        set
        {
            if (currentSong.fastClip == null) return;
            
            if (_fastMusic ^ value) {
                float scaleFactor = value ? 0.8f : 1.25f;
                float newTime = audioSource.time * scaleFactor;

                if (currentSong.loopEndSample != -1) {
                    float songStart = currentSong.loopStartSample * (value ? 0.8f : 1f);
                    float songEnd = currentSong.loopEndSample * (value ? 0.8f : 1f);

                    if (newTime >= songEnd)
                        newTime = songStart + (newTime - songEnd);
                }

                audioSource.clip = value && currentSong.fastClip ? currentSong.fastClip : currentSong.clip;
                audioSource.time = newTime;
                audioSource.Play();
            }

            _fastMusic = value;
        }
        get => currentSong.fastClip && _fastMusic;
    }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] public MusicData currentSong;

    public void Start()
    {
        if (currentSong)
        {
            Play(currentSong);
            needToBah = currentSong.hasBahs;
        }
    }

    public void Play(MusicData song) {
        currentSong = song;
        audioSource.loop = true;
        audioSource.clip = _fastMusic && song.fastClip ? song.fastClip : song.clip;
        audioSource.time = 0;
        audioSource.Play();
    }
    public void Stop() {
        audioSource.Stop();
    }

    public void Update() {
        if (audioSource is not { isPlaying: true })
            return;

        if (needToBah && currentSong.hasBahs && audioSource.time >= currentSong.bahTimestamps[bahIndex])
        {
            GameManager.Instance.BahAllEnemies();
            bahIndex++;
            if (bahIndex == currentSong.bahTimestamps.Length) needToBah = false;
        }

        if (currentSong.loopEndSample != -1) {
            float time = audioSource.time;
            float songStart = currentSong.loopStartSample * (FastMusic ? 0.8f : 1f);
            float songEnd = currentSong.loopEndSample == 0
                ? (FastMusic ? currentSong.fastClip.length : currentSong.clip.length)
                : currentSong.loopEndSample * (FastMusic ? 0.8f : 1f);

            if (time >= songEnd)
            {
                audioSource.time = songStart + (time - songEnd);
                bahIndex = 0;
                needToBah = true;
            }
        }
    }
}