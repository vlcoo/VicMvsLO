using UnityEngine;

public class LoopingMusic : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] public MusicData currentSong;

    private bool _fastMusic;
    private int bahIndex;
    private bool needToBah = true;

    public bool FastMusic
    {
        set
        {
            if (currentSong.fastClip == null) return;

            if (_fastMusic ^ value)
            {
                var scaleFactor = value ? 0.8f : 1.25f;
                var newTime = audioSource.time * scaleFactor;

                if (currentSong.loopEndSample != -1)
                {
                    var songStart = currentSong.loopStartSample * (value ? 0.8f : 1f);
                    var songEnd = currentSong.loopEndSample * (value ? 0.8f : 1f);

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

    public void Start()
    {
        if (currentSong)
        {
            Play(currentSong);
            needToBah = currentSong.hasBahs;
        }
    }

    public void Update()
    {
        if (audioSource is not { isPlaying: true })
            return;

        if (needToBah && currentSong.hasBahs && audioSource.time >= currentSong.bahTimestamps[bahIndex])
        {
            GameManager.Instance.BahAllEnemies();
            bahIndex++;
            if (bahIndex == currentSong.bahTimestamps.Length) needToBah = false;
        }

        if (currentSong.loopEndSample != -1)
        {
            var time = audioSource.time;
            var songStart = currentSong.loopStartSample * (FastMusic ? 0.8f : 1f);
            var songEnd = currentSong.loopEndSample == 0
                ? FastMusic ? currentSong.fastClip.length : currentSong.clip.length
                : currentSong.loopEndSample * (FastMusic ? 0.8f : 1f);

            if (time >= songEnd)
            {
                audioSource.time = songStart + (time - songEnd);
                bahIndex = 0;
                needToBah = true;
            }
        }
    }

    public void Play(MusicData song)
    {
        currentSong = song;
        audioSource.loop = true;
        audioSource.clip = _fastMusic && song.fastClip ? song.fastClip : song.clip;
        audioSource.time = 0;
        audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
    }
}