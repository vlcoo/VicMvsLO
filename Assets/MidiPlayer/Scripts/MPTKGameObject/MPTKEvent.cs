using System;
using System.Collections.Generic;
using UnityEngine;

namespace MidiPlayerTK
{

    /// <summary>
    /// Description of a MIDI Event. It's the heart of MPTK! Essential to handling MIDI by script from all others classes as MidiStreamPlayer, MidiFilePlayer, MidiFileLoader, MidiFileWriter2 ...\n
    /// 
    /// The MPTKEvent main property is MPTKEvent.Command, the content and role of other properties (as MPTKEvent.Value) depend on the value of MPTKEvent.Command. Look at the MPTKEvent.Value property.\n
    /// With this class, you can: play and stop a note, change instrument (preset, patch, ...), change some control as modulation (Pro) ...\n
    /// Use this class in relation with these classes:
    ///      - MidiFileLoader     to read MIDI events from a MIDI file.\n
    ///      - MidiFilePlayer     process MIDI events, thank to the class event OnEventNotesMidi when MIDI events are played from the internal MIDI sequencer.\n
    ///      - MidiFileWriter2    generate MIDI Music file from your own algorithm.\n
    ///      - MidiStreamPlayer   real-time generation of MIDI Music from your own algorithm.\n
    /// See here https://paxstellar.fr/class-mptkevent and here https://mptkapi.paxstellar.com/d9/d50/class_midi_player_t_k_1_1_m_p_t_k_event.html
    /// \n
    /// Also, below an example with MidiStreamPlayer\n
    /// @code
    /// 
    /// // Find a MidiStreamPlayer Prefab from the scene
    /// MidiStreamPlayer midiStreamPlayer = FindObjectOfType<MidiStreamPlayer>();
    /// midiStreamPlayer.MPTK_StartMidiStream();
    /// 
    /// // Change instrument to Marimba for channel 0
    /// MPTKEvent PatchChange = new MPTKEvent() {
    ///        Command = MPTKCommand.PatchChange,
    ///        Value = 12, // generally Marimba but depend on the SoundFont selected
    ///        Channel = 0 }; // Instrument are defined by channel (from 0 to 15). So at any time, only 16 différents instruments can be used simultaneously.
    /// midiStreamPlayer.MPTK_PlayEvent(PatchChange);    
    ///
    /// // Play a C4 during one second with the Marimba instrument
    /// MPTKEvent NotePlaying = new MPTKEvent() {
    ///        Command = MPTKCommand.NoteOn,
    ///        Value = 60, // play a C4 note
    ///        Channel = 0,
    ///        Duration = 1000, // one second
    ///        Velocity = 100 };
    /// midiStreamPlayer.MPTK_PlayEvent(NotePlaying);    
    /// @endcode
    /// </summary>
    public partial class MPTKEvent : ICloneable
    {
        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>@brief
        /// Track index of the event in the midi. \n
        /// There is any impact on the music played. \n
        /// It's just a cool way to regroup MIDI events in a ... track like in a sequencer.\n
        /// Track 0 is the first track read from the midi file.
        /// </summary>
        public long Track;

        /// <summary>@brief
        /// Time in Midi Tick (part of a Beat) of the Event since the start of playing the midi file.\n
        /// This time is independent of the Tempo or Speed. Not used for MidiStreamPlayer nor MidiInReader because they are real-time player.
        /// </summary>
        public long Tick;

        /// <summary>@brief
        /// Measure (bar) when this event will be played. Measure is calculated with the Time Signature event when a MIDI file is loaded.\n
        /// By default the time signature is 4/4.
        /// @version 2.10.0
        /// </summary>
        public int Measure;

        /// <summary>@brief
        /// Beat in measure of this event (have sense only for noteon). Measure is calculated with the Time Signature event when a MIDI file is loaded.\n
        /// By default the time signature is 4/4.
        /// @version 2.10.0
        /// </summary>
        public int Beat;

