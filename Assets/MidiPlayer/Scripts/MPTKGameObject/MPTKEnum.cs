using System;
using System.Collections.Generic;
using UnityEngine;

namespace MidiPlayerTK
{
    /// <summary>@brief
    /// MIDI command codes. Defined the action to be done with the message: note on/off, change instrument, ...\n
    /// Depending of the command selected, others properties must be set; Value, Channel, ...\n
    /// </summary>
    public enum MPTKCommand : byte
    {
        /// <summary>@brief
        /// Note Off\n
        /// Stop the note defined with the Value and the Channel\n
        ///      - MPTKEvent#Value contains the note to stop 60=C5.\n
        ///      - MPTKEvent#Channel the midi channel between 0 and 15\n
        /// </summary>
        NoteOff = 0x80,

        /// <summary>@brief
        /// Note On.\n
        ///      - MPTKEvent#Value contains the note to play 60=C5.\n
        ///      - MPTKEvent#Duration the duration of the note in millisecond, -1 for infinite\n
        ///      - MPTKEvent#Channel the midi channel between 0 and 15\n
        ///      - MPTKEvent#Velocity between 0 and 127\n
        /// </summary>
        NoteOn = 0x90,

        /// <summary>@brief
        /// Key After-touch.\n
        /// Not processed by Maestro Synth.
        /// </summary>
        KeyAfterTouch = 0xA0,


        /// <summary>@brief
        /// Control change.\n
        ///      - MPTKEvent.Controller contains the controller to change. See #MPTKController (Modulation, Pan, Bank Select ...).\n
        ///      - MPTKEvent.Value contains the value of the controller between 0 and 127.
        /// </summary>
        ControlChange = 0xB0,

        /// <summary>@brief
        /// Patch change.\n
        ///      - MPTKEvent.Value contains patch/preset/instrument to select between 0 and 127. 
        /// </summary>
        PatchChange = 0xC0,

        /// <summary>@brief
        /// Channel after-touch.\n
        /// Not processed by Maestro Synth.\n
        /// </summary>
        ChannelAfterTouch = 0xD0,

        /// <summary>@brief
        /// Pitch wheel change\n
        /// MPTKEvent.Value contains the Pitch Wheel Value between 0 and 16383.\n
        /// Higher values transpose pitch up, and lower values transpose pitch down.\n
        /// The default sensitivity value is 2. That means that the maximum pitch bend will result in a pitch change of two semitones\n
        /// above and below the sounding note, meaning a total of four semitones from lowest to highest  pitch bend positions.
        ///     - 0 is the lowest bend positions (default is 2 semitones), 
        ///     - 8192 (0x2000) centered value, the sounding notes aren't being transposed up or down,
        ///     - 16383 (0x3FFF) is the highest  pitch bend position (default is 2 semitones)
        /// </summary>
        PitchWheelChange = 0xE0,

        /// <summary>@brief
        /// Sysex message - not processed by Maestro\n
        /// </summary>
        Sysex = 0xF0,

        /// <summary>@brief
        /// Eox (comes at end of a sysex message)  - not processed by Maestro
        /// </summary>
        Eox = 0xF7,

        /// <summary>@brief
        /// Timing clock \n
        /// (used when synchronization is required)
        /// </summary>
        TimingClock = 0xF8,

        /// <summary>@brief
        /// Start sequence\n
        /// </summary>
        StartSequence = 0xFA,

        /// <summary>@brief
        /// Continue sequence\n
        /// </summary>
        ContinueSequence = 0xFB,

        /// <summary>@brief
        /// Stop sequence\n
        /// </summary>
        StopSequence = 0xFC,

        /// <summary>@brief
        /// Auto-Sensing\n
        /// </summary>
        AutoSensing = 0xFE,

