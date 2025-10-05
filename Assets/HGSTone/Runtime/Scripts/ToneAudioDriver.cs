using MeltySynth;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace HGS.Tone {
    public class ToneAudioDriver : MonoBehaviour {
        private IAudioRenderer _audioRenderer;
        private float[] _buffer;
        private int _sampleRate = 48000;
        private const int CHUNK_SIZE = 2048;
        private float _initDelay = 0.25f;
        private int _bufferPos = 0;
        private float Volume = 1.0f;

        private void Awake() {
            _sampleRate = AudioSettings.outputSampleRate;
            
            #if UNITY_WEBGL
                _buffer = new float[CHUNK_SIZE];
                JsAudioLib.Init(_sampleRate, _initDelay);
            #else
                // if (!gameObject.TryGetComponent(out AudioSource source)) {
                //     var audioSource = gameObject.AddComponent<AudioSource>();
                //     audioSource.playOnAwake = true;
                //     audioSource.spatialBlend = 0;
                // }
            #endif
        }

        public bool BufferingStopped { get; set; } = true;

        public void SetRenderer(IAudioRenderer audioRenderer) {
            _audioRenderer = audioRenderer;
        }

        public void SetVolume(float volume) {
            #if UNITY_WEBGL
                JsAudioLib.SetVolume(volume);
            #else
                if (TryGetComponent(out AudioSource source)) {
                    source.volume = volume;
                }
            #endif
            Volume = volume;
        }

        public float GetVolume() {
            #if UNITY_WEBGL
                return Volume;
            #else
                if (TryGetComponent(out AudioSource source)) {
                    return source.volume;
                }
            #endif
            return 1.0f;
        }

        // WEBGL implementation & JsAudioLib by https://github.com/hecomi/UnityWebGLAudioStream
        private void Update() {
            #if UNITY_WEBGL
                if (_audioRenderer == null || _buffer == null) return;
                int samplesToGenerate = (int)(_sampleRate * Time.unscaledDeltaTime);
                
                while (samplesToGenerate > 0) {
                    int samplesToWrite = Math.Min(CHUNK_SIZE - _bufferPos, samplesToGenerate);
                    
                    float[] tempBuffer = new float[samplesToWrite];
                    _audioRenderer.RenderMono(tempBuffer);
                    
                    Array.Copy(tempBuffer, 0, _buffer, _bufferPos, samplesToWrite);
                    _bufferPos += samplesToWrite;
                    
                    if (_bufferPos >= CHUNK_SIZE) {
                        // Debug.Log((BufferingStopped ? "not" : "yes") + " sending!!");
                        if (/*Array.TrueForAll(_buffer, x => x == 0) || */BufferingStopped) {}
                        else JsAudioLib.Play(_buffer, CHUNK_SIZE);
                        Array.Clear(_buffer, 0, CHUNK_SIZE);
                        _bufferPos = 0;
                    }
                    
                    samplesToGenerate -= samplesToWrite;
                }
            #endif
        }

        private void OnAudioFilterRead(float[] data, int channels) {
            #if !UNITY_WEBGL || UNITY_EDITOR
                if (_audioRenderer == null) return;
                
                _buffer = new float[data.Length];
                _audioRenderer.RenderInterleaved(_buffer);
                _buffer.CopyTo(data, 0);
            #endif
        }
    }
    
    public static class JsAudioLib
    {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        public static extern void Init(int sampleRate, float initDelay);
        [DllImport("__Internal")]
        public static extern void Play(float[] array, int size);
        [DllImport("__Internal")]
        public static extern void SetVolume(float volume);
        [DllImport("__Internal")]
        public static extern void SetVolumeMultiplier(float volumeMultiplier);
#else
        public static void Init(int sampleRate, float initDelay) { /*Debug.Log($"Init({sampleRate}, {initDelay})");*/ }
        public static void Play(float[] array, int size) { /*Debug.Log($"Play({array}, {size})");*/ }
        public static void SetVolume(float volume) { /*Debug.Log($"SetVolume({volume})");*/ }
        public static void SetVolumeMultiplier(float volumeMultiplier) { /*Debug.Log($"SetVolumeMultiplier({volumeMultiplier})");*/ }
#endif
    }
}