        /// <summary>@brief
        /// Initial Event Index in the MIDI list, set only when a MIDI file is loaded.
        /// </summary>
        public int Index;

        /// <summary>@brief
        /// V2.9.0 Time from System.DateTime when the Event has been created (in the constructor of this class). Divide by 10000 to get milliseconds.\n
        /// Replace TickTime from previous version (was confusing).
        /// </summary>
        public long CreateTime;

        /// <summary>@brief
        /// Real time in milliseconds of this MIDI Event from the start of the MIDI. It take into account the tempo changes but not MPTK_Speed of the MidiPlayer.\n
        /// v2.89.6 Correct the time shift when a tempo change is read. Thanks to Ken Scott http://www.youtube.com/vjchaotic for the tip.\n
        /// Not used for MidiStreamPlayer nor MidiInReader (MIDI events are always in real-time from your app).
        /// </summary>
        public float RealTime;

        /// <summary>@brief
        /// MIDI Command code. Defined the type of message. See #MPTKCommand (Note On, Control Change, Patch Change...)
        /// </summary>
        public MPTKCommand Command;

        /// <summary>@brief
        /// For #Command = #MPTKCommand.ControlChange contains the controller code (Modulation, Pan, Bank Select ...).\n
        /// #Value properties will contains the value of the controller. See #MPTKController.
        /// </summary>
        public MPTKController Controller;

        /// <summary>@brief
        /// For #Command = #MPTKCommand.MetaEvent contains MetaEvent Code. (Lyric, TimeSignature, ...).\n
        /// Others properties will contains the value of the meta. See #MPTKMeta (TextEvent, Lyric, TimeSignature, ...).
        /// </summary>
        public MPTKMeta Meta;

        /// <summary>@brief
        /// For #Command = #MPTKCommand.MetaEvent contains text information hold (TextEvent, Lyric, TimeSignature, ...)
        /// </summary>
        public string Info;

        /// <summary>@brief
        /// Contains a value in relation with the #Command.
        ///! <ul>
        ///! <li>#Command = #MPTKCommand.NoteOn
        ///!     <ul>
        ///!       <li> #Value contains midi note as defined in the MIDI standard and matched to the Middle C (note number 60) as C4.\n
        ///!         look here: http://www.music.mcgill.ca/~ich/classes/mumt306/StandardMIDIfileformat.html#BMA1_3
        ///        </li>
        ///!     </ul>
        ///! </li>
        ///! <li>#Command = #MPTKCommand.ControlChange
        ///!     <ul>
        ///!       <li> #Value contains controller value, see #MPTKController</li>
        ///!     </ul>
        ///! </li>
        ///! <li>#Command = #MPTKCommand.PatchChange
        ///!     <ul>
        ///!        <li>  #Value contains patch/preset/instrument value. See the current SoundFont to find value associated to each instrument.\n
        ///!                If your SoundFont follows the General Midi (GM) map, instrument Patch map will be like ths one:\n
        ///                 http://www.music.mcgill.ca/~ich/classes/mumt306/StandardMIDIfileformat.html#BMA1_4    
        ///!        </li>
        ///!     </ul>
        ///! </li>
        ///! <li>#Command = #MPTKCommand.MetaEvent and #Meta equal:
        ///!     <ul>
        ///!        <li>  #MPTKMeta.SetTempo</li>
        ///!        <ul>
        ///!            <li>  #Value contains new Microseconds Per Beat Note</li>
        ///!        </ul>
        ///!        <li>  #MPTKMeta.TimeSignature. See #MPTKMeta.TimeSignature</li>
        ///!        <li>  #MPTKMeta.KeySignature. See #MPTKMeta.KeySignature</li>
        ///!     </ul>
        ///! </li>
        ///! </ul>
        /// </summary>
        public int Value;

        /// <summary>@brief
        /// Midi channel fom 0 to 15 (9 for drum)
        /// </summary>
        public int Channel;