        /// <summary>@brief
        /// Meta events are optionnals information that could be defined in a MIDI. None are mandatory\n
        /// In MPTKEvent the attibute MPTKEvent#Meta defined the type of meta event. See #MPTKMeta (TextEvent, Lyric, TimeSignature, ...).\n
        ///     - if MPTKEvent#Meta = #MPTKMeta.SetTempo, MPTKEvent#Value contains new Microseconds Per Beat Note. . Please investigate MPTKEvent.QuarterPerMicroSecond2BeatPerMinute() to convert to BPM.
        ///     - if MPTKEvent#Meta = #MPTKMeta.TimeSignature, MPTKEvent#Value contains four bytes. From less significant to most significant. Please investigate MPTKEvent.ExtractFromInt().
        ///         -# Numerator (number of beats in a bar), 
        ///         -# Denominator (Beat unit: 1 means 2, 2 means 4 (crochet), 3 means 8 (quaver), 4 means 16, ...)
        ///         -# TicksInMetronomeClick, generally 24 (number of 1/32nds of a note happen by MIDI quarter note)
        ///         -# No32ndNotesInQuarterNote, generally 8 (standard MIDI clock ticks every 24 times every quarter note)
        ///     - if MPTKEvent#Meta = #MPTKMeta.KeySignature, MPTKEvent#Value contains two bytes. From less significant to most significant. Please investigate MPTKEvent.ExtractFromInt().
        ///         -# SharpsFlats (number of sharp) 
        ///         -# MajorMinor flag (0 the scale is major, 1 the scale is minor).
        ///     - for others, attribute MPTKEvent#Info contains textual information.
        /// </summary>
        MetaEvent = 0xFF,
    }

    /// <summary>@brief
    /// Midi Controller list.\n
    /// Each MIDI CC operates at 7-bit resolution, meaning it has 128 possible values. The values start at 0 and go to 127.\n
    /// Some instruments can receive higher resolution data for their MIDI control assignments. These high res assignments are defined by combining two separate CCs,\n
    /// one being the Most Significant Byte (MSB), and one being the Least Significant Byte (LSB).\n
    /// Most instruments just receive the MSB with default 7-bit resolution.
    /// See more information here https://www.presetpatch.com/midi-cc-list.aspx
    /// </summary>
    public enum MPTKController : byte
    {
        /// <summary>@brief
        /// Bank Select (MSB)
        /// </summary>
        BankSelectMsb = 0,

        /// <summary>@brief
        /// Modulation (MSB)
        /// </summary>
        Modulation = 1,

        /// <summary>@brief
        /// Breath Controller
        /// </summary>
        BreathController = 2,

        /// <summary>@brief
        /// Foot controller (MSB)
        /// </summary>
        FootController = 4,

        PORTAMENTO_TIME_MSB = 0x05,

        DATA_ENTRY_MSB = 6,

        /// <summary>@brief
        /// Channel volume (was MainVolume before v2.88.2
        /// </summary>
        VOLUME_MSB = 7,

        BALANCE_MSB = 8,

        /// <summary>@brief Pan MSB</summary>
        Pan = 10, //0xA

        /// <summary>@brief Expression (EXPRESSION_MSB)</summary>
        Expression = 11, // 0xB

        EFFECTS1_MSB = 12, //0x0C,
        EFFECTS2_MSB = 13, //0x0D,

        GPC1_MSB = 16, //0x10, /* general purpose controller */
        GPC2_MSB = 17, //0x11,
        GPC3_MSB = 18, //0x12,
        GPC4_MSB = 19, // 0x13,

        /// <summary>@brief Bank Select LSB.\n
        /// MPTK bank style is FLUID_BANK_STYLE_GS (see fluidsynth), bank = CC0/MSB (CC32/LSB ignored)
        /// </summary>
        BankSelectLsb = 32, // 0x20

        MODULATION_WHEEL_LSB = 33, // 0x21,
        BREATH_LSB = 34, // 0x22,
        FOOT_LSB = 36, // 0x24,
        PORTAMENTO_TIME_LSB = 37, // 0x25,


        DATA_ENTRY_LSB = 38, // 0x26,

        VOLUME_LSB = 39, // 0x27,

        BALANCE_LSB = 40, // 0x28,

        PAN_LSB = 42, //0x2A,

        EXPRESSION_LSB = 43, //0x2B,

        EFFECTS1_LSB = 44, //0x2C,
        EFFECTS2_LSB = 45, // 0x2D,
        GPC1_LSB = 48, // 0x30,
        GPC2_LSB = 49, // 0x31,
        GPC3_LSB = 50, // 0x32,
        GPC4_LSB = 51, // 0x33,

        /// <summary>@brief Sustain</summary>
        Sustain = 64, // 0x40

