using System.IO;
using MeltySynth;
using UnityEngine;
using static MeltySynth.Synthesizer;

namespace HGS.Tone
{
  public class ToneSequencer : MonoBehaviour
  {
    ToneSoundFont soundFont = null;
    [SerializeField] bool isLoop = false;
    Synthesizer _synthesizer;
    MidiFileSequencer _sequencer;

    bool _isPlaying = false;
    bool _isInitialized = false;

    bool IsPlaying => _isPlaying;

    public OnMidiMessage onMidiMessage;

    public void Init()
    {
      if (_isInitialized)
      {
        Debug.LogWarning("ToneSequencer is already initialized");
        return;
      }
      CreateDriver();
      _isInitialized = true;
    }

    public void CreateSynth(byte[] sfBytes)
    {
      var sf = new SoundFont(new MemoryStream(sfBytes));
      _synthesizer = new Synthesizer(sf, AudioSettings.outputSampleRate);
      _synthesizer.onMidiMessage = onMidiMessage;
      _sequencer = new MidiFileSequencer(_synthesizer);
    }

    public void Play(string file)
    {
      if (!_isInitialized) Init();

      var asset = Resources.Load<TextAsset>(file);
      Play(asset.bytes);
    }

    public void Play(byte[] midBytes)
    {
      _sequencer.Stop();
      var midi = new MidiFile(new MemoryStream(midBytes));
      _sequencer.Play(midi, isLoop);
      _isPlaying = true;
    }

    public void SetInstrument(int channel, int number)
    {
      _synthesizer.ProcessMidiMessage(channel, 0xC0, number, 0);
    }

    public void SetInstrument(int channel, MidiInstrumentCode instrumentCode)
    {
      SetInstrument(channel, (int)instrumentCode);
    }

    public void Stop()
    {
      _sequencer.Stop();
      _isPlaying = false;
    }

    void CreateDriver()
    {
      var driver = gameObject.GetComponent<ToneAudioDriver>();
      if (driver == null) driver = gameObject.AddComponent<ToneAudioDriver>();

      driver.SetRenderer(_sequencer);
    }
  }
}