        /// <summary>@brief
        /// When #Command = #MPTKCommand.NoteOn, the velocity between 0 and 127.
        /// </summary>
        public int Velocity;

        /// <summary>@brief
        /// When #Command = #MPTKCommand.NoteOn, contains duration of the note in millisecond.\n
        /// Set -1 for a noteon played indefinitely.\n
        /// @version 2.10.0 
        /// @note
        /// Others #Command roles removed:
        ///    - SetTempo: no longer contains new tempo (quarter per minute)\n
        ///    - TimeSignature: no longer contains the Denominator (Beat unit: 1 means 2, 2 means 4 (crochet), 3 means 8 (quaver), 4 means 16, ...)\n
        ///    - KeySignature: no longer contains the MajorMinor flag.\n
        /// </summary>
        public long Duration;

        /// <summary>@brief
        /// Short delay before playing the note in millisecond. Available only in Core mode.
        /// Apply only on NoteOn event.
        /// </summary>
        public long Delay;

        /// <summary>@brief
        /// When #Command = #MPTKCommand.NoteOn, duration of the note in MIDI Tick. 
        /// @details
        /// Duration in ticks is converted in duration in millisecond see (#Duration) when the MIDI file is loaded.
        /// @note
        /// Maestro does not use this value for playing a MIDI file but the #Duration in millisecond.
        /// https://en.wikipedia.org/wiki/Note_value
        /// </summary>
        public int Length;

        /// <summary>@brief
        /// When #Command = #MPTKCommand.NoteOn, length as https://en.wikipedia.org/wiki/Note_value
        /// </summary>
        public enum EnumLength { Whole, Half, Quarter, Eighth, Sixteenth }

        /// <summary>@brief
        /// Origin of the message. Midi ID if from Midi Input else zero. V2.83: rename source to Source et set public.
        /// </summary>
        public uint Source;

        /// <summary>@brief
        /// Associate an Id with this event.\n
        /// When reading a Midi file with MidiFilePlayer: this Id is unique for all Midi events played for this Midi.\n
        /// Consequently, when switching Midi, MPTK_ClearAllSound is able to clear (note-off) only the voices associated with this Midi file.\n
        /// Switching between Midi playing is very quick.\n
        /// Could also be used for other prefab as MidiStreamPlayer for your specific need, but don't change this properties with MidiFilePlayer.
        /// </summary>
        public int IdSession;


        /// <summary>@brief
        /// Tag information free of use for application purpose
        /// </summary>
        public object Tag;

        /// <summary>@brief
        /// List of voices associated to a NoteOn Event. It's frequent that multiple samples are used simultaneously for playing a note.
        /// </summary>
        public List<fluid_voice> Voices;

        /// <summary>@brief
        /// Check if playing of this midi event is over (all voices are OFF)
        /// </summary>
        public bool IsOver
        {
            get
            {
                if (Voices != null)
                {
                    foreach (fluid_voice voice in Voices)
                        if (voice.status != fluid_voice_status.FLUID_VOICE_OFF)
                            return false;
                }
                // All voices are off or empty
                return true;
            }
        }

        public MPTKEvent()
        {
            Command = MPTKCommand.NoteOn;
            // V2.82 set default value
            Duration = -1;
            Channel = 0;
            Delay = 0;
            Velocity = 127; // max
            IdSession = -1;
            CreateTime = DateTime.UtcNow.Ticks;
        }

        /// <summary>@brief
        /// Delta time in system time (calculated with DateTime.UtcNow.Ticks) since the creation of this event.\n
        /// Mainly useful to evaluate MPTK latency. One system ticks equal 100 nano second.\n
        /// @note Disabled by default. Defined DEBUG_PERF_AUDIO in MidiSynth to activate for debug purpose only.
        /// </summary>
        public long LatenceTime { get { return DateTime.UtcNow.Ticks - CreateTime; } }

