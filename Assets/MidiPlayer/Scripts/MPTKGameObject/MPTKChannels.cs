using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace MidiPlayerTK
{
    // specific channel properties - internal class
    // removed 2.10.1 - included in public MptkChannel (class name was fluid_channel);
    //public class mptk_channel // V2.82 new
    //{
    //    public bool enabled; // V2.82 move from MptkChannel 
    //    public float volume; // volume for the channel, between 0 and 1

    //    public int forcedPreset; // forced preset for this channel
    //    public int forcedBank; // forced bank for this channel
    //    public int lastPreset; // last preset for this channel
    //    public int lastBank; // last bank for this channel

    //    public int countChannel; // countChannel of note-on for the channel
    //    //private int channum;
    //    //private MidiSynth synth;
    //    public mptk_channel(MidiSynth psynth, int pchanum)
    //    {
    //        //synth = psynth;
    //        //channum = pchanum;
    //        enabled = true;
    //        volume = 1f;
    //        countChannel = 0;
    //        forcedPreset = -1;
    //        forcedBank = -1;
    //    }
    //}

    /// <summary>
    /// Description and list of MIDI Channels associated to the MIDI synth.\n
    /// Each MIDI Synth is equipped with 16 channels which carry all the MIDI information pertinent.
    /// They serve to distinguish between instruments and provide independent control over each one. \n
    /// By transmitting MIDI messages on their respective channels, you can alter the instrument, volume, pitch, and other parameters. \n
    /// Within the Maestro Midi Player Toolkit, MIDI channels are designated numerically from 0 to 15. Notably, channel 9 is set aside specifically for drum sounds.
    /// @snippet TestMidiFilePlayerScripting.cs ExampleUsingChannelAPI_Full
    /// </summary>
    public class MPTKChannels : IEnumerable<MPTKChannel>
    {

        private List<MPTKChannel> Channels { get; set; }

        /// <summary>@brief 
        /// Channel count. Classically 16 when MIDI is read from a MIDI file.\n
        /// Can be extended but not compliant with MIDI file, only for internal use (experimental)
        /// </summary>
        public int Length { get { return Channels.Count; } }

        /// <summary>@brief 
        /// Enable or disable all channels for playing
        /// @snippet TestMidiFilePlayerScripting.cs ExampleUsingChannelAPI_6
        /// </summary>
        public bool EnableAll
        {
            set
            {
                foreach (var c in Channels)
                    c.Enable = value;
            }
        }

        /// <summary>@brief 
        /// Set the volume for all channels as a percentage between 0 and 1. 
        /// </summary>
        public float VolumeAll
        {
            set
            {
                foreach (var c in Channels)
                    c.Volume = value;
            }
        }



        /// <summary>@brief 
        /// Allows access to all channels in the MIDI Synth.
        /// Within the Maestro Midi Player Toolkit, MIDI channels are designated numerically from 0 to 15. Notably, channel 9 is set aside specifically for drum sounds.
        /// @snippet TestMidiFilePlayerScripting.cs ExampleUsingChannelAPI_1
        /// </summary>
        /// <param name="channel">Channel number between 0 and 15 included</param>
        /// <returns></returns>
        public MPTKChannel this[int channel]
        {
            get
            {
                try
                {
                    return Channels[channel];
                }
                catch (Exception)
                {
                    Debug.LogError($"Error when trying access to MPTK_Channels, channel {channel}");
                    //if (Channels == null)
                    //    Debug.LogException(ex);
                }
                return null;
            }
            set
            {
                try
                {
                    Channels[channel] = value;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error when trying access to MPTK_Channels, channel {channel}");
                    if (Channels == null)
                        Debug.LogException(ex);
                }
            }
        }


        public MPTKChannels(MidiSynth psynth, int countChannel = 16)
        {
            if (psynth.VerboseChannel) Debug.Log("Create channels");
            Channels = new List<MPTKChannel>();
            for (int i = 0; i < countChannel; i++)
                Channels.Add(new MPTKChannel(i, psynth));
        }

        [Preserve]
        public MPTKChannels()
        {
        }

        /// <summary>@brief 
        /// Enable to reset all channels when MIDI start playing (if true, Channels member is allocated at the synth reinit). Default is true.\n
        /// Weird behaviors could occurs if set to false with MidiFilePlayer but could be useful for MidiStreamPlayer.
        /// @version 2.10.1
        /// </summary>
        public bool EnableResetChannel = true;


        /// <summary>@brief 
        /// Reset Maestro channels extension information. 
        ///     - ForcedBank is disable
        ///     - ForcedPreset is disable
        ///     - Enable is set to true
        ///     - Volume is set to max (1)
        /// Other information like current preset, bank, controller are not reset.
        /// @version 2.10.1
        /// </summary>
        /// <param name="channelNum">select the channel number to reset, by default all</param>
        public void ResetExtension(int channelNum = -1)
        {
            //if (synth != null && synth.VerboseChannel) Debug.Log("Reset channel extension feature");

            if (channelNum < 0)
                foreach (MPTKChannel channel in Channels)
                {
                    channel.ForcedBank = -1;
                    channel.ForcedPreset = -1;
                    channel.LastBank = 0;
                    channel.LastPreset = 0;
                    channel.Enable = true;
                    channel.Volume = 1;
                }
            else if (channelNum < Channels.Count)
            {
                Channels[channelNum].ForcedBank = -1;
                Channels[channelNum].ForcedPreset = -1;
                Channels[channelNum].LastBank = 0;
                Channels[channelNum].LastPreset = 0;
                Channels[channelNum].Enable = true;
                Channels[channelNum].Volume = 1;
            }
            else
                Debug.LogWarning($"MPTK_ResetChannels: channel number is incorrect, must be between 0 and {Channels.Count}");
        }

        public IEnumerator<MPTKChannel> GetEnumerator()
        {
            return Channels.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Channels.GetEnumerator();
        }
    }

    /// <summary>
    /// Description of a MIDI Channel associated to the MIDI synth.\n
    /// Each MIDI Synth is equipped with 16 channels which carry all the MIDI information pertinent.
    /// They serve to distinguish between instruments and provide independent control over each one. \n
    /// By transmitting MIDI messages on their respective channels, you can alter the instrument, volume, pitch, and other parameters. \n
    /// Within the Maestro Midi Player Toolkit, MIDI channels are designated numerically from 0 to 15. Notably, channel 9 is set aside specifically for drum sounds.
    /// @snippet TestMidiFilePlayerScripting.cs ExampleUsingChannelAPI_One
    /// </summary>
    public class MPTKChannel
    {

        /* Field shift amounts for sfont_bank_prog bit field integer */
        //const int PROG_SHIFTVAL = 0;
        //const int BANK_SHIFTVAL = 8;
        //const int SFONT_SHIFTVAL = 22;

        //const int PROG_MASKVAL = 0x000000FF;  /* Bit 7 is used to indicate unset state */
        //const int BANK_MASKVAL = 0x003FFF00;
        //const int BANKLSB_MASKVAL = 0x00007F00;
        //const int BANKMSB_MASKVAL = 0x003F8000;
        //const uint SFONT_MASKVAL = 0xFFC00000;

        //public enum enumStyleBanq
        //{
        //    FLUID_BANK_STYLE_GM,  /**< GM style, bank = 0 always (CC0/MSB and CC32/LSB ignored) */
        //    FLUID_BANK_STYLE_GS, /**< GS style, bank = CC0/MSB (CC32/LSB ignored) */
        //    FLUID_BANK_STYLE_XG,  /**< XG style, bank = CC32/LSB (CC0/MSB ignored) */
        //    FLUID_BANK_STYLE_MMA, /**< MMA style bank = 128*MSB+LSB */
        //}
        //public enumStyleBanq StyleBanq = enumStyleBanq.FLUID_BANK_STYLE_GM;

        /// <summary>@brief
        /// Last preset used for this channel, useful when a forced preset has been set.
        /// </summary>
        public int LastPreset;

        /// <summary>@brief
        /// Last bank used for this channel, useful when a forced preset has been set.
        /// </summary>
        public int LastBank;

        /// <summary>@brief
        /// Channel number between 0 and 15, channel 9 is set aside specifically for drum sounds.
        /// </summary>
        public int Channel { get { return channum; } }
        private int channum;

        private HiPreset hiPreset;
        public HiPreset HiPreset { get { return hiPreset; } }

        public short key_pressure;
        public short channel_pressure;
        public short pitch_bend;
        public short pitch_wheel_sensitivity;

        // NRPN system 
        //int nrpn_select;
        // cached values of last MSB values of MSB/LSB controllers
        //byte bank_msb;

        // Maestro specifique
        private int forcedPreset; // forced preset for this channel
        private int forcedBank; // forced bank for this channel

        private int count; // countChannel of note-on for the channel
        private int banknum;
        // controller values
        private short[] cc;
        private MidiSynth synth;

        // the micro-tuning TO BE DONE ... one day
        //private fluid_tuning tuning;

        /* The values of the generators, set by NRPN messages, or by
         * fluid_synth_set_gen(), are cached in the channel so they can be
         * applied to future notes. They are copied to a voice's generators
         * in fluid_voice_init(), wihich calls fluid_gen_init().  */
        private double[] gens;

        /* By default, the NRPN values are relative to the values of the
         * generators set in the SoundFont. For example, if the NRPN
         * specifies an attack of 100 msec then 100 msec will be added to the
         * combined attack time of the sound font and the modulators.
         *
         * However, it is useful to be able to specify the generator value
         * absolutely, completely ignoring the generators of the sound font
         * and the values of modulators. The gen_abs field, is a boolean
         * flag indicating whether the NRPN value is absolute or not.
         */
        private bool[] gen_abs;

        public MPTKChannel(int channel, MidiSynth psynth)
        {
            channum = channel;
            synth = psynth;
            Enable = true;
            Volume = 1f;
            count = 0;
            forcedPreset = -1;
            forcedBank = -1;
            gens = new double[Enum.GetNames(typeof(fluid_gen_type)).Length];
            gen_abs = new bool[Enum.GetNames(typeof(fluid_gen_type)).Length];
            cc = new short[128];
            hiPreset = null;
            //tuning = null;
            fluid_channel_init();
            fluid_channel_init_ctrl();

        }
        /// <summary>@brief 
        /// Build an information string about the channel. It's also a good pretext to display an example of Channel API. \n
        /// Exemple of return 
        /// @li Channel:2	Enabled	[Preset:18, Bank:0]		'Rock Organ'			Count:1		Volume:1
        /// @li Channel:4	Muted	[Preset:F44, Bank:0]	'Stereo Strings Trem'	Count:33	Volume:0,50
        /// </summary>
        /// <param name="channel">index channel</param>
        /// <returns>Information string</returns>
        public override string ToString()
        {
            string info = "Channel:";

            string sPreset = "";
            if (ForcedPreset == -1)
            {
                // Preset not forced, get the preset defined on this channel by the Midi
                sPreset = PresetNum.ToString();
            }
            else
            {
                sPreset = $"F{ForcedPreset}";
            }

            string sMuted = Enable ? "Enabled" : "Muted";

            info += $"{Channel}\t";
            info += $"{sMuted}\t";
            info += $"[Preset:{sPreset}, ";
            info += $"Bank:{BankNum}]\t";
            info += $"'{PresetName}'\t";
            info += $"Count:{NoteCount}\t";
            info += $"Volume:{Volume:F2}\t";

            return info;
        }

        /// <summary>@brief 
        /// Properties to enable (unmute) or disable (mute) a channel or get status.
        /// All channels are unmuted when MIDI start playing (MidiFilePlayer#MPTK_Play).\n
        /// By the way, to mute channels just before the playing, use the MidiFilePlayer#OnEventStartPlayMidi.
        /// Look to the demo: Assets\MidiPlayer\Demo\FreeMVP\MidiLoop.cs
        /// @snippet MidiLoop.cs ExampleFindPlayerAndAddListener
        /// @snippet MidiLoop.cs ExampleChannelEnabled
        /// </summary>
        /// <returns>true if channel is enabled</returns>
        /// @snippet TestMidiFilePlayerScripting.cs ExampleUsingChannelAPI_7
        public bool Enable { get; set; }


        /// <summary>@brief 
        /// Get the count of notes (NoteOn) played since the start of the MIDI.
        /// </summary>
        public int NoteCount { get { return count; } set { count = value; } }

        /// <summary>@brief 
        /// Get or Set the volume for a channel as a percentage between 0 and 1. 
        /// </summary>
        /// @snippet TestMidiFilePlayerScripting.cs ExampleUsingChannelAPI_5
        public float Volume { get; set; }

        private int prognum;

        /// <summary>@brief 
        /// Get channel current preset number.
        /// </summary>
        /// @snippet TestMidiStream.cs ExampleUsingChannelAPI_4
        public int PresetNum
        {
            get
            {
                return prognum;
            }
            set
            {
                ImSoundFont sfont = MidiPlayerGlobal.ImSFCurrent;
                if (sfont == null)
                {
                    Debug.LogWarning($"MPTK_Channel[{Channel}].PresetNum - no SoundFont defined");
                }
                else if (value < 0 || value > 127)
                {
                    Debug.LogWarning($"MPTK_Channel[{Channel}].PresetNum out of range, must be between 0 and 127, found {value}");
                }
                else
                {
                    LastPreset = value;
                    fluid_synth_program_change(value);
                }
            }
        }

        /// <summary>@brief
        /// Get the current bank number associated to the channel.\n
        /// Each MIDI channel can play a different preset and bank.\n
        /// </summary>
        /// @snippet TestMidiStream.cs ExampleUsingChannelAPI_4
        public int BankNum
        {
            get
            {
                return banknum;
            }
            set
            {
                if (value < 0 || value > 16383)
                {
                    Debug.LogWarning($"MPTK_Channel[{Channel}].BankNum out of range, must be between 0 and 16383, found {value}");
                }
                banknum = value;
            }
        }

        /// <summary>@brief 
        /// Get the current preset name for the channel.\n
        /// Each MIDI channel can play a different preset.\n
        /// </summary>
        /// <param name="channel">MIDI channel must be between 0 and 15</param>
        /// <returns>channel preset name or "" if channel error</returns>
        /// @snippet TestMidiStream.cs ExampleUsingChannelAPI_3
        public string PresetName { get { return hiPreset != null ? hiPreset.Name : "no preset defined"; } }



        /// <summary>@brief 
        /// Set forced preset on the channel. MIDI will allways playing with this preset even if a MIDI Preset Change message is received.\n
        /// Set to -1 to disable this behavior.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>preset index, -1 if not set</returns>
        /// @snippet TestMidiFilePlayerScripting.cs ExampleUsingChannelAPI_2
        /// 
        public int ForcedPreset
        {
            get
            {
                return forcedPreset;
            }
            set
            {
                if (value >= 0)
                {
                    if (synth.VerboseChannel) Debug.Log($"ForcedPreset Channel:{Channel} Preset forced to:{value}");
                    forcedPreset = value;
                    fluid_synth_program_change(value);
                }
                else
                {
                    // If < 0  disable forced preset
                    forcedPreset = -1;
                    if (synth.VerboseChannel) Debug.Log($"ForcedPreset Channel:{Channel} Restore preset to:{LastPreset}");
                    fluid_synth_program_change(LastPreset);
                }
            }
        }

        /// <summary>@brief 
        /// Set forced bank on the channel from 0 to 65635. MIDI will allways using this bank even if a bank change message is received.\n
        /// Set to -1 to disable this behavior.
        /// </summary>
        /// <returns>preset index, -1 if not set</returns>
        /// @snippet TestMidiFilePlayerScripting.cs ExampleUsingChannelAPI_2
        /// 
        public int ForcedBank
        {
            get
            {
                return forcedBank;
            }
            set
            {
                if (value >= 0)
                {
                    if (synth.VerboseChannel) Debug.Log($"ForcedBank Channel:{Channel} Force bank to:{value}");
                    forcedBank = value; // set to -1 to disable forced bank
                }
                else
                {
                    if (synth.VerboseChannel) Debug.Log($"ForcedBank Channel:{Channel} Restore bank to:{LastBank}");
                    banknum = LastBank;
                    forcedBank = -1;
                }
            }
        }

        private void fluid_channel_init()
        {
            prognum = 0;
            if (MidiPlayerGlobal.ImSFCurrent != null)
            {
                banknum = Channel == 9 ? MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber : MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber;
                hiPreset = fluid_synth_find_preset(banknum, prognum);
            }
        }

        private void fluid_channel_init_ctrl()
        {
            /*
                @param is_all_ctrl_off if nonzero, only resets some controllers, according to
                https://www.midi.org/techspecs/rp15.php
                For MPTK: is_all_ctrl_off=0, all controllers will be reset
            */

            key_pressure = 0;
            channel_pressure = 0;
            pitch_bend = 0x2000; // Range is 0x4000, pitch bend wheel starts in centered position
            pitch_wheel_sensitivity = 2; // two semi-tones 

            for (int i = 0; i < gens.Length; i++)
            {
                gens[i] = 0.0f;
                gen_abs[i] = false;
            }

            for (int i = 0; i < 128; i++)
            {
                cc[i] = 0;
            }

            //fluid_channel_clear_portamento(chan); /* Clear PTC receive */
            //chan->previous_cc_breath = 0;         /* Reset previous breath */
            /* Reset polyphonic key pressure on all voices */
            //for (i = 0; i < 128; i++)
            //{
            //    fluid_channel_set_key_pressure(chan, i, 0);
            //}

            /* Set RPN controllers to NULL state */
            cc[(int)MPTKController.RPN_LSB] = 127;
            cc[(int)MPTKController.RPN_MSB] = 127;

            /* Set NRPN controllers to NULL state */
            cc[(int)MPTKController.NRPN_LSB] = 127;
            cc[(int)MPTKController.NRPN_MSB] = 127;

            /* Expression (MSB & LSB) */
            cc[(int)MPTKController.Expression] = 127;
            cc[(int)MPTKController.EXPRESSION_LSB] = 127;

            /* Just like panning, a value of 64 indicates no change for sound ctrls */
            for (int i = (int)MPTKController.SOUND_CTRL1; i <= (int)MPTKController.SOUND_CTRL10; i++)
            {
                cc[i] = 64;
            }

            // Volume / initial attenuation (MSB & LSB) 
            cc[(int)MPTKController.VOLUME_MSB] = 100; // V2.88.2 before was 127
            cc[(int)MPTKController.VOLUME_LSB] = 0;

            // Pan (MSB & LSB) 
            cc[(int)MPTKController.Pan] = 64;
            cc[(int)MPTKController.PAN_LSB] = 0;

            cc[(int)MPTKController.BALANCE_MSB] = 64;
            cc[(int)MPTKController.BALANCE_LSB] = 0;

            /* Reverb */
            /* fluid_channel_set_cc (chan, EFFECTS_DEPTH1, 40); */
            /* Note: although XG standard specifies the default amount of reverb to
               be 40, most people preferred having it at zero.
               See https://lists.gnu.org/archive/html/fluid-dev/2009-07/msg00016.html */
        }

        /* was  fluid_channel_cc */
        /// <summary>@brief
        /// Read or write the value of the controller.\n
        /// <returns>controller value if read, previous value if write, -1 if error</returns>
        /// @snippet TestMidiStream.cs ExampleAccessToControler
        /// </summary>
        /// <param name="numController">Controller to read or write</param>
        /// <param name="valueController">Value to set, default is -1 for only reading the controller value (no write)</param>
        public int Controller(MPTKController numController, int valueController = -1)
        {
            if ((int)numController < 0 || (int)numController > 127)
            {
                Debug.LogWarning($"MPTK_Channel[{Channel}].Controller out of range, must be between 0 and 127, found {numController} {(int)numController}");
                return -1;
            }
            if (valueController < 0)
            {
                return cc[(int)numController];
            }
            else
            {
                short previousValue = cc[(int)numController];
                cc[(int)numController] = (short)valueController;

                if (synth.VerboseController)
                    Debug.Log($"MPTK_Channel[{Channel}].Controller Control:{numController} Value:{valueController} Previous:{previousValue}");

                switch (numController)
                {
                    case MPTKController.Sustain:
                        {
                            if (valueController < 64)
                            {
                                /*  	printf("** sustain off\n"); */
                                synth.fluid_synth_damp_voices(Channel);
                            }
                            else
                            {
                                /*  	printf("** sustain on\n"); */
                            }
                        }
                        break;

                    case MPTKController.BankSelectMsb:
                        banknum = valueController & 0x7F;
                        LastBank = banknum;
                        //synth.fluid_synth_program_change(channum, prognum);
                        break;

                    case MPTKController.BankSelectLsb:
                        banknum = banknum * 128 + (valueController & 0x7F);
                        LastBank = banknum;
                        break;

                    case MPTKController.AllNotesOff:
                        synth.fluid_synth_noteoff(Channel, -1);
                        break;

                    case MPTKController.AllSoundOff:
                        synth.fluid_synth_soundoff(Channel);
                        break;

                    case MPTKController.ResetAllControllers:
                        fluid_channel_init_ctrl();
                        synth.fluid_synth_modulate_voices_all(Channel);
                        break;

                    case MPTKController.DATA_ENTRY_LSB: /* not allowed to modulate (spec SF 2.01 - 8.2.1) */
                        break;

                    case MPTKController.DATA_ENTRY_MSB: /* not allowed to modulate (spec SF 2.01 - 8.2.1) */
                        {
                            //int data = (valueController << 7) + cc[(int)MPTKController.DATA_ENTRY_LSB] ;

                            //if (chan->nrpn_active)   /* NRPN is active? */
                            //{
                            //    /* SontFont 2.01 NRPN Message (Sect. 9.6, p. 74)  */
                            //    if ((fluid_channel_get_cc(chan, NRPN_MSB) == 120)
                            //            && (fluid_channel_get_cc(chan, NRPN_LSB) < 100))
                            //    {
                            //        nrpn_select = chan->nrpn_select;

                            //        if (nrpn_select < GEN_LAST)
                            //        {
                            //            float val = fluid_gen_scale_nrpn(nrpn_select, data);
                            //            fluid_synth_set_gen_LOCAL(synth, channum, nrpn_select, val);
                            //        }

                            //        chan->nrpn_select = 0;  /* Reset to 0 */
                            //    }
                            //}
                            //else 
                            // if (fluid_channel_get_cc(chan, RPN_MSB) == 0)      /* RPN is active: MSB = 0? */
                            {
                                switch ((midi_rpn_event)cc[(int)MPTKController.RPN_LSB])
                                {
                                    case midi_rpn_event.RPN_PITCH_BEND_RANGE:    /* Set bend range in semitones */
                                        //fluid_channel_set_pitch_wheel_sensitivity(synth->channel[channum], value);
                                        pitch_wheel_sensitivity = (short)valueController;

                                        /* Update bend range */
                                        /* fluid_synth_update_pitch_wheel_sens_LOCAL(synth, channum);    
                                               fluid_synth_modulate_voices_LOCAL(synth, chan, 0, FLUID_MOD_PITCHWHEELSENS);
                                                    fluid_voice_t* voice;
                                                    int i;

                                                    for (i = 0; i < synth->polyphony; i++)
                                                    {
                                                        voice = synth->voice[i];

                                                        if (fluid_voice_get_channel(voice) == chan)
                                                        {
                                                            fluid_voice_modulate(voice, is_cc, ctrl);
                                                        }
                                                    }
                                        */
                                        break;

                                        //case RPN_CHANNEL_FINE_TUNE:   /* Fine tune is 14 bit over +/-1 semitone (+/- 100 cents, 8192 = center) */
                                        //    fluid_synth_set_gen_LOCAL(synth, channum, GEN_FINETUNE,
                                        //                              (float)(data - 8192) * (100.0f / 8192.0f));
                                        //    break;

                                        //case RPN_CHANNEL_COARSE_TUNE: /* Coarse tune is 7 bit and in semitones (64 is center) */
                                        //    fluid_synth_set_gen_LOCAL(synth, channum, GEN_COARSETUNE,
                                        //                              value - 64);
                                        //    break;

                                        //case RPN_TUNING_PROGRAM_CHANGE:
                                        //    fluid_channel_set_tuning_prog(chan, value);
                                        //    fluid_synth_activate_tuning(synth, channum,
                                        //                                fluid_channel_get_tuning_bank(chan),
                                        //                                value, TRUE);
                                        //    break;

                                        //case RPN_TUNING_BANK_SELECT:
                                        //    fluid_channel_set_tuning_bank(chan, value);
                                        //    break;

                                        //case RPN_MODULATION_DEPTH_RANGE:
                                        //    break;
                                }
                            }

                            break;
                        }
                    //case MPTKController.DATA_ENTRY_MSB:
                    //    {
                    //        //int data = (value << 7) + chan->cc[DATA_ENTRY_LSB];

                    ///* SontFont 2.01 NRPN Message (Sect. 9.6, p. 74)  */
                    //if ((chan->cc[NRPN_MSB] == 120) && (chan->cc[NRPN_LSB] < 100))
                    //{
                    //    float val = fluid_gen_scale_nrpn(chan->nrpn_select, data);
                    //    FLUID_LOG(FLUID_WARN, "%s: %d: Data = %d, value = %f", __FILE__, __LINE__, data, val);
                    //    fluid_synth_set_gen(chan->synth, chan->channum, chan->nrpn_select, val);
                    //}
                    //    break;
                    //}

                    //case MPTKController.NRPN_MSB:
                    //    cc[(int)MPTKController.NRPN_LSB] = 0;
                    //    nrpn_select = 0;
                    //    break;

                    //case MPTKController.NRPN_LSB:
                    //    /* SontFont 2.01 NRPN Message (Sect. 9.6, p. 74)  */
                    //    if (cc[(int)MPTKController.NRPN_MSB] == 120)
                    //    {
                    //        if (value == 100)
                    //        {
                    //            nrpn_select += 100;
                    //        }
                    //        else if (value == 101)
                    //        {
                    //            nrpn_select += 1000;
                    //        }
                    //        else if (value == 102)
                    //        {
                    //            nrpn_select += 10000;
                    //        }
                    //        else if (value < 100)
                    //        {
                    //            nrpn_select += value;
                    //            Debug.LogWarning(string.Format("NRPN Select = {0}", nrpn_select));
                    //        }
                    //    }
                    //    break;

                    //case MPTKController.RPN_MSB:
                    //    break;

                    //case MPTKController.RPN_LSB:
                    //    // erase any previously received NRPN message 
                    //    cc[(int)MPTKController.NRPN_MSB] = 0;
                    //    cc[(int)MPTKController.NRPN_LSB] = 0;
                    //    nrpn_select = 0;
                    //    break;

                    default:
                        if (synth.MPTK_ApplyRealTimeModulator)
                            synth.fluid_synth_modulate_voices(Channel, 1, (int)numController);
                        break;
                }
                return previousValue;
            }
        }


        /*
         * fluid_channel_pitch_bend
         */
        // Not recommended to use
        public void fluid_channel_pitch_bend(int val)
        {
            if (synth.VerboseChannel) Debug.LogFormat("PitchChange\tChannel:{0}\tValue:{1}", Channel, val);
            pitch_bend = (short)val;
            synth.fluid_synth_modulate_voices(Channel, 0, (int)fluid_mod_src.FLUID_MOD_PITCHWHEEL); //STRANGE
        }

        // Not recommended to use
        public HiPreset fluid_synth_find_preset(int banknum, int prognum)
        {
            ImSoundFont sfont = MidiPlayerGlobal.ImSFCurrent;

            HiPreset preset_found = CheckBankAndPresetExist(banknum, prognum, sfont);
            if (preset_found != null)
                return preset_found;


            // v2.9.0 try to find the same preset in the first bank
            if (banknum != 0)
            {
                banknum = 0;
                if (banknum >= 0 && banknum < sfont.Banks.Length &&
                   sfont.Banks[banknum] != null &&
                   sfont.Banks[banknum].defpresets != null &&
                   prognum < sfont.Banks[banknum].defpresets.Length &&
                   sfont.Banks[banknum].defpresets[prognum] != null)
                {
                    if (synth.VerboseVoice)
                        Debug.Log($"Select the preset {prognum} in the bank 0.");
                    return sfont.Banks[banknum].defpresets[prognum];
                }
            }

            // Not find, return the first available preset
            foreach (ImBank bank in sfont.Banks)
                if (bank != null)
                    foreach (HiPreset preset in bank.defpresets)
                        if (preset != null)
                        {
                            if (synth.VerboseVoice)
                                Debug.Log($"Select the preset {preset.Num} in the bank 0.");
                            return preset;
                        }
            return null;
        }

        private HiPreset CheckBankAndPresetExist(int banknum, int prognum, ImSoundFont sfont)
        {
            if (sfont == null)
            {
                Debug.LogWarningFormat("Find preset: no soundfont defined");
            }
            else if (banknum >= 0 && banknum < sfont.Banks.Length && sfont.Banks[banknum] != null)
            {
                if (sfont.Banks[banknum].defpresets != null && prognum < sfont.Banks[banknum].defpresets.Length && sfont.Banks[banknum].defpresets[prognum] != null)
                {
                    return sfont.Banks[banknum].defpresets[prognum];
                }
                else
                    Debug.LogWarning($"Preset {prognum} not found in the bank {banknum} of the selected SoundFont.");
            }
            else
                Debug.LogWarning($"Bank {banknum} not found in the selected SoundFont.");
            return null;
        }

        // Not recommended to use
        public void fluid_synth_program_change(int preset)
        {
            //MptkChannel channel;
            //HiPreset hiPreset;
            int banknum;

            if (Channel != 9 || synth.MPTK_EnablePresetDrum == true) // V2.89.0
            {
                if (ForcedPreset >= 0)
                    preset = ForcedPreset;

                banknum = ForcedBank >= 0 ? ForcedBank : BankNum; //fluid_channel_get_banknum

                prognum = preset; // fluid_channel_set_prognum
                BankNum = banknum;

                if (synth.VerboseVoice) Debug.LogFormat("ProgramChange\tChannel:{0}\tBank:{1}\tPreset:{2}", Channel, banknum, preset);
                hiPreset = fluid_synth_find_preset(banknum, preset);

            }
        }
    }
}