        /// <summary>@brief Portamento On/Off - not yet imlemented </summary>
        Portamento = 65, // 0x41

        /// <summary>@brief Sostenuto On/Off - not yet imlemented </summary>
        Sostenuto = 66, // 0x42

        /// <summary>@brief Soft Pedal On/Off - not yet imlemented </summary>
        SoftPedal = 67, // 0x43

        /// <summary>@brief Legato Footswitch - not yet imlemented </summary>
        LegatoFootswitch = 68, // 0x44

        HOLD2_SWITCH = 69, // 0x45,

        SOUND_CTRL1 = 70, // 0x46,
        SOUND_CTRL2 = 71, // 0x47,
        SOUND_CTRL3 = 72, // 0x48,
        SOUND_CTRL4 = 73, // 0x49,
        SOUND_CTRL5 = 74, // 0x4A,
        SOUND_CTRL6 = 75, // 0x4B,
        SOUND_CTRL7 = 76, // 0x4C,
        SOUND_CTRL8 = 77, // 0x4D,
        SOUND_CTRL9 = 78, // 0x4E,
        SOUND_CTRL10 = 79, // 0x4F,

        GPC5 = 80, // 0x50,
        GPC6 = 81, // 0x51,
        GPC7 = 82, // 0x52,
        GPC8 = 83, // 0x53,

        PORTAMENTO_CTRL = 84, // 0x54, 

        EFFECTS_DEPTH1 = 91, // 0x5B,
        EFFECTS_DEPTH2 = 92, // 0x5C,
        EFFECTS_DEPTH3 = 93, // 0x5D,
        EFFECTS_DEPTH4 = 94, // 0x5E,
        EFFECTS_DEPTH5 = 95, // 0x5F,

        DATA_ENTRY_INCR = 96, // 0x60,
        DATA_ENTRY_DECR = 97, // 0x61,

        /// <summary>@brief
        /// Non Registered Parameter Number LSB\n
        /// http://www.philrees.co.uk/nrpnq.htm
        /// </summary>
        NRPN_LSB = 98, // 0x62,

        /// <summary>@brief
        /// Non Registered Parameter Number MSB\n
        /// http://www.philrees.co.uk/nrpnq.htm
        /// </summary>
        NRPN_MSB = 99, // 0x63,

        /// <summary>@brief
        /// Registered Parameter Number LSB\n
        /// http://www.philrees.co.uk/nrpnq.htm
        /// </summary>
        RPN_LSB = 100, // 0x64,

        /// <summary>@brief
        /// Registered Parameter Number MSB\n
        /// http://www.philrees.co.uk/nrpnq.htm
        /// </summary>
        RPN_MSB = 101, // 0x65,

        /// <summary@brief >All sound off (ALL_SOUND_OFF)</summary>
        AllSoundOff = 120, // 0x78,

        /// <summary>@brief Reset all controllers (ALL_CTRL_OFF)</summary>
        ResetAllControllers = 121, // 0x79

        LOCAL_CONTROL = 122, // 0x7A,

        /// <summary>@brief All notes off (ALL_NOTES_OFF)</summary>
        AllNotesOff = 123, // 0x7B

        OMNI_OFF = 124, // 0x7C,
        OMNI_ON = 125, // 0x7D,
        POLY_OFF = 126, // 0x7E,
        POLY_ON = 127, // 0x7F
    }


    /// <summary>@brief
    /// General MIDI RPN event numbers (LSB, MSB = 0)
    /// The only confusing part of using parameter numbers, initially, is that there are two parts to using them.\n
    /// First you need to tell the synthesizer what parameter you want to change, then you need to tell it how to change the parameter. \n
    /// For example, if you want to change the "pitch bend sensitivity" to 12 semitones, you would send the following controler midi message:\n
    ///     - MPTKEvent#Controller=RPN_MSB (101) MPTKEvent#Value=0
    ///     - MPTKEvent#Controller=RPN_LSB (100) MPTKEvent#Value=midi_rpn_event.RPN_PITCH_BEND_RANGE
    ///     - MPTKEvent#Controller=DATA_ENTRY_MSB (6) MPTKEvent#Value=12
    ///     - MPTKEvent#Controller=DATA_ENTRY_LSB (38) MPTKEvent#Value=0
    /// https://www.2writers.com/Eddie/TutNrpn.htm
    /// </summary>
    public enum midi_rpn_event
    {
        /// <summary>@brief
        /// Change pitch bend sensitivity
        /// </summary>
        RPN_PITCH_BEND_RANGE = 0x00,