        /// <summary>@brief
        /// Delta time in milliseconds (calculated with DateTime.UtcNow.Ticks) since the creation of this event.\n
        /// Mainly useful to evaluate MPTK latency. One system ticks equal 100 nano second.\n
        /// @note Disabled by default. Defined DEBUG_PERF_AUDIO in MidiSynth to activate for debug purpose only.
        /// </summary>
        public long LatenceTimeMillis { get { return LatenceTime / fluid_voice.Nano100ToMilli; } }

        /// <summary>@brief
        /// Create a MPTK Midi event from a midi input message
        /// </summary>
        /// <param name="data"></param>
        public MPTKEvent(ulong data)
        {
            Source = (uint)(data & 0xffffffffUL);
            Command = (MPTKCommand)((data >> 32) & 0xFF);
            if (Command < MPTKCommand.Sysex)
            {
                Channel = (int)Command & 0xF;
                Command = (MPTKCommand)((int)Command & 0xF0);
            }
            byte data1 = (byte)((data >> 40) & 0xff);
            byte data2 = (byte)((data >> 48) & 0xff);

            if (Command == MPTKCommand.NoteOn && data2 == 0)
                Command = MPTKCommand.NoteOff;

            //if ((int)Command != 0xFE)
            //    Debug.Log($"{data >> 32:X}");

            switch (Command)
            {
                case MPTKCommand.NoteOn:
                    Value = data1; // Key
                    Velocity = data2;
                    Duration = -1; // no duration are defined in Midi flux
                    break;
                case MPTKCommand.NoteOff:
                    Value = data1; // Key
                    Velocity = data2;
                    break;
                case MPTKCommand.KeyAfterTouch:
                    Value = data1; // Key
                    Velocity = data2;
                    break;
                case MPTKCommand.ControlChange:
                    Controller = (MPTKController)data1;
                    Value = data2;
                    break;
                case MPTKCommand.PatchChange:
                    Value = data1;
                    break;
                case MPTKCommand.ChannelAfterTouch:
                    Value = data1;
                    break;
                case MPTKCommand.PitchWheelChange:
                    Value = data2 << 7 | data1; // Pitch-bend is transmitted with 14-bit precision. 
                    break;
                case MPTKCommand.TimingClock:
                case MPTKCommand.StartSequence:
                case MPTKCommand.ContinueSequence:
                case MPTKCommand.StopSequence:
                case MPTKCommand.AutoSensing:
                    // no value
                    break;

            }
        }

        /// <summary>@brief
        /// Build a packet midi message from a MPTKEvent. Example:  0x00403C90 for a noton (90h, 3Ch note,  40h volume)
        /// </summary>
        /// <returns></returns>
        public ulong ToData()
        {
            ulong data = (ulong)Command | ((ulong)Channel & 0xF);
            switch (Command)
            {
                case MPTKCommand.NoteOn:
                    data |= (ulong)Value << 8 | (ulong)Velocity << 16;
                    break;
                case MPTKCommand.NoteOff:
                    data |= (ulong)Value << 8 | (ulong)Velocity << 16;
                    break;
                case MPTKCommand.KeyAfterTouch:
                    data |= (ulong)Value << 8 | (ulong)Velocity << 16;
                    break;
                case MPTKCommand.ControlChange:
                    data |= (ulong)Controller << 8 | (ulong)Value << 16;
                    break;
                case MPTKCommand.PatchChange:
                    data |= (ulong)Value << 8;
                    break;
                case MPTKCommand.ChannelAfterTouch:
                    data |= (ulong)Value << 8;
                    break;
                case MPTKCommand.PitchWheelChange:
                    // The pitch bender is measured by a fourteen bit value. Center (no pitch change) is 2000H. 
                    // Two data after the command code 
                    //  1) the least significant 7 bits. 
                    //  2) the most significant 7 bits.
                    data |= ((ulong)Value & 0x7F) << 8 | ((ulong)Value & 0x7F00) << 16;
                    break;
                case MPTKCommand.TimingClock:
                case MPTKCommand.StartSequence:
                case MPTKCommand.ContinueSequence:
                case MPTKCommand.StopSequence:
                case MPTKCommand.AutoSensing:
                    data = (ulong)Command;
                    break;

            }
            return data;
        }

