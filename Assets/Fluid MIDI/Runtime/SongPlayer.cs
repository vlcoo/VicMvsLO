using System;
using System.Collections.Generic;
using System.IO;
using FluidSynth;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace FluidMidi
{
    public class SongPlayer : MonoBehaviour
    {
        private static readonly ISet<SongPlayer> players = new HashSet<SongPlayer>();

        [SerializeField] public Synthesizer synthesizer;
        [SerializeField] public StreamingAsset song = new();

        [SerializeField] [Tooltip("Start playing after the song is loaded for the first time.")]
        public bool playOnStart = true;

        [SerializeField]
        [Tooltip("Automatically unload the song when it is stopped.")]
        [ToggleIntFoldout(name = "Delay", tooltip = "Seconds to wait for notes to finish playing.")]
        private ToggleInt unloadOnStop = new(false, 3);

        [SerializeField]
        [Tooltip("Play again when the song reaches the end.")]
        [ToggleIntFoldout(name = "Start Ticks", tooltip = "Position to start playing again.")]
        private ToggleInt loop = new(true, 0);

        [SerializeField] [Tooltip("Make the song end early.")]
        private ToggleInt endTicks = new(false, 0);

        [SerializeField] [Range(Api.Synth.GAIN_MIN, Api.Synth.GAIN_MAX)]
        private float gain = 0.2f;

        [SerializeField] [Range((float)Api.Player.TEMPO_MIN, (float)Api.Player.TEMPO_MAX)]
        private float tempo = 1;

        [SerializeField] [BitField] public int channels = -1;
        private IntPtr driver = IntPtr.Zero;
        private IntPtr playerPtr = IntPtr.Zero;
        private JobHandle prepareJob;

        private IntPtr synthPtr = IntPtr.Zero;
        private float unloadDelay = -1;

        public int Ticks => playerPtr != IntPtr.Zero ? Api.Player.GetCurrentTick(playerPtr) : 0;

        public float Gain
        {
            get => gain;

            set
            {
                if (value < Api.Synth.GAIN_MIN || value > Api.Synth.GAIN_MAX)
                {
                    Logger.LogError("Unable to set gain to " + value);
                    return;
                }

                if (synthPtr != IntPtr.Zero) Api.Synth.SetGain(synthPtr, value);

                gain = value;
            }
        }

        public float Tempo
        {
            get => tempo;

            set
            {
                if (value < Api.Player.TEMPO_MIN || value > Api.Player.TEMPO_MAX ||
                    (playerPtr != IntPtr.Zero &&
                     Api.Player.SetTempo(playerPtr, Api.Player.TempoType.Internal, value) != Api.Result.OK))
                {
                    Logger.LogError("Unable to set tempo to " + value);
                    return;
                }

                tempo = value;
            }
        }

        public int StartTicks
        {
            get => loop.Value;
            set => loop.Value = value;
        }

        public int EndTicks
        {
            get => endTicks.Value;
            set
            {
                endTicks.Enabled = value != 0;
                endTicks.Value = value;
            }
        }

        /// <summary>
        ///     True if the song is loaded and ready to play.
        /// </summary>
        public bool IsReady => driver != IntPtr.Zero;

        /// <summary>
        ///     True if the song is playing.
        /// </summary>
        /// <remarks>
        ///     A paused song is still considered playing.
        /// </remarks>
        public bool IsPlaying =>
            IsPaused || (playerPtr != IntPtr.Zero &&
                         Api.Player.GetStatus(playerPtr) == Api.Player.Status.Playing);

        /// <summary>
        ///     True if the song is paused.
        /// </summary>
        /// <remarks>
        ///     A paused song is still considered playing.
        /// </remarks>
        public bool IsPaused { get; private set; }

        /// <summary>
        ///     True if the song has finished playing or was stopped.
        /// </summary>
        public bool IsDone => unloadDelay >= 0 && !IsPlaying;

        public int Channels
        {
            get => channels;
            set
            {
                channels = value;
                SetActiveChannels();
            }
        }

        private void Reset()
        {
            var synthesizers = FindObjectsOfType<Synthesizer>();
            if (synthesizers.Length == 0)
                synthesizer = gameObject.AddComponent<Synthesizer>();
            else if (synthesizers.Length == 1) synthesizer = synthesizers[0];
        }

        private void Start()
        {
            if (playOnStart)
            {
                Init();
                Play();
            }
        }

        private void Update()
        {
            if (unloadOnStop.Enabled && IsDone)
            {
                unloadDelay -= Time.unscaledDeltaTime;
                if (unloadDelay <= 0)
                {
                    unloadDelay = 0;
                    enabled = false;
                }
            }
            else if (driver == IntPtr.Zero && prepareJob.IsCompleted)
            {
                CreateDriver();
            }
        }

        private void OnDisable()
        {
            if (synthesizer == null) return;

            IsPaused = false;
            if (driver != IntPtr.Zero)
            {
                Api.Driver.Destroy(driver);
                driver = IntPtr.Zero;
            }
            else if (!prepareJob.IsCompleted)
            {
                Logger.LogWarning("Disabling SongPlayer before prepared");
            }

            prepareJob.Complete();
            Api.Player.Destroy(playerPtr);
            playerPtr = IntPtr.Zero;
            Api.Synth.RemoveSoundFont(synthPtr, Api.Synth.GetSoundFont(synthPtr, 0));
            Api.Synth.Destroy(synthPtr);
            synthPtr = IntPtr.Zero;
            synthesizer.RemoveReference();
            Settings.RemoveReference();
            Logger.RemoveReference();
        }

        private void OnDestroy()
        {
            players.Remove(this);
        }

        private void OnValidate()
        {
            if (loop.Value < 0) loop.Value = 0;

            if (endTicks.Value < 0) endTicks.Value = 0;

            var songPath = song.GetFullPath();
            if (songPath.Length > 0 && Api.Misc.IsMidiFile(songPath) == 0)
            {
                Logger.LogError("Not a MIDI file: " + songPath);
                song.SetFullPath(string.Empty);
            }

            if (synthPtr != IntPtr.Zero) Api.Synth.SetGain(synthPtr, gain);

            if (playerPtr != IntPtr.Zero)
            {
                Api.Player.SetTempo(playerPtr, Api.Player.TempoType.Internal, tempo);
                Api.Player.SetActiveChannels(playerPtr, channels);
            }
        }

        public static void PauseAll()
        {
            foreach (var player in players) player.Pause();
        }

        public static void ResumeAll()
        {
            foreach (var player in players) player.Resume();
        }

        public static void StopAll()
        {
            foreach (var player in players) player.Stop();
        }

        /// <summary>
        ///     Start playing the song from the beginning.
        /// </summary>
        /// <remarks>
        ///     Will load the song if necessary.
        /// </remarks>
        public void Play()
        {
            enabled = true;
            if (playerPtr == IntPtr.Zero)
            {
                Logger.LogError("Play called before SongPlayer initialized! " +
                                "Call from Start() or later or adjust script execution priority.");
                return;
            }

            if (!IsPlaying)
            {
                Api.Player.Seek(playerPtr, 0);
                Api.Player.Play(playerPtr);
                unloadDelay = unloadOnStop.Value;
                IsPaused = false;
            }
        }

        public void Stop()
        {
            if (IsPlaying)
            {
                Api.Player.Stop(playerPtr);
                IsPaused = false;
            }
        }

        public void Pause()
        {
            if (IsPlaying)
            {
                Api.Player.Stop(playerPtr);
                IsPaused = true;
            }
        }

        /// <summary>
        ///     Starts playing again.
        /// </summary>
        /// <remarks>
        ///     This has no effect unless the song is paused.
        /// </remarks>
        public void Resume()
        {
            if (IsPaused)
            {
                Api.Player.Play(playerPtr);
                IsPaused = false;
            }
        }

        /// <summary>
        ///     Start playing from a new position.
        /// </summary>
        /// <remarks>
        ///     If the song is not playing, it will begin playing from the new position
        ///     when playback starts. This function will fail if the SongPlayer is not enabled
        ///     or during playback before a previous call to seek has taken effect.
        /// </remarks>
        /// <param name="ticks">The new position in ticks.</param>
        /// <returns>
        ///     True if the new position was set successfully.
        /// </returns>
        public bool Seek(int ticks)
        {
            return playerPtr != IntPtr.Zero && Api.Player.Seek(playerPtr, ticks) == Api.Result.OK;
        }

        public void SetTickEvent(Action<int> action)
        {
            Api.Player.SetTickCallback(playerPtr, (data, tick) =>
            {
                if (action == null) return -1;
                action.Invoke(tick);
                return 0;
            }, IntPtr.Zero);
        }

        public bool IsChannelEnabled(int channel)
        {
            ValidateChannel(channel);
            return (channels & (1 << (channel - 1))) != 0;
        }


        public void EnableChannels(params int[] channels)
        {
            foreach (var channel in channels)
                if (ValidateChannel(channel))
                    this.channels |= 1 << (channel - 1);

            SetActiveChannels();
        }

        public void DisableChannels(params int[] channels)
        {
            foreach (var channel in channels)
                if (ValidateChannel(channel))
                    this.channels &= ~(1 << (channel - 1));

            SetActiveChannels();
        }

        public void Init()
        {
            players.Add(this);
            if (synthesizer == null)
            {
                Logger.LogError("No synthesizer specified");
                enabled = false;
                return;
            }

            Logger.AddReference();
            Settings.AddReference();
            synthesizer.AddReference();
            synthPtr = Api.Synth.Create(Settings.Ptr);
            Api.Synth.SetGain(synthPtr, gain);
            playerPtr = Api.Player.Create(synthPtr);
            Api.Player.SetTempo(playerPtr, Api.Player.TempoType.Internal, tempo);
            Api.Player.SetActiveChannels(playerPtr, channels);
            var songPath = song.GetFullPath();
            if (songPath.Length > 0)
            {
                if (File.Exists(songPath))
                    Api.Player.Add(playerPtr, song.GetFullPath());
                else
                    Logger.LogError("Song file missing: " + songPath);
            }
            else
            {
                Logger.LogError("No song specified");
            }

            if (loop.Enabled)
            {
                Api.Player.SetLoop(playerPtr, -1);
                if (loop.Value > 0) Api.Player.SetLoopBegin(playerPtr, loop.Value);
            }

            Api.Player.SetEnd(playerPtr, endTicks.Enabled ? endTicks.Value : -1);
            Api.Player.Stop(playerPtr);
            prepareJob = new PrepareJob(playerPtr).Schedule();
            unloadDelay = -1;
        }

        private void CreateDriver()
        {
            if (synthesizer.SoundFontPtr != IntPtr.Zero)
            {
                if (Api.Synth.SoundFontCount(synthPtr) == 0) Api.Synth.AddSoundFont(synthPtr, synthesizer.SoundFontPtr);

                driver = Api.Driver.Create(Settings.Ptr, synthPtr);
            }
        }

        private bool ValidateChannel(int channel)
        {
            if (channel < 1 || channel > 16)
            {
                Logger.LogError("Invalid channel: " + channel);
                return false;
            }

            return true;
        }

        private void SetActiveChannels()
        {
            if (playerPtr != IntPtr.Zero) Api.Player.SetActiveChannels(playerPtr, channels);
        }

        private struct PrepareJob : IJob
        {
            [NativeDisableUnsafePtrRestriction] [ReadOnly]
            private readonly IntPtr playerPtr;

            public PrepareJob(IntPtr playerPtr)
            {
                this.playerPtr = playerPtr;
            }

            public void Execute()
            {
                Api.Player.Prepare(playerPtr);
            }
        }
    }
}