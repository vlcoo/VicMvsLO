using System.Collections.Generic;
using UnityEngine;

namespace FluidMidi
{
    public class Playlist : MonoBehaviour
    {
        [SerializeField] private List<SongPlayer> songs = new();

        private int index;

        public bool IsReady => songs.Count == 0 || songs[0].IsReady;

        private void Start()
        {
            Play();
        }

        private void Update()
        {
            if (songs[index].IsDone)
            {
                ++index;
                Play();
            }
        }

        private void Play()
        {
            while (index < songs.Count)
            {
                if (songs[index] != null)
                {
                    songs[index].Play();
                    return;
                }

                ++index;
            }

            enabled = false;
        }
    }
}