        /// <summary>@brief
        /// Convert Beat Per Minute to duration of a quarter in microsecond.\n
        /// With BPM=1,   microsecondsPerQuaterNote=60 000 000 µs ->  60 secondes per quarter (quite slow!)\n
        /// With BPM=120, microsecondsPerQuaterNote=500?000 µs -> 0.5 seconde per quarter\n
        /// </summary>
        /// <param name="bpm">Beats Per Minute (with assumption beat=quarter)</param>
        /// <returns>60000000 / bpm or 500000 if bpm <= 0</returns>
        public static int BeatPerMinute2QuarterPerMicroSecond(double bpm)
        {
            return bpm > 0 ? (int)(60000000d / bpm) : 500000;
        }

        /// <summary>@brief
        /// Convert duration of a quarter in microsecond to Beats Per Minute (with assumption beat=quarter).\n
        /// With microsecondsPerQuaterNote=500?000 µs, BPM = 120
        /// </summary>
        /// <param name="microsecondsPerQuaterNote"></param>
        /// <returns>60000000 / bpm or 120 if microsecondsPerQuaterNote <= 0</returns>
        public static double QuarterPerMicroSecond2BeatPerMinute(int microsecondsPerQuaterNote)
        {
            return microsecondsPerQuaterNote > 0 ? 60000000d / microsecondsPerQuaterNote : 120;
        }

        /// <summary>
        /// Store four bytes into one integer.
        /// </summary>
        /// <param name="b1">byte 0 - less significant</param>
        /// <param name="b2">byte 1 </param>
        /// <param name="b3">byte 2 </param>
        /// <param name="b4">byte 3 - moss significant</param>
        /// <returns>(b4 << 24) | (b3 << 16) | (b2 << 8) | b1</returns>
        static public int BuildIntFromBytes(byte b1, byte b2, byte b3, byte b4)
        {
            return (b4 << 24) | (b3 << 16) | (b2 << 8) | b1;
        }

        /// <summary>
        /// Extract byte position 'n' from an integer 
        /// </summary>
        /// <param name="v">value build with #BuildIntFromBytes</param>
        /// <param name="n">byte position from 0 (less significant) to 3 (most significant)</param>
        /// <returns>(v >> (8*n)) & 0xFF</returns>
        static public byte ExtractFromInt(uint v, int n)
        {
            return (byte)((v >> (8 * n)) & 0xFF);
        }

        /// <summary>
        /// Extract value 2 from a double int build with #BuildIntFromBytes
        /// </summary>
        /// <param name="v">value build with #BuildIntFromBytes</param>
        /// <returns>v - (v / 100) * 100</returns>
        //static public int ExtractDoubleValue2(int v)
        //{
        //    return v - (v / 100) * 100;
        //}

        /// <summary>@brief
        /// Build a string description of the Midi event. V2.83 removes "end of lines" on each returns string.
        /// For a better result when displayed on the console (Debug.Log), enable "Monospace font" in the setting of the console (three vertical dot in the panel)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = "";

