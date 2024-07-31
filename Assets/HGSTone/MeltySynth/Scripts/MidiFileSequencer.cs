using System;
using UnityEngine;

namespace MeltySynth
{
    /// <summary>
    /// An instance of the MIDI file sequencer.
    /// </summary>
    /// <remarks>
    /// Note that this class does not provide thread safety.
    /// If you want to do playback control and render the waveform in separate threads,
    /// you must ensure that the methods will not be called simultaneously.
    /// </remarks>
    public sealed class MidiFileSequencer : IAudioRenderer
    {
        private readonly Synthesizer synthesizer;

        private float speed;

        private MidiFile midiFile;
        private bool loop;

        private int blockWrote;

        private TimeSpan currentTime;
        private int msgIndex;
        private int loopIndex;
        private int startLoopTicks = 0;
        private int endLoopTicks = -1;
        private double currentTempo;

        /// <summary>
        /// Initializes a new instance of the sequencer.
        /// </summary>
        /// <param name="synthesizer">The synthesizer to be handled by the sequencer.</param>
        public MidiFileSequencer(Synthesizer synthesizer)
        {
            if (synthesizer == null)
            {
                throw new ArgumentNullException(nameof(synthesizer));
            }

            this.synthesizer = synthesizer;

            speed = 1F;
        }

        /// <summary>
        /// Plays the MIDI file.
        /// </summary>
        /// <param name="midiFile">The MIDI file to be played.</param>
        /// <param name="loop">If <c>true</c>, the MIDI file loops after reaching the end.</param>
        public void Play(MidiFile midiFile, bool loop)
        {
            if (midiFile == null)
            {
                throw new ArgumentNullException(nameof(midiFile));
            }

            this.midiFile = midiFile;
            this.loop = loop;

            blockWrote = synthesizer.BlockSize;

            currentTime = TimeSpan.Zero;
            currentTempo = midiFile.InitialTempo;
            msgIndex = 0;
            loopIndex = 0;

            synthesizer.Reset();
        }

        /// <summary>
        /// Stop playing.
        /// </summary>
        public void Stop()
        {
            midiFile = null;

            synthesizer.Reset();
        }

        // <AUTHOR> https://github.com/samhocevar/go-meltysynth/commit/6847c7aa31422948927d174434f7c75c111c9b4a
        public TimeSpan Pos()
        {
            return currentTime + MidiFile.GetTimeSpanFromSeconds(blockWrote / (float)synthesizer.SampleRate);
        }

        public void Seek(TimeSpan pos)
        {
            // When seeking in the past, we’ll have to restart message processing from the beginning of the file
            if (pos < currentTime)
            {
                synthesizer.Reset();
                msgIndex = 0;
            }

            // When seeking in the past or far into the future, find a synchronisation block
            // seekLookBehind seconds before the seek point, play all messages until that block,
            // and stop all notes.
            const double seekLookBehind = 0.15; // decided by trial and error
            if (pos < currentTime || pos > currentTime + MidiFile.GetTimeSpanFromSeconds(seekLookBehind))
            {
                var syncBlock = Math.Max(0, pos.TotalSeconds - seekLookBehind) * synthesizer.SampleRate /
                                synthesizer.BlockSize;
                blockWrote = 0;
                currentTime = MidiFile.GetTimeSpanFromSeconds(syncBlock * synthesizer.BlockSize / synthesizer.SampleRate);
                ProcessEvents();
                synthesizer.NoteOffAll(false);
            }

            // Play but discard samples up to the desired seek point
            int skipSamples = (int)Math.Max(0, (pos - currentTime).TotalSeconds * synthesizer.SampleRate);
            var left = new float[skipSamples];
            var right = new float[skipSamples];
            Render(left, right);
        }
        // </AUTHOR>

        public double PosTicks()
        {
            return Pos().TotalSeconds * midiFile.Resolution * currentTempo / 60.0;
        }

        public void Seek(int midiTicks)
        {
            var seekTime = MidiFile.GetTimeSpanFromSeconds(60.0 / (float)(midiFile.Resolution * currentTempo) * midiTicks);
            Seek(seekTime);
        }

        /// <inheritdoc/>
        public void Render(Span<float> left, Span<float> right)
        {
            if (left.Length != right.Length)
            {
                throw new ArgumentException("The output buffers must be the same length.");
            }

            var wrote = 0;
            while (wrote < left.Length)
            {
                if (blockWrote == synthesizer.BlockSize)
                {
                    ProcessEvents();
                    blockWrote = 0;
                    currentTime += MidiFile.GetTimeSpanFromSeconds((double)speed * synthesizer.BlockSize / synthesizer.SampleRate);
                }

                var srcRem = synthesizer.BlockSize - blockWrote;
                var dstRem = left.Length - wrote;
                var rem = Math.Min(srcRem, dstRem);

                synthesizer.Render(left.Slice(wrote, rem), right.Slice(wrote, rem));

                blockWrote += rem;
                wrote += rem;
            }
        }

        private void ProcessEvents()
        {
            if (midiFile == null)
            {
                return;
            }

            while (msgIndex < midiFile.Messages.Length)
            {
                var time = midiFile.Times[msgIndex];
                var msg = midiFile.Messages[msgIndex];
                if (time <= currentTime)
                {
                    if (msg.Type == MidiFile.MessageType.Normal)
                    {
                        synthesizer.ProcessMidiMessage(msg.Channel, msg.Command, msg.Data1, msg.Data2);
                    }
                    else if (msg.Type == MidiFile.MessageType.TempoChange)
                    {
                        currentTempo = msg.Tempo;
                    }
                    else if (loop)
                    {
                        if (msg.Type == MidiFile.MessageType.LoopStart)
                        {
                            loopIndex = msgIndex;
                        }
                        else if (msg.Type == MidiFile.MessageType.LoopEnd)
                        {
                            Seek(midiFile.Times[loopIndex]);
                        }
                    }
                    msgIndex++;
                }
                else
                {
                    break;
                }
            }

            if ((msgIndex == midiFile.Messages.Length || (EndLoopTicks > 0 && PosTicks() >= EndLoopTicks)) && loop)
            {
                Seek(startLoopTicks);
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the current playback position is at the end of the sequence.
        /// </summary>
        /// <remarks>
        /// If the <see cref="Play(MidiFile, bool)">Play</see> method has not yet been called, this value is true.
        /// This value will never be <c>true</c> if loop playback is enabled.
        /// </remarks>
        public bool EndOfSequence
        {
            get
            {
                if (midiFile == null)
                {
                    return true;
                }
                else
                {
                    return msgIndex == midiFile.Messages.Length;
                }
            }
        }

        /// <summary>
        /// Gets or sets the playback speed.
        /// </summary>
        /// <remarks>
        /// The default value is 1.
        /// The tempo will be multiplied by this value.
        /// </remarks>
        public float Speed
        {
            get => speed;

            set
            {
                if (value > 0)
                {
                    speed = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("The playback speed must be a positive value.");
                }
            }
        }

        public int StartLoopTicks
        {
            get => startLoopTicks;

            set
            {
                if (value >= 0)
                {
                    startLoopTicks = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("The start loop ticks must be a non-negative value.");
                }
            }
        }

        public int EndLoopTicks
        {
            get => endLoopTicks;

            set
            {
                if (value >= 0)
                {
                    endLoopTicks = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("The end loop ticks must be a non-negative value.");
                }
            }
        }
    }
}
