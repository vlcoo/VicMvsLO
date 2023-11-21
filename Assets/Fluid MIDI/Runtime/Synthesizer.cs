using System;
using System.IO;
using FluidSynth;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace FluidMidi
{
    public class Synthesizer : MonoBehaviour
    {
        [SerializeField] public StreamingAsset soundFont = new();

        private int count;
        private JobHandle loadSoundFontJob;
        private IntPtr synthPtr;

        internal IntPtr SoundFontPtr =>
            loadSoundFontJob.IsCompleted ? Api.Synth.GetSoundFont(synthPtr, 0) : IntPtr.Zero;

        private void Reset()
        {
            if (Directory.Exists(Application.streamingAssetsPath))
            {
                var files = Directory.GetFiles(Application.streamingAssetsPath, "*.sf2", SearchOption.AllDirectories);
                if (files.Length == 1) soundFont.SetFullPath(files[0].Replace(Path.DirectorySeparatorChar, '/'));
            }
        }

        public void OnEnable()
        {
        }

        private void OnDisable()
        {
            RemoveReference();
        }

        private void OnValidate()
        {
            var soundFontPath = soundFont.GetFullPath();
            if (soundFontPath.Length > 0 && Api.Misc.IsSoundFont(soundFontPath) == 0)
            {
                Logger.LogError("Not a sound font: " + soundFontPath);
                soundFont.SetFullPath(string.Empty);
            }
        }

        internal void AddReference()
        {
            if (count == 0)
            {
                Logger.AddReference();
                Settings.AddReference();
                synthPtr = Api.Synth.Create(Settings.Ptr);
                loadSoundFontJob = new LoadSoundFontJob(synthPtr, soundFont.GetFullPath()).Schedule();
            }

            ++count;
        }

        internal void RemoveReference()
        {
            if (--count == 0)
            {
                if (!loadSoundFontJob.IsCompleted) Logger.LogWarning("Destroying Synthesizer before sound font loaded");
                loadSoundFontJob.Complete();
                Api.Synth.Destroy(synthPtr);
                Settings.RemoveReference();
                Logger.RemoveReference();
            }
        }

        public void Init()
        {
            AddReference();
        }

        private struct LoadSoundFontJob : IJob
        {
            [ReadOnly] [NativeDisableUnsafePtrRestriction]
            private readonly IntPtr synth;

            [ReadOnly] [DeallocateOnJobCompletion] private readonly NativeArray<char> path;

            public LoadSoundFontJob(IntPtr synth, string path)
            {
                this.synth = synth;
                this.path = new NativeArray<char>(path.ToCharArray(), Allocator.Persistent);
            }

            public void Execute()
            {
                var pathString = new string(path.ToArray());
                if (pathString.Length > 0)
                {
                    if (File.Exists(pathString))
                    {
                        Logger.Log("Loading sound font: " + pathString);
                        Api.Synth.LoadSoundFont(synth, pathString, 0);
                    }
                    else
                    {
                        Logger.LogError("Sound font file missing: " + pathString);
                    }
                }
                else
                {
                    Logger.LogError("No sound font specified");
                }
            }
        }
    }
}