            string command = "unknown";
            switch (Command)
            {
                case MPTKCommand.NoteOn: command = "Note On"; break;
                case MPTKCommand.NoteOff: command = "Note Off"; break;
                case MPTKCommand.PatchChange: command = "Preset Change"; break;
                case MPTKCommand.ControlChange: command = $"Controller {Controller}"; break;
                case MPTKCommand.KeyAfterTouch: command = "Key After Touch"; break;
                case MPTKCommand.ChannelAfterTouch: command = "Channel After Touch" ; break;
                case MPTKCommand.PitchWheelChange: command = "Pitch Wheel Change"; break;
                case MPTKCommand.MetaEvent:
                    try
                    {
                        switch (Meta)
                        {
                            case MPTKMeta.KeySignature: command = $"Key Signature"; break;
                            case MPTKMeta.TimeSignature: command = $"Time Signature"; break;
                            case MPTKMeta.SetTempo: command = $"Tempo"; break;
                            default: command = $"Meta {Meta}"; break;
                        }
                    }
                    catch { command = $"{Meta} error value:{Value}"; }
                    break;

                case MPTKCommand.TimingClock:
                case MPTKCommand.StartSequence:
                case MPTKCommand.ContinueSequence:
                case MPTKCommand.StopSequence:
                case MPTKCommand.AutoSensing: command += $"Command:{Command}"; break;
                default: command += $"Command:{Command}"; break;
            }

            string position = $"T:{Track,-2:00} ";
            if (Command == MPTKCommand.NoteOn || Command == MPTKCommand.NoteOff || Command == MPTKCommand.KeyAfterTouch || Command == MPTKCommand.ControlChange ||
                Command == MPTKCommand.PatchChange || Command == MPTKCommand.ChannelAfterTouch || Command == MPTKCommand.PitchWheelChange)
                position += $"C:{Channel,-2:00} ";
            else
                position += "     ";
            position += $"Time:{RealTime / 1000f:F2} / {Tick,-7:0000000} tick Measure:{Measure}/{Beat}";

            command = $"{command,-30} {position}";

            switch (Command)
            {
                case MPTKCommand.NoteOn:
                    string sDuration = Duration < 0 ? "Inf.    " : $"{Duration / 1000f:F2} / {Length} ticks";
                    result += $"{command} Note:{Value,-3:000} Velocity:{Velocity:000} Duration:{sDuration}";
                    break;
                case MPTKCommand.NoteOff:
                    result += $"{command} Note:{Value,-3:000} Velocity:{Velocity}";
                    break;
                case MPTKCommand.PatchChange:
                    result += $"{command} Value:{Value,-3:000}";
                    break;
                case MPTKCommand.ControlChange:
                    result += $"{command} Value:{Value,-3:000}";
                    break;
                case MPTKCommand.KeyAfterTouch:
                    result += $"{command} Not processed by Maestro Synth";
                    break;
                case MPTKCommand.ChannelAfterTouch:
                    result += $"{command} Not processed by Maestro Synth";
                    break;
                case MPTKCommand.PitchWheelChange:
                    result += $"{command} Value:{Value,-3:000}";
                    break;
                case MPTKCommand.MetaEvent:
                    try
                    {
                        switch (Meta)
                        {
                            case MPTKMeta.KeySignature: result = $"{command} SharpsFlats:{MPTKEvent.ExtractFromInt((uint)Value, 0)} MajorMinor:{MPTKEvent.ExtractFromInt((uint)Value, 1)}"; break;
                            case MPTKMeta.TimeSignature: result = $"{command} Numerator:{MPTKEvent.ExtractFromInt((uint)Value, 0)} Denominator:{MPTKEvent.ExtractFromInt((uint)Value, 1)}"; break;
                            case MPTKMeta.SetTempo: result = $"{command} Microseconds:{Value} Tempo:{60000000 / Value:F2}"; break;
                            default:
                                string sinfo = Info ?? "";
                                result = $"{command} {sinfo}"; break;
                        }
                    }
                    catch { result = $"{command} {Meta} error value:{Value}"; }
                    break;

                case MPTKCommand.TimingClock:
                case MPTKCommand.StartSequence:
                case MPTKCommand.ContinueSequence:
                case MPTKCommand.StopSequence:
                case MPTKCommand.AutoSensing:
                    result += $"{Command}";
                    break;
                default:
                    result += $"{Command} Value:{Value} Duration:{Duration,6} Velocity:{Velocity,3} source:{Source}";
                    break;
            }
            return result;
        }
    }
}