        RPN_CHANNEL_FINE_TUNE = 0x01,
        RPN_CHANNEL_COARSE_TUNE = 0x02,
        RPN_TUNING_PROGRAM_CHANGE = 0x03,
        RPN_TUNING_BANK_SELECT = 0x04,
        RPN_MODULATION_DEPTH_RANGE = 0x05
    }

    /// <summary>@brief
    /// MIDI MetaEvent Type. Meta events are optionnals information that could be defined in a MIDI. None are mandatory\n
    /// In MPTKEvent the attibute MPTKEvent.Meta defined the type of meta event. 
    /// </summary>
    public enum MPTKMeta : byte
    {
        /// <summary>@brief Track sequence number</summary>
        TrackSequenceNumber = 0x00,
        /// <summary>@brief Text event</summary>
        TextEvent = 0x01,
        /// <summary>@brief Copyright</summary>
        Copyright = 0x02,
        /// <summary>@brief Sequence track name</summary>
        SequenceTrackName = 0x03,
        /// <summary>@brief Track instrument name</summary>
        TrackInstrumentName = 0x04,
        /// <summary>@brief Lyric</summary>
        Lyric = 0x05,
        /// <summary>@brief Marker</summary>
        Marker = 0x06,
        /// <summary>@brief Cue point</summary>
        CuePoint = 0x07,
        /// <summary>@brief Program (patch) name</summary>
        ProgramName = 0x08,
        /// <summary>@brief Device (port) name</summary>
        DeviceName = 0x09,
        /// <summary>@brief MIDI Channel (not official?)</summary>
        MidiChannel = 0x20,

        /// <summary>@brief MIDI Port (not official?)</summary>
        MidiPort = 0x21,

        /// <summary>@brief End track</summary>
        EndTrack = 0x2F,

        /// <summary>@brief Set tempo
        /// MPTKEvent.Value contains new Microseconds Per Beat Note.
        /// @deprecated MPTKEvent.Value content with version 2.10.0 - Now MPTKEvent.Duration no longer contains tempo in quarter per minute. Please investigate MPTKEvent.QuarterPerMicroSecond2BeatPerMinute()
        /// </summary>
        SetTempo = 0x51,

        /// <summary>@brief MPTE offset</summary>
        SmpteOffset = 0x54,

        /// <summary>@brief Time signature
        /// MPTKEvent.Value contains four bytes. From less significant to most significant. Please investigate MPTKEvent.ExtractFromInt():
        ///    -# Numerator (number of beats in a bar), 
        ///    -# Denominator (Beat unit: 1 means 2, 2 means 4 (crochet), 3 means 8 (quaver), 4 means 16, ...)
        ///    -# TicksInMetronomeClick, generally 24 (number of 1/32nds of a note happen by MIDI quarter note)
        ///    -# No32ndNotesInQuarterNote, generally 8 (standard MIDI clock ticks every 24 times every quarter note)
        /// @deprecated MPTKEvent.Value content with version 2.10.0 - Now MPTKEvent.Value no longer contains the numerator and MPTKEvent.Duration the Denominator (all values are merged in MPTKEvent::Value 
        /// </summary>
        TimeSignature = 0x58,

        /// <summary>@brief Key signature
        /// MPTKEvent.Value contains two bytes. From less significant to most significant. Please investigate MPTKEvent.ExtractFromInt().
        ///     -# SharpsFlats (number of sharp) 
        ///     -# MajorMinor flag (0 the scale is major, 1 the scale is minor).
        /// @deprecated MPTKEvent.Value content with version 2.10.0 - MPTKEvent.Duration no longer contains the MajorMinor.
        /// </summary>
        KeySignature = 0x59,

        /// <summary>@brief Sequencer specific</summary>
        SequencerSpecific = 0x7F,
    }

    [System.Serializable]
    public enum EventEndMidiEnum
    {
        MidiEnd,
        ApiStop,
        Replay,
        Next,
        Previous,
        MidiErr,
        Loop
    }

