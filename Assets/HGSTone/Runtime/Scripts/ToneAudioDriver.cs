using MeltySynth;
using UnityEngine;

namespace HGS.Tone
{
  public class ToneAudioDriver : MonoBehaviour
  {
    private IAudioRenderer _audioRenderer;
    private float[] _buffer;

    private void Awake()
    {
      CreateAudioSource();
    }

    private void CreateAudioSource()
    {
      if (!gameObject.TryGetComponent(out AudioSource source))
      {
        gameObject.AddComponent<AudioSource>();
      }
    }

    public void SetRenderer(IAudioRenderer audioRenderer)
    {
      _audioRenderer = audioRenderer;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
      _buffer = new float[data.Length];
      _audioRenderer.RenderInterleaved(_buffer);
      _buffer.CopyTo(data, 0);
    }
  }
}