    /// <summary>@brief
    /// Status of the last midi file loaded
    /// @li      -1: midi file is loading
    /// @li       0: succes, midi file loaded 
    /// @li       1: error, no Midi found
    /// @li       2: error, not a midi file, too short size
    /// @li       3: error, not a midi file, signature MThd not found.
    /// @li       4: error, network error or site not found.
    /// </summary>
    [System.Serializable]
    public enum LoadingStatusMidiEnum
    {
        /// <summary>@brief
        /// -1: midi file is loading.
        /// </summary>
        NotYetDefined = -1,

        /// <summary>@brief
        /// 0: succes, midi file loaded.
        /// </summary>
        Success = 0,

        /// <summary>@brief
        /// 1: error, no Midi file found.
        /// </summary>
        NotFound = 1,

        /// <summary>@brief
        /// 2: error, not a midi file, too short size.
        /// </summary>
        TooShortSize = 2,

        /// <summary>@brief
        /// 3: error, not a midi file, signature MThd not found.
        /// </summary>
        NoMThdSignature = 3,

        /// <summary>@brief
        /// 4: error, network error or site not found.
        /// </summary>
        NetworkError = 4,

        /// <summary>@brief
        /// 5: error, midi file corrupted, error detected when loading the midi events.
        /// </summary>
        MidiFileInvalid = 5,

        /// <summary>@brief
        /// 6: SoundFont not loaded.
        /// </summary>
        SoundFontNotLoaded = 6,

        /// <summary>@brief
        /// 7: error, Already playing.
        /// </summary>
        AlreadyPlaying = 7,

        /// <summary>@brief
        /// 8: error, MPTK_MidiName must start with file:// or http:// or https:// (only for MidiExternalPlayer).
        /// </summary>
        MidiNameInvalid = 8,

        /// <summary>@brief
        /// 9: error,  Set MPTK_MidiName by script or in the inspector with Midi Url/path before playing.
        /// </summary>
        MidiNameNotDefined = 9,

        /// <summary>@brief
        /// 10: error, Read 0 byte from the MIDI file.
        /// </summary>
        MidiFileEmpty = 10,
    }

    /// <summary>@brief
    /// Status of the last midi file loaded
    /// @li      -1: midi file is loading
    /// @li       0: succes, midi file loaded 
    /// @li       1: error, no Midi found
    /// @li       2: error, not a midi file, too short size
    /// @li       3: error, not a midi file, signature MThd not found.
    /// @li       4: error, network error or site not found.
    /// </summary>
    [System.Serializable]
    public enum LoadingStatusSoundFontEnum
    {
        /// <summary>@brief
        /// -1: SoundFont is loading.
        /// </summary>
        InProgress = -1,

        /// <summary>@brief
        /// 0: succes, SoundFont loaded.
        /// </summary>
        Success = 0,

        /// <summary>@brief
        /// 1: error, no SoundFont found.
        /// </summary>
        //NotFound = 1,

        /// <summary>@brief
        /// 2: error, not a SoundFont, too short size.
        /// </summary>
        //TooShortSize = 2,

        /// <summary>@brief
        /// 3: error, not a SoundFont, signature RIFF not found.
        /// </summary>
        NoRIFFSignature = 3,

        /// <summary>@brief
        /// 4: error, network error or site not found.
        /// </summary>
        NetworkError = 4,

        /// <summary>@brief
        /// 5: error, SoundFont corrupted, error detected when loading the SoundFont.
        /// </summary>
        //MidiFileInvalid = 5,

        /// <summary>@brief
        /// 6: SoundFont not loaded.
        /// </summary>
        SoundFontNotLoaded = 6,

        /// <summary>@brief
        /// 7: error, Already playing.
        /// </summary>
        ///AlreadyPlaying = 7,

        /// <summary>@brief
        /// 8: error, URL must start with file:// or http:// or https://
        /// </summary>
        InvalidURL = 8,

        ///// <summary>@brief
        ///// 9: error,  Set MPTK_MidiName by script or in the inspector with Midi Url/path before playing.
        ///// </summary>
        //MidiNameNotDefined = 9,

        ///// <summary>@brief
        ///// 10: error, Read 0 byte from the SoundFont file.
        ///// </summary>
        SoundFontEmpty = 10,
    }
}
