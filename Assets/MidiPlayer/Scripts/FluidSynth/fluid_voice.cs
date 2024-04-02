//#define MPTK_PRO
//#define DEBUGPERF
//#define DEBUGTIME
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MidiPlayerTK
{
    /* for fluid_voice_add_mod */
    public enum fluid_voice_addorover_mod
    {
        FLUID_VOICE_OVERWRITE,
        FLUID_VOICE_ADD,
        FLUID_VOICE_DEFAULT
    }

    public enum fluid_voice_status
    {
        FLUID_VOICE_CLEAN,
        FLUID_VOICE_ON,
        FLUID_VOICE_SUSTAINED,
        FLUID_VOICE_OFF
    }

    public partial class fluid_voice
    {
        /// <summary>@brief
        /// Real time at start of the voice in tick\n
        /// A single tick represents one hundred nanoseconds or one ten-millionth of a second.
        /// There are 10,000 ticks in a millisecond, or 10 million ticks in a second.         
        /// </summary>
        public long TimeAtStart;

        // Set only if VerboseSynth is true
        public long TimeFromStart;
        public long TimeAtEnd;
        public long NewTimeWrite;
        public long LastTimeWrite;

        /// <summary>@brief
        /// Delay in ms between call to fluid_voice_write
        /// </summary>
        public long DeltaTimeWrite;

        /// <summary>@brief
        /// Current Now.Ticks of the write process (from OnAudioFilterRead)
        /// </summary>
        long ticks;

        public MidiSynth synth;
        public MPTKEvent MptkEvent;
        public int IndexActive;
        public long LatenceTick;

        /// <summary>@brief
        /// Legacy mode, mix fluid_voice and a AudioSource
        /// </summary>
        public VoiceAudioSource VoiceAudio;

        //public string SampleName;
        public float StartVolume;
        public bool IsLoop;

        static public int LastId;
        public int IdVoice;
        public int IdSession;

        /// <summary>@brief
        ///  MPTK specific - Note duration in tick. Set to -1 to indefinitely.
        /// A single tick represents one hundred nanoseconds or one ten-millionth of a second.
        /// There are 10,000 ticks in a millisecond, or 10 million ticks in a second.         
        /// </summary>
        public long DurationTick;

        /// <summary>@brief
        /// Duration of the note in millisecond, only for information
        /// </summary>
        public long Duration;

        public const uint Nano100ToMilli = 10000;

        // min vol envelope release (to stop clicks) in SoundFont timecents : ~16ms 
        public const int NO_CHANNEL = 0xff;

        /* used for filter turn off optimization - if filter cutoff is above the
           specified value and filter q is below the other value, turn filter off */
        public const float FLUID_MAX_AUDIBLE_FILTER_FC = 19000.0f;
        public const float FLUID_MIN_AUDIBLE_FILTER_Q = 1.2f;

        /* Smallest amplitude that can be perceived (full scale is +/- 0.5)
         * 16 bits => 96+4=100 dB dynamic range => 0.00001
         * 0.00001 * 2 is approximately 0.00003 :)
         */
        public const float FLUID_NOISE_FLOOR = 0.00003f;

        /* these should be the absolute minimum that FluidSynth can deal with */
        public const int FLUID_MIN_LOOP_SIZE = 2;
        public const int FLUID_MIN_LOOP_PAD = 0;

        /* min vol envelope release (to stop clicks) in SoundFont timecents */
        public const float FLUID_MIN_VOLENVRELEASE = -7200.0f;/* ~16ms */

        public const float M_PI = 3.1415926535897932384626433832795f;


        public fluid_voice_status status;
        public int chan;             /* the channel number, quick access for channel messages */
        public int key;              /* the key, quick acces for noteoff */
        public int vel;              /* the velocity */
        public MPTKChannel midiChannel;
        // removed 2.10.1 public mptk_channel mptkChannel;
        public HiGen[] gens; //[GEN_LAST];
        public List<HiMod> mods; //[FLUID_NUM_MOD];
        public int mod_count;
        public bool has_looped;                 /* Flag that is set as soon as the first loop is completed. */
        public HiSample sample;
        public int check_sample_sanity_flag;   /* Flag that initiates, that sample-related parameters have to be checked. */
        public float output_rate;        /* the sample rate of the synthesizer */

        //public uint start_time;
        public uint FluidTicks; // From fluidsynth (named ticks). Augmented of BUFSIZE at each call of write.

        public float amp;                /* current linear amplitude */
        public ulong /*fluid_phase_t*/ phase;             /* the phase of the sample wave */

        /* Temporary variables used in fluid_voice_write() */

        public float phase_incr;    /* the phase increment for the next 64 samples */
        public float amp_incr;      /* amplitude increment value */
        public float[] dsp_buf;      /* buffer to store interpolated sample data to */

        /* End temporary variables */

        /* basic parameters */
        public float pitch_update;    /* the pitch in midicents */
        public float attenuation;        /* the attenuation in centibels */
        public float min_attenuation_cB; /* Estimate on the smallest possible attenuation during the lifetime of the voice */
        public float root_pitch;

        /* sample and loop start and end points (offset in sample memory).  */
        public int start;
        public int end;
        public int loopstart;
        public int loopend;    /* Note: first point following the loop (superimposed on loopstart) */


        /* master gain */
        //public double synth_gain;

        /// <summary>@brief
        /// volume enveloppe
        /// </summary>
        public fluid_env_data[] volenv_data; //[FLUID_VOICE_ENVLAST];

        /// <summary>@brief
        /// Count time since the start of the section
        /// </summary>
        public long volenv_count;

        /// <summary>@brief
        /// Current section in the enveloppe
        /// </summary>
        public fluid_voice_envelope_index volenv_section;

        public float volenv_val;

        //public float amplitude_that_reaches_noise_floor_nonloop;
        //public float amplitude_that_reaches_noise_floor_loop;

        /* mod env */
        public fluid_env_data[] modenv_data;
        public long modenv_count;
        public fluid_voice_envelope_index modenv_section;
        public float modenv_val;         /* the value of the modulation envelope */
        public float modenv_to_fc;
        public float modenv_to_pitch;

        /* mod lfo */
        public float modlfo_val;          /* the value of the modulation LFO */
        public uint modlfo_delay;       /* the delay of the lfo in samples */
        public float modlfo_incr;         /* the lfo frequency is converted to a per-buffer increment */
        public float modlfo_to_fc;
        public float modlfo_to_pitch;
        public float modlfo_to_vol;

        /* vib lfo */
        public float viblfo_val;        /* the value of the vibrato LFO */
        public long viblfo_delay;      /* the delay of the lfo in samples */
        public float viblfo_incr;       /* the lfo frequency is converted to a per-buffer increment */
        public float viblfo_to_pitch;

        /* resonant filter */
        //public float fres;              /* the resonance frequency, in cents (not absolute cents) */
        //public float last_fres;         /* Current resonance frequency of the IIR filter */
        //                                /* Serves as a flag: A deviation between fres and last_fres */
        //                                /* indicates, that the filter has to be recalculated. */
        //public float q_lin;             /* the q-factor on a linear scale */
        //public float filter_gain;       /* Gain correction factor, depends on q */
        //public float hist1, hist2;      /* Sample history for the IIR filter */
        //public bool filter_startup;             /* Flag: If set, the filter will be set directly. Else it changes smoothly. */

        ///* filter coefficients */
        ///* The coefficients are normalized to a0. */
        ///* b0 and b2 are identical => b02 */
        //public float b02;             /* b0 / a0 */
        //public float b1;              /* b1 / a0 */
        //public float a1;              /* a0 / a0 */
        //public float a2;              /* a1 / a0 */

        //public float b02_incr;
        //public float b1_incr;
        //public float a1_incr;
        //public float a2_incr;
        //public int filter_coeff_incr_count;

        /* pan */
        public float pan;
        private float amp_left;
        private float amp_right;

        /* interpolation method, as in fluid_interp in fluidsynth.h */
        // move to synth
        //public fluid_interp interp_method;

        /* for debugging */
        //public int debug;
        //TBC double ref;


        static int[] list_of_generators_to_initialize =
             {
                (int)fluid_gen_type.GEN_STARTADDROFS,                    /* SF2.01 page 48 #0  - Unity load wave from wave file, no real time change possible on wave attribute */
                (int)fluid_gen_type.GEN_ENDADDROFS,                      /*                #1  - Unity load wave from wave file, no real time change possible on wave attribute */
                (int)fluid_gen_type.GEN_STARTLOOPADDROFS,                /*                #2  - Unity load wave from wave file, no real time change possible on wave attribute */
                (int)fluid_gen_type.GEN_ENDLOOPADDROFS,                  /*                #3  - Unity load wave from wave file, no real time change possible on wave attribute */
                /* (int)fluid_gen_type.GEN_STARTADDRCOARSEOFS see comment below [1]        #4  - Unity load wave from wave file, no real time change possible on wave attribute */
                (int)fluid_gen_type.GEN_MODLFOTOPITCH,                   /*                #5   */
                (int)fluid_gen_type.GEN_VIBLFOTOPITCH,                   /*                #6   */
                (int)fluid_gen_type.GEN_MODENVTOPITCH,                   /*                #7   */
                (int)fluid_gen_type.GEN_FILTERFC,                        /*                #8   */
                (int)fluid_gen_type.GEN_FILTERQ,                         /*                #9   */
                (int)fluid_gen_type.GEN_MODLFOTOFILTERFC,                /*                #10  */
                (int)fluid_gen_type.GEN_MODENVTOFILTERFC,                /*                #11  */
                /* (int)fluid_gen_type.GEN_ENDADDRCOARSEOFS [1]                            #12  - Unity load wave from wave file, no real time change possible on wave attribute */
                (int)fluid_gen_type.GEN_MODLFOTOVOL,                     /*                #13  */
                /* not defined                                         #14  */
                (int)fluid_gen_type.GEN_CHORUSSEND,                      /*                #15  */
                (int)fluid_gen_type.GEN_REVERBSEND,                      /*                #16  */
                (int)fluid_gen_type.GEN_PAN,                             /*                #17  */
                /* not defined                                         #18  */
                /* not defined                                         #19  */
                /* not defined                                         #20  */
                (int)fluid_gen_type.GEN_MODLFODELAY,                     /*                #21  */
                (int)fluid_gen_type.GEN_MODLFOFREQ,                      /*                #22  */
                (int)fluid_gen_type.GEN_VIBLFODELAY,                     /*                #23  */
                (int)fluid_gen_type.GEN_VIBLFOFREQ,                      /*                #24  */
                (int)fluid_gen_type.GEN_MODENVDELAY,                     /*                #25  */
                (int)fluid_gen_type.GEN_MODENVATTACK,                    /*                #26  */
                (int)fluid_gen_type.GEN_MODENVHOLD,                      /*                #27  */
                (int)fluid_gen_type.GEN_MODENVDECAY,                     /*                #28  */
                /* (int)fluid_gen_type.GEN_MODENVSUSTAIN [1]                               #29  */
                (int)fluid_gen_type.GEN_MODENVRELEASE,                   /*                #30  */
                /* (int)fluid_gen_type.GEN_KEYTOMODENVHOLD [1]                             #31  */
                /* (int)fluid_gen_type.GEN_KEYTOMODENVDECAY [1]                            #32  */
                (int)fluid_gen_type.GEN_VOLENVDELAY,                     /*                #33  */
                (int)fluid_gen_type.GEN_VOLENVATTACK,                    /*                #34  */
                (int)fluid_gen_type.GEN_VOLENVHOLD,                      /*                #35  */
                (int)fluid_gen_type.GEN_VOLENVDECAY,                     /*                #36  */
                /* (int)fluid_gen_type.GEN_VOLENVSUSTAIN [1]                               #37  */
                (int)fluid_gen_type.GEN_VOLENVRELEASE,                   /*                #38  */
                /* (int)fluid_gen_type.GEN_KEYTOVOLENVHOLD [1]                             #39  */
                /* (int)fluid_gen_type.GEN_KEYTOVOLENVDECAY [1]                            #40  */
                /* (int)fluid_gen_type.GEN_STARTLOOPADDRCOARSEOFS [1]                      #45 - Unity load wave from wave file, no real time change possible on wave attribute */
                (int)fluid_gen_type.GEN_KEYNUM,                          /*                #46  */
                (int)fluid_gen_type.GEN_VELOCITY,                        /*                #47  */
                (int)fluid_gen_type.GEN_ATTENUATION,                     /*                #48  */
                /* (int)fluid_gen_type.GEN_ENDLOOPADDRCOARSEOFS [1]                        #50  - Unity load wave from wave file, no real time change possible on wave attribute */
                /* (int)fluid_gen_type.GEN_COARSETUNE           [1]                        #51  */
                /* (int)fluid_gen_type.GEN_FINETUNE             [1]                        #52  */
                (int)fluid_gen_type.GEN_OVERRIDEROOTKEY,                 /*                #58  */
                (int)fluid_gen_type.GEN_PITCH,                           /*                ---  */
             };

        static int[] list_of_weakgenerators_to_initialize =
     {
                (int)fluid_gen_type.GEN_STARTADDROFS,                    /* SF2.01 page 48 #0  - Unity load wave from wave file, no real time change possible on wave attribute */
                (int)fluid_gen_type.GEN_ENDADDROFS,                      /*                #1  - Unity load wave from wave file, no real time change possible on wave attribute */
                (int)fluid_gen_type.GEN_STARTLOOPADDROFS,                /*                #2  - Unity load wave from wave file, no real time change possible on wave attribute */
                (int)fluid_gen_type.GEN_ENDLOOPADDROFS,                  /*                #3  - Unity load wave from wave file, no real time change possible on wave attribute */
                /* (int)fluid_gen_type.GEN_STARTADDRCOARSEOFS see comment below [1]        #4  - Unity load wave from wave file, no real time change possible on wave attribute */
                //(int)fluid_gen_type.GEN_MODLFOTOPITCH,                   /*                #5   */
                //(int)fluid_gen_type.GEN_VIBLFOTOPITCH,                   /*                #6   */
                //(int)fluid_gen_type.GEN_MODENVTOPITCH,                   /*                #7   */
                //(int)fluid_gen_type.GEN_FILTERFC,                        /*                #8   */
                //(int)fluid_gen_type.GEN_FILTERQ,                         /*                #9   */
                //(int)fluid_gen_type.GEN_MODLFOTOFILTERFC,                /*                #10  */
                //(int)fluid_gen_type.GEN_MODENVTOFILTERFC,                /*                #11  */
                /* (int)fluid_gen_type.GEN_ENDADDRCOARSEOFS [1]                            #12  - Unity load wave from wave file, no real time change possible on wave attribute */
                //(int)fluid_gen_type.GEN_MODLFOTOVOL,                     /*                #13  */
                /* not defined                                         #14  */
                //(int)fluid_gen_type.GEN_CHORUSSEND,                      /*                #15  */
                //(int)fluid_gen_type.GEN_REVERBSEND,                      /*                #16  */
                (int)fluid_gen_type.GEN_PAN,                             /*                #17  */
                /* not defined                                         #18  */
                /* not defined                                         #19  */
                /* not defined                                         #20  */
                //(int)fluid_gen_type.GEN_MODLFODELAY,                     /*                #21  */
                //(int)fluid_gen_type.GEN_MODLFOFREQ,                      /*                #22  */
                //(int)fluid_gen_type.GEN_VIBLFODELAY,                     /*                #23  */
                //(int)fluid_gen_type.GEN_VIBLFOFREQ,                      /*                #24  */
                //(int)fluid_gen_type.GEN_MODENVDELAY,                     /*                #25  */
                //(int)fluid_gen_type.GEN_MODENVATTACK,                    /*                #26  */
                //(int)fluid_gen_type.GEN_MODENVHOLD,                      /*                #27  */
                //(int)fluid_gen_type.GEN_MODENVDECAY,                     /*                #28  */
                /* (int)fluid_gen_type.GEN_MODENVSUSTAIN [1]                               #29  */
                //(int)fluid_gen_type.GEN_MODENVRELEASE,                   /*                #30  */
                /* (int)fluid_gen_type.GEN_KEYTOMODENVHOLD [1]                             #31  */
                /* (int)fluid_gen_type.GEN_KEYTOMODENVDECAY [1]                            #32  */
                //(int)fluid_gen_type.GEN_VOLENVDELAY,                     /*                #33  */
                //(int)fluid_gen_type.GEN_VOLENVATTACK,                    /*                #34  */
                //(int)fluid_gen_type.GEN_VOLENVHOLD,                      /*                #35  */
                //(int)fluid_gen_type.GEN_VOLENVDECAY,                     /*                #36  */
                /* (int)fluid_gen_type.GEN_VOLENVSUSTAIN [1]                               #37  */
                //(int)fluid_gen_type.GEN_VOLENVRELEASE,                   /*                #38  */
                /* (int)fluid_gen_type.GEN_KEYTOVOLENVHOLD [1]                             #39  */
                /* (int)fluid_gen_type.GEN_KEYTOVOLENVDECAY [1]                            #40  */
                /* (int)fluid_gen_type.GEN_STARTLOOPADDRCOARSEOFS [1]                      #45 - Unity load wave from wave file, no real time change possible on wave attribute */
                (int)fluid_gen_type.GEN_KEYNUM,                          /*                #46  */
                (int)fluid_gen_type.GEN_VELOCITY,                        /*                #47  */
                (int)fluid_gen_type.GEN_ATTENUATION,                     /*                #48  */
                /* (int)fluid_gen_type.GEN_ENDLOOPADDRCOARSEOFS [1]                        #50  - Unity load wave from wave file, no real time change possible on wave attribute */
                /* (int)fluid_gen_type.GEN_COARSETUNE           [1]                        #51  */
                /* (int)fluid_gen_type.GEN_FINETUNE             [1]                        #52  */
                (int)fluid_gen_type.GEN_OVERRIDEROOTKEY,                 /*                #58  */
                (int)fluid_gen_type.GEN_PITCH,                           /*                ---  */
             };


        public const float _ratioHalfTone = 1.0594630943592952645618252949463f;

        public bool weakDevice;

        static public long TicksToMilli(long ticks)
        {
            return (long)(ticks / Nano100ToMilli);
        }

        static public float TicksToMilliF(long ticks)
        {
            return (float)ticks / (float)Nano100ToMilli;
        }

        public fluid_voice(MidiSynth psynth)
        {
            synth = psynth;
            IdSession = -1;
            IdVoice = LastId++;
            //Debug.Log("New  fluid_voice " + IdVoice);
            //Audiosource.PlayOneShot(new AudioClip(), 0);

            weakDevice = synth.MPTK_CorePlayer ? false : synth.MPTK_WeakDevice;

            gens = new HiGen[Enum.GetNames(typeof(fluid_gen_type)).Length];
            for (int i = 0; i < gens.Length; i++)
            {
                gens[i] = new HiGen();
                gens[i].type = (fluid_gen_type)i;
                gens[i].flags = fluid_gen_flags.GEN_UNUSED;
            }

            status = fluid_voice_status.FLUID_VOICE_CLEAN;
            chan = NO_CHANNEL;
            key = 0;
            vel = 0;
            midiChannel = null;
            sample = null;
            output_rate = synth.OutputRate;
#if MPTK_PRO && UNITY_ANDROID && UNITY_OBOE
            dsp_buf = new float[synth.FLUID_MAX_BUFSIZE]; // buffer are length variable
#else
            dsp_buf = new float[synth.FLUID_BUFSIZE];
#endif

            modenv_data = new fluid_env_data[Enum.GetNames(typeof(fluid_voice_envelope_index)).Length];
            for (int i = 0; i < modenv_data.Length; i++)
                modenv_data[i] = new fluid_env_data();

            volenv_data = new fluid_env_data[Enum.GetNames(typeof(fluid_voice_envelope_index)).Length];
            for (int i = 0; i < volenv_data.Length; i++)
                volenv_data[i] = new fluid_env_data();

            // The 'sustain' and 'finished' segments of the volume / modulation envelope are constant. 
            // They are never affected by any modulator or generator. 
            // Therefore it is enough to initialize them once during the lifetime of the synth.

            volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].count = 0xffffffff; // infiny until note off or duration is over
            volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].coeff = 1;
            volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].incr = 0;          // Volume remmains constant during sustain phase
            volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].min = -1;          // not used for sustain (constant volume)
            volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].max = 2; //1;     // not used for sustain (constant volume)

            volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].count = 0xffffffff;
            volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].coeff = 0;
            volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].incr = 0;
            volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].min = -1;
            volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].max = 1;

            modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].count = 0xffffffff;
            modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].coeff = 1;
            modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].incr = 0;
            modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].min = -1;
            modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].max = 2; //1; fluidsythn original value=2

            modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].count = 0xffffffff;
            modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].coeff = 0;
            modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].incr = 0;
            modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].min = -1;
            modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].max = 1;

#if MPTK_PRO
            InitFilter();
#endif
        }

        /// <summary>@brief
        /// Defined default voice value. Called also when a voice is reused.
        /// </summary>
        /// <param name="psample"></param>
        /// <param name="pchannel"></param>
        /// <param name="pkey"></param>
        /// <param name="pvel"></param>
        public void fluid_voice_init(MPTKChannel pchannel, int pkey, int pvel/*, double gain*/)
        {
            key = pkey;
            vel = pvel;
            midiChannel = pchannel;
            chan = midiChannel.Channel;
            mod_count = 0;
            FluidTicks = 0;
            has_looped = false; /* Will be set during voice_write when the 2nd loop point is reached */
            //last_fres = -1; /* The filter coefficients have to be calculated later in the DSP loop. */
            //filter_startup = true; /* Set the filter immediately, don't fade between old and new settings */
            //interp_method = channel.interp_method; move to synth

            /* vol env initialization */
            volenv_count = 0;
            volenv_section = (fluid_voice_envelope_index)0;
            volenv_val = 0;
            amp = 0.0f; /* The last value of the volume envelope, used to calculate the volume increment during processing */

            /* mod env initialization*/
            modenv_count = 0;
            modenv_section = (fluid_voice_envelope_index)0;
            modenv_val = 0;

            /* mod lfo */
            modlfo_val = 0;/* Fixme: Retrieve from any other existing voice on this channel to keep LFOs in unison? */

            /* vib lfo */
            viblfo_val = 0; /* Fixme: See mod lfo */

            /* Clear sample history in filter */
            //hist1 = 0;
            //hist2 = 0;

            /* Set all the generators to their default value, according to SF
             * 2.01 section 8.1.3 (page 48). The value of NRPN messages are
             * copied from the channel to the voice's generators. The sound font
             * loader overwrites them. The generator values are later converted
             * into voice parameters in
             * fluid_voice_calculate_runtime_synthesis_parameters.  */
            HiGen.fluid_gen_init(gens, midiChannel);
#if DEBUGPERF
            synth.DebugPerf("After fluid_gen_init voice:");
#endif
            /* For a looped sample, this value will be overwritten as soon as the
             * loop parameters are initialized (they may depend on modulators).
             * This value can be kept, it is a worst-case estimate.
             */

            //amplitude_that_reaches_noise_floor_nonloop = FLUID_NOISE_FLOOR/* / synth.gain*/;
            //amplitude_that_reaches_noise_floor_loop = FLUID_NOISE_FLOOR /*/ synth.gain*/;

            // Increment the reference count of the sample to prevent the unloading of the soundfont while this voice is playing.
            //sample.refcount++;
        }


        /// <summary>@brief
        ///  Adds a modulator to the voice.  "mode" indicates, what to do, if an identical modulator exists already.
        /// mode == FLUID_VOICE_ADD: Identical modulators on preset level are added
        /// mode == FLUID_VOICE_OVERWRITE: Identical modulators on instrument level are overwritten
        /// mode == FLUID_VOICE_DEFAULT: This is a default modulator, there can be no identical modulator.Don't check.
        /// </summary>
        /// <param name="pmod"></param>
        /// <param name="mode"></param>
        public void fluid_voice_add_mod(HiMod pmod, fluid_voice_addorover_mod mode)
        {
            /*
             * Some soundfonts come with a huge number of non-standard
             * controllers, because they have been designed for one particular
             * sound card.  Discard them, maybe print a warning.
             */

            if (((pmod.Flags1 & (byte)fluid_mod_flags.FLUID_MOD_CC) == 0)
                    && ((pmod.Src1 != 0)      /* SF2.01 section 8.2.1: Constant value */
                    && (pmod.Src1 != 2)       /* Note-on velocity */
                    && (pmod.Src1 != 3)       /* Note-on key number */
                    && (pmod.Src1 != 10)      /* Poly pressure */
                    && (pmod.Src1 != 13)      /* Channel pressure */
                    && (pmod.Src1 != 14)      /* Pitch wheel */
                    && (pmod.Src1 != 16)))    /* Pitch wheel sensitivity */
            {
                Debug.LogFormat($"Ignoring invalid controller, using non-CC source {pmod.Src1}.");
                return;
            }

            if (mode == fluid_voice_addorover_mod.FLUID_VOICE_ADD || mode == fluid_voice_addorover_mod.FLUID_VOICE_OVERWRITE)
            {
                foreach (HiMod mod1 in this.mods)
                {
                    /* if identical modulator exists, add them */
                    //fluid_mod_test_identity(mod1, mod))
                    if ((mod1.Dest == pmod.Dest) &&
                        (mod1.Src1 == pmod.Src1) &&
                        (mod1.Src2 == pmod.Src2) &&
                        (mod1.Flags1 == pmod.Flags1) &&
                        (mod1.Flags2 == pmod.Flags2))
                    {
                        if (mode == fluid_voice_addorover_mod.FLUID_VOICE_ADD)
                            mod1.Amount += pmod.Amount;
                        else
                            mod1.Amount = pmod.Amount;
                        return;
                    }
                }
            }

            // Add a new modulator (No existing modulator to add / overwrite).
            // Also, default modulators (FLUID_VOICE_DEFAULT) are added without checking, 
            // if the same modulator already exists. 
            if (this.mods.Count < HiMod.FLUID_NUM_MOD)
            {
                HiMod mod1 = new HiMod();
                mod1.Amount = pmod.Amount;
                mod1.Dest = pmod.Dest;
                mod1.Flags1 = pmod.Flags1;
                mod1.Flags2 = pmod.Flags2;
                mod1.Src1 = pmod.Src1;
                mod1.Src2 = pmod.Src2;
                this.mods.Add(mod1);
            }
        }


        public void fluid_voice_start(MPTKEvent note)
        {
            // The maximum volume of the loop is calculated and cached once for each sample with its nominal loop settings. 
            // This happens, when the sample is used for the first time.

            fluid_voice_calculate_runtime_synthesis_parameters();
#if DEBUGPERF
            synth.DebugPerf("After synthesis_parameters:");
#endif
            if (!weakDevice)
            {
                if (synth.VerboseEnvVolume)
                    for (int i = 0; i < (int)fluid_voice_envelope_index.FLUID_VOICE_ENVLAST; i++)
                        Debug.LogFormat("Volume Env. {0} {1,24} {2}", i, (fluid_voice_envelope_index)i, volenv_data[i].ToString());

                if (synth.VerboseEnvModulation)
                    for (int i = 0; i < (int)fluid_voice_envelope_index.FLUID_VOICE_ENVLAST; i++)
                        Debug.LogFormat("Modulation Env. {0} {1,24} {2}", i, (fluid_voice_envelope_index)i, modenv_data[i].ToString());

                // precalculation disabled, may cause issue on iOS and low output rate - V2.89.0 
                // if (!synth.AdsrSimplified)
                //{
                // Precalculate env. volume
                //fluid_env_data env_data = volenv_data[(int)volenv_section];
                //while (env_data.count <= 0d && (int)volenv_section < volenv_data.Length)
                //{
                //    float lastmax = env_data.max; ;
                //    volenv_section++;
                //    env_data = volenv_data[(int)volenv_section];
                //    //volenv_count = 0d;
                //    volenv_val = lastmax;
                //    if (synth.VerboseEnvVolume) Debug.LogFormat("Volume Precalculate Env. Count -. section:{0}  new count:{1} volenv_val:{2}", (int)volenv_section, env_data.count, volenv_val);
                //}

                //if (synth.VerboseEnvVolume)
                //    Debug.LogFormat("After precalc. volenv_section {0} ", volenv_section);


                //// Precalculate env. modulation
                //env_data = modenv_data[(int)modenv_section];
                //while (env_data.count <= 0d && (int)modenv_section < modenv_data.Length)
                //{
                //    float lastmax = env_data.max;
                //    modenv_section++;
                //    env_data = modenv_data[(int)modenv_section];
                //    modenv_count = 0;
                //    modenv_val = lastmax;
                //    if (synth.VerboseEnvModulation) Debug.LogFormat("Modulation Precalculate Env. Count -. section:{0}  new count:{1} modenv_val:{2}", (int)modenv_section, env_data.count, modenv_val);
                //}
                //}
                if (synth.AdsrSimplified)
                {
                    volenv_section = fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY;
                    volenv_val = 1;
                    volenv_count = volenv_data[(int)volenv_section].count;

                    modenv_section = fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY;
                    modenv_val = 1;
                    modenv_count = modenv_data[(int)modenv_section].count;
                }
            }
            else
                volenv_val = 1;


            StartVolume = synth.MPTK_Volume * (float)(fluid_conv.fluid_atten2amp(attenuation) * fluid_conv.fluid_cb2amp(960.0f * (1f - volenv_val)));
            IsLoop = ((gens[(int)fluid_gen_type.GEN_SAMPLEMODE].Val == (double)fluid_loop.FLUID_LOOP_UNTIL_RELEASE) || (gens[(int)fluid_gen_type.GEN_SAMPLEMODE].Val == (double)fluid_loop.FLUID_LOOP_DURING_RELEASE));

            // Force setting of the phase at the first DSP loop run This cannot be done earlier, because it depends on modulators.
            check_sample_sanity_flag = 1 << 1; //#define FLUID_SAMPLESANITY_STARTUP (1 << 1) 

            // Voice with status FLUID_VOICE_ON are played in background when CorePlayer is enabled
            status = fluid_voice_status.FLUID_VOICE_ON;
            LatenceTick = -1L;

            // A single tick represents one hundred nanoseconds or one ten-millionth of a second.
            // There are 10,000 ticks in a millisecond, or 10 million ticks in a second. 
            TimeAtStart = note.Delay * fluid_voice.Nano100ToMilli + DateTime.UtcNow.Ticks;
            TimeAtEnd = DurationTick > -1 ? TimeAtStart + DurationTick : long.MaxValue;
            LastTimeWrite = TimeAtStart;
            //time = 0.0;

            if (VoiceAudio != null)
                // Play sound with an AudioSource component
                VoiceAudio.RunUnityThread();
        }

        /// <summary>@brief
        /// in this function we calculate the values of all the parameters. the parameters are converted to their most useful unit for the DSP algorithm, 
        /// for example, number of samples instead of timecents.
        /// Some parameters keep their "perceptual" unit and conversion will be done in the DSP function.
        /// This is the case, for example, for the pitch since it is modulated by the controllers in cents.
        /// </summary>
        void fluid_voice_calculate_runtime_synthesis_parameters()
        {
            // When the voice is made ready for the synthesis process, a lot of voice-internal parameters have to be calculated.
            // At this point, the sound font has already set the -nominal- value for all generators (excluding GEN_PITCH). 
            // Most generators can be modulated - they include a nominal value and an offset (which changes with velocity, note number, channel parameters like
            // aftertouch, mod wheel...) 
            // Now this offset will be calculated as follows:
            //  - Process each modulator once.
            //  - Calculate its output value.
            //  - Find the target generator.
            //  - Add the output value to the modulation value of the generator.
            // Note: The generators have been initialized with fluid_gen_set_default_values.

            //foreach (HiMod m in mods) Debug.Log(m.ToString());


            foreach (HiMod m in mods)
            {
                //if (m.dest == (int)fluid_gen_type.GEN_FILTERFC)
                //    Debug.Log("GEN_FILTERFC");

                gens[m.Dest].Mod += m.fluid_mod_get_value(midiChannel, key, vel);
            }

            // The GEN_PITCH is a hack to fit the pitch bend controller into the modulator paradigm.  
            // Now the nominal pitch of the key is set.
            // Note about SCALETUNE: SF2.01 8.1.3 says, that this generator is a non-realtime parameter. So we don't allow modulation (as opposed

            // TO BE DONE ... one day
            //// to _GEN(voice, GEN_SCALETUNE) When the scale tuning is varied, one key remains fixed. Here C3 (MIDI number 60) is used.
            //if (midiChannel.tuning != null)
            //{
            //    gens[(int)fluid_gen_type.GEN_PITCH].Val = midiChannel.tuning.pitch[60] + (gens[(int)fluid_gen_type.GEN_SCALETUNE].Val / 100.0f * midiChannel.tuning.pitch[key] - midiChannel.tuning.pitch[60]);
            //}
            //else
            //{
                gens[(int)fluid_gen_type.GEN_PITCH].Val = (gens[(int)fluid_gen_type.GEN_SCALETUNE].Val * (key - 60.0f) + 100.0f * 60.0f);
            //}

            /* Now the generators are initialized, nominal and modulation value.
             * The voice parameters (which depend on generators) are calculated
             * with fluid_voice_update_param. Processing the list of generator
             * changes will calculate each voice parameter once.
             *
             * Note [1]: Some voice parameters depend on several generators. For
             * example, the pitch depends on GEN_COARSETUNE, GEN_FINETUNE and
             * GEN_PITCH.  voice.pitch.  Unnecessary recalculation is avoided
             * by removing all but one generator from the list of voice
             * parameters.  Same with GEN_XXX and GEN_XXXCOARSE: the
             * initialisation list contains only GEN_XXX.
             */

            // Calculate the voice parameter(s) dependent on each generator.
            if (!weakDevice)
                foreach (int igen in list_of_generators_to_initialize)
                    fluid_voice_update_param(igen);
            else
                foreach (int igen in list_of_weakgenerators_to_initialize)
                    fluid_voice_update_param(igen);

            // Make an estimate on how loud this voice can get at any time (attenuation). */
            min_attenuation_cB = fluid_voice_get_lower_boundary_for_attenuation();
            if (synth.VerboseEnvVolume) Debug.Log($"min_attenuation_cB:{min_attenuation_cB}");

        }


        /*
         * fluid_voice_get_lower_boundary_for_attenuation
         *
         * Purpose:
         *
         * A lower boundary for the attenuation (as in 'the minimum
         * attenuation of this voice, with volume pedals, modulators
         * etc. resulting in minimum attenuation, cannot fall below x cB) is
         * calculated.  This has to be called during fluid_voice_init, after
         * all modulators have been run on the voice once.  Also,
         * voice.attenuation has to be initialized.
         */
        float fluid_voice_get_lower_boundary_for_attenuation()
        {
            float possible_att_reduction_cB = 0;
            float lower_bound;

            foreach (HiMod m in mods)
            {
                // Modulator has attenuation as target and can change over time? 
                if ((m.Dest == (int)fluid_gen_type.GEN_ATTENUATION)
                    && ((m.Flags1 & (byte)fluid_mod_flags.FLUID_MOD_CC) > 0 || (m.Flags2 & (byte)fluid_mod_flags.FLUID_MOD_CC) > 0))
                {

                    float current_val = m.fluid_mod_get_value(midiChannel, key, vel);
                    float v = Mathf.Abs(m.Amount);

                    if ((m.Src1 == (byte)fluid_mod_src.FLUID_MOD_PITCHWHEEL)
                        || (m.Flags1 & (byte)fluid_mod_flags.FLUID_MOD_BIPOLAR) > 0
                        || (m.Flags2 & (byte)fluid_mod_flags.FLUID_MOD_BIPOLAR) > 0
                        || (m.Amount < 0))
                    {
                        /* Can this modulator produce a negative contribution? */
                        v *= -1f;
                    }
                    else
                    {
                        /* No negative value possible. But still, the minimum contribution is 0. */
                        v = 0f;
                    }

                    /* For example:
                     * - current_val=100
                     * - min_val=-4000
                     * - possible_att_reduction_cB += 4100
                     */
                    if (current_val > v)
                    {
                        possible_att_reduction_cB += (current_val - v);
                    }
                }
            }

            lower_bound = attenuation - possible_att_reduction_cB;

            /* SF2.01 specs do not allow negative attenuation */
            if (lower_bound < 0f)
            {
                lower_bound = 0f;
            }
            return lower_bound;
        }
        /// <summary>@brief
        /// The value of a generator (gen) has changed.  (The different generators are listed in fluidsynth.h, or in SF2.01 page 48-49). Now the dependent 'voice' parameters are calculated.
        /// fluid_voice_update_param can be called during the setup of the  voice (to calculate the initial value for a voice parameter), or
        /// during its operation (a generator has been changed due to real-time parameter modifications like pitch-bend).
        /// Note: The generator holds three values: The base value .val, an offset caused by modulators .mod, and an offset caused by the
        /// NRPN system. _GEN(voice, generator_enumerator) returns the sum of all three.
        /// From fluid_midi_send_event NOTE_ON -. synth_noteon -. fluid_voice_start -. fluid_voice_calculate_runtime_synthesis_parameters
        /// From fluid_midi_send_event CONTROL_CHANGE -. fluid_synth_cc -. fluid_channel_cc Default      -. fluid_synth_modulate_voices     -. fluid_voice_modulate
        /// From fluid_midi_send_event CONTROL_CHANGE -. fluid_synth_cc -. fluid_channel_cc ALL_CTRL_OFF -. fluid_synth_modulate_voices_all -. fluid_voice_modulate_all
        /// </summary>
        /// <param name="igen"></param>
        public void fluid_voice_update_param(int igen)
        {
            //Debug.Log("fluid_voice_update_param " + (fluid_gen_type)igen);
            float genVal = CalculateGeneratorValue(igen);
            switch (igen)
            {
                case (int)fluid_gen_type.GEN_PAN:
                    /* range checking is done in the fluid_pan function: range from -500 to 500 */
                    pan = genVal;
                    //if (midiChannel.preset.Num % 2 == 0)
                    //    pan = 500;
                    //else
                    //    pan = -500;
                    if (synth.MPTK_CorePlayer)
                    {
                        // Init with default volume channel value
                        amp_left = midiChannel.Volume;
                        amp_right = midiChannel.Volume;

                        if (synth.MPTK_EnablePanChange)
                        {
                            amp_left *= fluid_conv.fluid_pan(pan, true);
                            amp_right *= fluid_conv.fluid_pan(pan, false);
                        }

                        if (synth.VerboseCalcGen)
                            Debug.LogFormat($"Calc {(fluid_gen_type)igen} EnablePanChange={synth.MPTK_EnablePanChange} synth.gain={synth.gain:0.00} pan={pan:0.00} amp_left={amp_left:0.00} amp_right={amp_right:0.00} mptkChannel.volume={midiChannel.Volume}");
                    }
                    break;

                case (int)fluid_gen_type.GEN_ATTENUATION:
                    // Range: SF2.01 section 8.1.3 # 48 Motivation for range checking:OHPiano.SF2 sets initial attenuation to a whooping -96 dB
                    attenuation = genVal < 0.0f ? 0.0f : genVal > 14440.0f ? 1440.0f : genVal;
                    if (synth.VerboseCalcGen) Debug.LogFormat($"Channel:{chan} Calc {(fluid_gen_type)igen} genVal:{genVal} attenuation ={attenuation:0.00}");
                    break;

                // The pitch is calculated from the current note 
                case (int)fluid_gen_type.GEN_PITCH:
                case (int)fluid_gen_type.GEN_COARSETUNE:
                case (int)fluid_gen_type.GEN_FINETUNE:
                    // The testing for allowed range is done in 'fluid_ct2hz' 
                    pitch_update = CalculateGeneratorValue((int)fluid_gen_type.GEN_PITCH) +
                            CalculateGeneratorValue((int)fluid_gen_type.GEN_COARSETUNE) * 100f +
                            CalculateGeneratorValue((int)fluid_gen_type.GEN_FINETUNE);

                    if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} pitch={1:0.00} root_pitch={2:0.00} ", (fluid_gen_type)igen, pitch_update, root_pitch);
                    break;

                case (int)fluid_gen_type.GEN_REVERBSEND:
#if MPTK_PRO
                    /* The generator unit is 'tenths of a percent'. */
                    reverb_send = (genVal) / 1000f;
                    reverb_send = reverb_send < 0f ? 0f : reverb_send > 1f ? 1f : reverb_send;
                    amp_reverb = reverb_send /** synth.gain*/;
                    if (synth.VerboseCalcGen)
                        Debug.LogFormat("Calc {0} reverb_send={1:0.00} amp_reverb={2:0.00}", (fluid_gen_type)igen, reverb_send, amp_reverb);
#endif
                    break;

                case (int)fluid_gen_type.GEN_CHORUSSEND:
#if MPTK_PRO
                    /* The generator unit is 'tenths of a percent'. */
                    chorus_send = (genVal) / 1000f;
                    chorus_send = chorus_send < 0f ? 0f : chorus_send > 1f ? 1f : chorus_send;
                    amp_chorus = chorus_send /** synth.gain*/;
                    if (synth.VerboseCalcGen)
                        Debug.LogFormat("Calc {0} chorus_send={1:0.00} amp_chorus={2:0.00}", (fluid_gen_type)igen, chorus_send, amp_chorus);
#endif
                    break;

                case (int)fluid_gen_type.GEN_OVERRIDEROOTKEY:
                    // This is a non-realtime parameter. Therefore the .mod part of the generator can be neglected.
                    //* NOTE: origpitch sets MIDI root note while pitchadj is a fine tuning amount which offsets the original rate.  
                    // This means that the fine tuning is inverted with respect to the root note (so subtract it, not add).
                    if (genVal > -1)
                    {
                        //FIXME: use flag instead of -1
                        root_pitch = genVal * 100.0f - sample.PitchAdj;
                    }
                    else
                    {
                        root_pitch = sample.OrigPitch * 100.0f - sample.PitchAdj;
                    }

                    if (synth.MPTK_CorePlayer)
                    {
                        root_pitch = fluid_conv.fluid_ct2hz(root_pitch);
                        root_pitch *= output_rate / (float)sample.SampleRate;
                        //Debug.Log($"output_rate:{output_rate} SampleRate:{sample.SampleRate}");
                    }
                    if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} root_pitch={1:0.00} sample_PitchAdj={2:0.00}", (fluid_gen_type)igen, root_pitch, sample.PitchAdj);
                    break;

                case (int)fluid_gen_type.GEN_FILTERFC:
#if MPTK_PRO
                    // The resonance frequency is converted from absolute cents to midicents .val and .mod are both used, this permits real-time
                    // modulation.  The allowed range is tested in the 'fluid_ct2hz' function [PH,20021214]
                    fres = genVal;
                    resonant_filter.fluid_iir_filter_set_fres(fres);
                    if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} Fres={1:0.00} val={2:0.00}", (fluid_gen_type)igen, fres, genVal);
#endif
                    break;

                case (int)fluid_gen_type.GEN_FILTERQ:
#if MPTK_PRO
                    // The generator contains 'centibels' (1/10 dB) => divide by 10 to obtain dB
                    q_dB = genVal / 10f;

                    //// SF 2.01 page 59: The SoundFont specs ask for a gain reduction equal to half the height of the resonance peak (Q).  For example, for a 10 dB
                    ////  resonance peak, the gain is reduced by 5 dB.  This is done by multiplying the total gain with sqrt(1/Q).  `Sqrt' divides dB by 2 
                    //// (100 lin = 40 dB, 10 lin = 20 dB, 3.16 lin = 10 dB etc)
                    ////  The gain is later factored into the 'b' coefficients  (numerator of the filter equation).  This gain factor depends
                    ////  only on Q, so this is the right place to calculate it.
                    //filter_gain = 1f / Mathf.Sqrt(q_lin);

                    //// The synthesis loop will have to recalculate the filter coefficients. */
                    //last_fres = -1f;
                    resonant_filter.fluid_iir_filter_set_q(q_dB, synth.MPTK_EffectSoundFont.FilterQModOffset);
                    if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} q_dB={1:0.00} MPTK_SFFilterQModOffset={2:0.00}", (fluid_gen_type)igen, q_dB, synth.MPTK_EffectSoundFont.FilterQModOffset);
#endif
                    break;

                case (int)fluid_gen_type.GEN_MODLFOTOPITCH:
                    modlfo_to_pitch = genVal < -12000f ? -12000f : genVal > 12000f ? 12000f : genVal;
                    break;

                case (int)fluid_gen_type.GEN_MODLFOTOVOL:
                    modlfo_to_vol = genVal < -960f ? -960f : genVal > 960f ? 960f : genVal;
                    if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} modlfo_to_vol={1:0.00}", (fluid_gen_type)igen, modlfo_to_vol);
                    break;

                case (int)fluid_gen_type.GEN_MODLFOTOFILTERFC:
                    modlfo_to_fc = genVal < -12000f ? -12000f : genVal > 12000f ? 12000f : genVal;
                    break;

                case (int)fluid_gen_type.GEN_MODLFODELAY:
                    {
                        float x = genVal < -12000f ? -12000f : genVal > 5000f ? 5000f : genVal;
                        modlfo_delay = (uint)(output_rate * fluid_conv.fluid_tc2sec_delay(x));
                        if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} modlfo_delay={1:0.00} ms", (fluid_gen_type)igen, modlfo_delay / Nano100ToMilli);
                    }
                    break;

                case (int)fluid_gen_type.GEN_MODLFOFREQ:
                    {
                        //the frequency is converted into a delta value, per buffer of FLUID_BUFSIZE samples - the delay into a sample delay
                        float x = genVal < -16000.0f ? -16000.0f : genVal > 4500.0f ? 4500.0f : genVal;
                        modlfo_incr = (4.0f * synth.FLUID_BUFSIZE * fluid_conv.fluid_act2hz(x) / output_rate);
                        if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} modlfo_incr={1:0.00} x={2:0.00}", (fluid_gen_type)igen, modlfo_incr, x);
                    }
                    break;

                case (int)fluid_gen_type.GEN_VIBLFOFREQ:
                    {
                        // the frequency is converted into a delta value, per buffer of FLUID_BUFSIZE samples the delay into a sample delay
                        float x = genVal < -16000.0f ? -16000.0f : genVal > 4500.0f ? 4500.0f : genVal;
                        viblfo_incr = (4.0f * synth.FLUID_BUFSIZE * fluid_conv.fluid_act2hz(x) / output_rate);
                        if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} viblfo_incr={1:0.00} x={2:0.00}", (fluid_gen_type)igen, viblfo_incr, x);
                    }
                    break;

                case (int)fluid_gen_type.GEN_VIBLFODELAY:
                    {
                        float x = genVal < -12000f ? -12000f : genVal > 5000f ? 5000f : genVal;
                        viblfo_delay = (uint)(output_rate * fluid_conv.fluid_tc2sec_delay(x));
                        if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} viblfo_delay={1:0.00} x={2:0.00}",
                            (fluid_gen_type)igen, viblfo_delay, x);
                    }
                    break;

                case (int)fluid_gen_type.GEN_VIBLFOTOPITCH:
                    viblfo_to_pitch = genVal < -12000f ? -12000f : genVal > 12000f ? 12000f : genVal;
                    if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} viblfo_to_pitch={1:0.00} val={2:0.00}", (fluid_gen_type)igen, viblfo_to_pitch, genVal);
                    break;

                case (int)fluid_gen_type.GEN_KEYNUM:
                    {
                        // GEN_KEYNUM: SF2.01 page 46, item 46
                        // If this generator is active, it forces the key number to its value.  Non-realtime controller.
                        // There is a flag, which should indicate, whether a generator is enabled or not.  But here we rely on the default value of -1.
                        int x = Convert.ToInt32(genVal);
                        if (x >= 0) key = x;
                    }
                    break;

                case (int)fluid_gen_type.GEN_VELOCITY:
                    {
                        // GEN_VELOCITY: SF2.01 page 46, item 47
                        // If this generator is active, it forces the velocity to its value. Non-realtime controller.
                        // There is a flag, which should indicate, whether a generator is enabled or not. But here we rely on the default value of -1. 
                        int x = Convert.ToInt32(genVal);
                        if (x >= 0) vel = x;
                        //Debug.Log(string.Format("fluid_voice_update_param {0} vel={1} ", (fluid_gen_type)igen, x));

                    }
                    break;

                case (int)fluid_gen_type.GEN_MODENVTOPITCH:
                    modenv_to_pitch = genVal < -12000.0f ? -12000.0f : genVal > 12000.0f ? 12000.0f : genVal;
                    break;

                case (int)fluid_gen_type.GEN_MODENVTOFILTERFC:
                    // Range: SF2.01 section 8.1.3 # 1
                    // Motivation for range checking:Filter is reported to make funny noises now 
                    modenv_to_fc = genVal < -12000.0f ? -12000.0f : genVal > 12000.0f ? 12000.0f : genVal;
                    break;

                // sample start and ends points
                // Range checking is initiated via the check_sample_sanity flag, because it is impossible to check here:
                // During the voice setup, all modulators are processed, while the voice is inactive. Therefore, illegal settings may
                // occur during the setup (for example: First move the loop end point ahead of the loop start point => invalid, then move the loop start point forward => valid again.
                // Unity adaptation: wave are played from a wave file not from a global data buffer. It's not possible de change these
                // value after importing the SoudFont. Only loop address are taken in account whrn importing the SF
                case (int)fluid_gen_type.GEN_STARTADDROFS:              /* SF2.01 section 8.1.3 # 0 */
                case (int)fluid_gen_type.GEN_STARTADDRCOARSEOFS:        /* SF2.01 section 8.1.3 # 4 */
                    if (sample != null)
                    {
                        start = (int)(sample.Start
                            + (int)gens[(int)fluid_gen_type.GEN_STARTADDROFS].Val + gens[(int)fluid_gen_type.GEN_STARTADDROFS].Mod /*+ gens[(int)fluid_gen_type.GEN_STARTADDROFS].nrpn*/
                            + 32768 * (int)gens[(int)fluid_gen_type.GEN_STARTADDRCOARSEOFS].Val + gens[(int)fluid_gen_type.GEN_STARTADDRCOARSEOFS].Mod /*+ gens[(int)fluid_gen_type.GEN_STARTADDRCOARSEOFS].nrpn*/);
                        if (start >= sample.Data.Length) start = sample.Data.Length - 1;
                        check_sample_sanity_flag = 1; //? FLUID_SAMPLESANITY_CHECK(1 << 0)
                                                      //if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} start={1} val={2:0.00} mod={3:0.00}", (fluid_gen_type)igen, start, gens[igen].Val, gens[igen].Mod);
                    }
                    break;
                case (int)fluid_gen_type.GEN_ENDADDROFS:                 /* SF2.01 section 8.1.3 # 1 */
                case (int)fluid_gen_type.GEN_ENDADDRCOARSEOFS:           /* SF2.01 section 8.1.3 # 12 */
                    if (sample != null)
                    {
                        end = (int)(sample.End - 1
                            + (int)gens[(int)fluid_gen_type.GEN_ENDADDROFS].Val + gens[(int)fluid_gen_type.GEN_ENDADDROFS].Mod /*+ gens[(int)fluid_gen_type.GEN_ENDADDROFS].nrpn*/
                            + 32768 * (int)gens[(int)fluid_gen_type.GEN_ENDADDRCOARSEOFS].Val + gens[(int)fluid_gen_type.GEN_ENDADDRCOARSEOFS].Mod /*+ gens[(int)fluid_gen_type.GEN_ENDADDRCOARSEOFS].nrpn*/);
                        if (end >= sample.Data.Length) end = sample.Data.Length - 1;
                        check_sample_sanity_flag = 1; //? FLUID_SAMPLESANITY_CHECK(1 << 0)
                                                      //if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} end={1} val={2:0.00} mod={3:0.00}", (fluid_gen_type)igen, end, gens[igen].Val, gens[igen].Mod);
                    }
                    break;
                case (int)fluid_gen_type.GEN_STARTLOOPADDROFS:           /* SF2.01 section 8.1.3 # 2 */
                case (int)fluid_gen_type.GEN_STARTLOOPADDRCOARSEOFS:     /* SF2.01 section 8.1.3 # 45 */
                    if (sample != null)
                    {
                        loopstart =
                            (int)(sample.LoopStart +
                            (int)gens[(int)fluid_gen_type.GEN_STARTLOOPADDROFS].Val +
                            gens[(int)fluid_gen_type.GEN_STARTLOOPADDROFS].Mod +
                            //gens[(int)fluid_gen_type.GEN_STARTLOOPADDROFS].nrpn +
                            32768 * (int)gens[(int)fluid_gen_type.GEN_STARTLOOPADDRCOARSEOFS].Val +
                            gens[(int)fluid_gen_type.GEN_STARTLOOPADDRCOARSEOFS].Mod /*+ gens[(int)fluid_gen_type.GEN_STARTLOOPADDRCOARSEOFS].nrpn*/);
                        if (loopstart >= sample.Data.Length) loopstart = sample.Data.Length - 1;
                        check_sample_sanity_flag = 1; //? FLUID_SAMPLESANITY_CHECK(1 << 0)
                                                      //if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} loopstart={1} val={2:0.00} mod={3:0.00}", (fluid_gen_type)igen, loopstart, gens[igen].Val, gens[igen].Mod);
                    }
                    break;

                case (int)fluid_gen_type.GEN_ENDLOOPADDROFS:             /* SF2.01 section 8.1.3 # 3 */
                case (int)fluid_gen_type.GEN_ENDLOOPADDRCOARSEOFS:       /* SF2.01 section 8.1.3 # 50 */
                    if (sample != null)
                    {
                        loopend =
                            (int)(sample.LoopEnd +
                            (int)gens[(int)fluid_gen_type.GEN_ENDLOOPADDROFS].Val +
                            gens[(int)fluid_gen_type.GEN_ENDLOOPADDROFS].Mod
                            //+ gens[(int)fluid_gen_type.GEN_ENDLOOPADDROFS].nrpn
                            + 32768 * (int)gens[(int)fluid_gen_type.GEN_ENDLOOPADDRCOARSEOFS].Val +
                            gens[(int)fluid_gen_type.GEN_ENDLOOPADDRCOARSEOFS].Mod
                            /*+ gens[(int)fluid_gen_type.GEN_ENDLOOPADDRCOARSEOFS].nrpn*/);
                        if (loopend >= sample.Data.Length) loopend = sample.Data.Length - 1;
                        check_sample_sanity_flag = 1; //? FLUID_SAMPLESANITY_CHECK(1 << 0)
                                                      //if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} loopend={1} val={2:0.00} mod={3:0.00}", (fluid_gen_type)igen, loopend, gens[igen].Val, gens[igen].Mod);
                    }
                    break;

                //
                // Volume ADSR
                // -----------

                // - delay and hold times are converted to absolute number of samples
                // - sustain is converted to its absolute value
                // - attack, decay and release are converted to their increment per sample
                case (int)fluid_gen_type.GEN_VOLENVDELAY:                /* SF2.01 section 8.1.3 # 33 */
                    {
                        float x = genVal < -12000f ? -12000f : genVal > 5000f ? 5000f : genVal;

                        uint count = synth.MPTK_CorePlayer ?
                                        // NUM_BUFFERS_DELAY
                                        (uint)(output_rate * fluid_conv.fluid_tc2sec_delay(x) / synth.FLUID_BUFSIZE) :
                                        (uint)(Nano100ToMilli * fluid_conv.fluid_tc2sec_delay(x) * 1000f);
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].count = count;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].coeff = 0f;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].incr = 0f;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].min = -1f;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].max = 1f;
                        if (synth.VerboseCalcADSR)
                        {
                            float delaySec = fluid_conv.fluid_tc2sec_delay(x);
                            Debug.Log($"{(fluid_gen_type)igen,-30} genVal={x,-7} count={count,-5} incr={0:F7} delay={delaySec * 1000f:F7} ms output_rate:{output_rate} FLUID_BUFSIZE:{synth.FLUID_BUFSIZE}");
                        }
                    }
                    break;

                case (int)fluid_gen_type.GEN_VOLENVATTACK:               /* SF2.01 section 8.1.3 # 34 */
                    {
                        float x = genVal < -12000f ? -12000f : genVal > 8000f ? 8000f : genVal;

                        uint count = synth.MPTK_CorePlayer ?
                                        // NUM_BUFFERS_ATTACK
                                        1 + (uint)(output_rate * fluid_conv.fluid_tc2sec_attack(x) / synth.FLUID_BUFSIZE) :
                                        (uint)(Nano100ToMilli * fluid_conv.fluid_tc2sec_attack(x) * 1000f);

                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].count = count;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].coeff = 1f;
                        float incr = count != 0 ? 1f / count : 0f;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].incr = incr;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].min = -1f;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].max = 1f;
                        if (synth.VerboseCalcADSR)
                        {
                            float attackSec = fluid_conv.fluid_tc2sec_attack(x);
                            Debug.Log($"{(fluid_gen_type)igen,-30} genVal={x,-7} count={count,-5} incr={incr:F7} attack={attackSec * 1000f:F7} ms");
                        }
                    }
                    break;

                case (int)fluid_gen_type.GEN_VOLENVHOLD:                 /* SF2.01 section 8.1.3 # 35 */
                case (int)fluid_gen_type.GEN_KEYTOVOLENVHOLD:            /* SF2.01 section 8.1.3 # 39 */
                    {
                        uint count = synth.MPTK_CorePlayer ?
                            calculate_hold_decay_buffers((int)fluid_gen_type.GEN_VOLENVHOLD, (int)fluid_gen_type.GEN_KEYTOVOLENVHOLD, false) : /* 0 means: hold */
                            (uint)(Nano100ToMilli * calculate_hold_decay_ms((int)fluid_gen_type.GEN_VOLENVHOLD, (int)fluid_gen_type.GEN_KEYTOVOLENVHOLD, false)); /* 0 means: hold */

                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].count = count;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].coeff = 1f;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].incr = 0f; // Volume stay stable during hold phase
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].min = -1f;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].max = 2f;// was 1 with 2.05;
                        if (synth.VerboseCalcADSR)
                        {
                            float holdSec = calculate_hold_decay_buffers_seconds((int)fluid_gen_type.GEN_VOLENVHOLD, (int)fluid_gen_type.GEN_KEYTOVOLENVHOLD, false);
                            Debug.Log($"{(fluid_gen_type)igen,-30} genVal={genVal,-7} count={count,-5} incr={0:F7} hold={holdSec * 1000f:F7} ms");
                        }
                    }
                    break;

                case (int)fluid_gen_type.GEN_VOLENVDECAY:               /* SF2.01 section 8.1.3 # 36 */
                case (int)fluid_gen_type.GEN_VOLENVSUSTAIN:             /* SF2.01 section 8.1.3 # 37 */
                case (int)fluid_gen_type.GEN_KEYTOVOLENVDECAY:          /* SF2.01 section 8.1.3 # 40 */
                    {
                        // x = 1.0f - 0.001f * fluid_voice_gen_value(voice, GEN_VOLENVSUSTAIN);
                        float genv = CalculateGeneratorValue((int)fluid_gen_type.GEN_VOLENVSUSTAIN);
                        float x = 1f - 0.001f * genv;
                        x = x < 0f ? 0f : x > 1f ? 1f : x;

                        uint count = synth.MPTK_CorePlayer ?
                            calculate_hold_decay_buffers((int)fluid_gen_type.GEN_VOLENVDECAY, (int)fluid_gen_type.GEN_KEYTOVOLENVDECAY, true)
                            :
                            (uint)(Nano100ToMilli * calculate_hold_decay_ms((int)fluid_gen_type.GEN_VOLENVDECAY, (int)fluid_gen_type.GEN_KEYTOVOLENVDECAY, true));
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].count = count;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].coeff = 1f;
                        float incr = count != 0 ? -1f / count : 0f;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].incr = incr; ;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].min = x; // Value to reach 
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].max = 2f;// was 1 with 2.05;

                        if (synth.VerboseCalcADSR)
                        {
                            float decaySec = calculate_hold_decay_buffers_seconds((int)fluid_gen_type.GEN_VOLENVDECAY, (int)fluid_gen_type.GEN_KEYTOVOLENVDECAY, true);
                            //Debug.Log(string.Format("Calc {0} y={1:F7} count={2} incr={3:F7}", (fluid_gen_type)igen, y,
                            //    volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].count,
                            //    volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].incr));
                            Debug.Log($"{(fluid_gen_type)igen,-30} genVal={genv,-7} count={count,-5} incr={incr:F7} decay:{decaySec * 1000f:F7} ms target:{x}");
                        }
                    }
                    break;

                case (int)fluid_gen_type.GEN_VOLENVRELEASE:             /* SF2.01 section 8.1.3 # 38 */
                    {
                        float x = genVal < FLUID_MIN_VOLENVRELEASE ? FLUID_MIN_VOLENVRELEASE : genVal > 8000.0f ? 8000.0f : genVal;
                        uint count;
                        if (synth.MPTK_CorePlayer)
                        {
                            //NUM_BUFFERS_RELEASE
                            count = 1 + (uint)(output_rate * fluid_conv.fluid_tc2sec_release(x) * synth.MPTK_ReleaseTimeMod / synth.FLUID_BUFSIZE);
                        }
                        else
                        {
                            //uint count = 1 + (uint)(output_rate * fluid_conv.fluid_tc2sec_release(x) / fluid_synth_t.FLUID_BUFSIZE);
                            uint rt = (uint)(Nano100ToMilli * fluid_conv.fluid_tc2sec_release(x) * 1000f);
                            count = rt < synth.MPTK_ReleaseTimeMin ? synth.MPTK_ReleaseTimeMin : rt;
                        }
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].count = count;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].coeff = 1f;
                        float incr = count != 0 ? -1f / count : 0f;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].incr = incr;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].min = 0f;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].max = 1f;
                        if (synth.VerboseCalcADSR)
                        {
                            //Debug.Log(string.Format("Calc {0} genVal={1:F7} count={2} ms. incr={3:F7} fluid_tc2sec_release(x)={4:F7}",
                            //        (fluid_gen_type)igen, x,
                            //        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].count,
                            //        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].incr,
                            //        fluid_conv.fluid_tc2sec_release(x)));
                            float releaseSec = fluid_conv.fluid_tc2sec_release(x);
                            Debug.Log($"{(fluid_gen_type)igen,-30} genVal={x,-7} count={count,-5} incr={incr:F7} release={releaseSec * 1000f:F7} ms");
                        }

                    }
                    break;

                //
                // Modulation envelope
                //
                // - delay and hold times are converted to absolute number of samples
                // - sustain is converted to its absolute value
                // - attack, decay and release are converted to their increment per sample
                case (int)fluid_gen_type.GEN_MODENVDELAY:                /* SF2.01 section 8.1.3 # 33 */
                    {
                        float x = genVal < -12000f ? -12000f : genVal > 5000f ? 5000f : genVal;

                        uint count = synth.MPTK_CorePlayer ?
                                        // NUM_BUFFERS_DELAY
                                        (uint)(output_rate * fluid_conv.fluid_tc2sec_delay(x) / synth.FLUID_BUFSIZE) :
                                        (uint)(Nano100ToMilli * fluid_conv.fluid_tc2sec_delay(x) * 1000f);
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].count = count;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].coeff = 0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].incr = 0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].min = -1f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].max = 1f;
                        if (synth.VerboseCalcGen) Debug.Log(string.Format("Calc {0} count={1} ms. ", (fluid_gen_type)igen, modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].count));

                    }
                    break;

                case (int)fluid_gen_type.GEN_MODENVATTACK:               /* SF2.01 section 8.1.3 # 34 */
                    {
                        float x = genVal < -12000f ? -12000f : genVal > 8000f ? 8000f : genVal;

                        uint count = synth.MPTK_CorePlayer ?
                                        // NUM_BUFFERS_ATTACK
                                        1 + (uint)(output_rate * fluid_conv.fluid_tc2sec_attack(x) / synth.FLUID_BUFSIZE) :
                                        (uint)(Nano100ToMilli * fluid_conv.fluid_tc2sec_attack(x) * 1000f);

                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].count = count;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].coeff = 1f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].incr = count != 0 ? 1f / count : 0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].min = -1f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].max = 1f;
                        if (synth.VerboseCalcGen) Debug.Log(string.Format("Calc {0} x={1:F7} count={2} ms. incr={3:F7}", (fluid_gen_type)igen, x,
                            modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].count,
                            modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].incr));
                    }
                    break;

                case (int)fluid_gen_type.GEN_MODENVHOLD:                 /* SF2.01 section 8.1.3 # 35 */
                case (int)fluid_gen_type.GEN_KEYTOMODENVHOLD:            /* SF2.01 section 8.1.3 # 39 */
                    {
                        uint count = synth.MPTK_CorePlayer ?
                            calculate_hold_decay_buffers((int)fluid_gen_type.GEN_MODENVHOLD, (int)fluid_gen_type.GEN_KEYTOMODENVHOLD, false) : /* 0 means: hold */
                            (uint)(Nano100ToMilli * calculate_hold_decay_ms((int)fluid_gen_type.GEN_MODENVHOLD, (int)fluid_gen_type.GEN_KEYTOMODENVHOLD, false)); /* 0 means: hold */

                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].count = count;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].coeff = 1f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].incr = 0f; // Volume stay stable during hold phase
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].min = -1f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].max = 2f;// was 1 with 2.05;
                        if (synth.VerboseCalcGen)
                            Debug.Log(string.Format("Calc {0} count={1} ms={2} ",
                                    (fluid_gen_type)igen,
                                    modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].count,
                                    calculate_hold_decay_ms((int)fluid_gen_type.GEN_MODENVHOLD, (int)fluid_gen_type.GEN_KEYTOMODENVHOLD, false)));
                    }
                    break;

                case (int)fluid_gen_type.GEN_MODENVDECAY:               /* SF2.01 section 8.1.3 # 36 */
                case (int)fluid_gen_type.GEN_MODENVSUSTAIN:             /* SF2.01 section 8.1.3 # 37 */
                case (int)fluid_gen_type.GEN_KEYTOMODENVDECAY:          /* SF2.01 section 8.1.3 # 40 */
                    {
                        uint count = synth.MPTK_CorePlayer ?
                            calculate_hold_decay_buffers((int)fluid_gen_type.GEN_MODENVDECAY, (int)fluid_gen_type.GEN_KEYTOMODENVDECAY, true) : /* 1 for decay */
                            (uint)(Nano100ToMilli * calculate_hold_decay_ms((int)fluid_gen_type.GEN_MODENVDECAY, (int)fluid_gen_type.GEN_KEYTOMODENVDECAY, true)); /* 1 for decay */

                        float y = 1f - 0.001f * CalculateGeneratorValue((int)fluid_gen_type.GEN_MODENVSUSTAIN);
                        y = y < 0f ? 0f : y > 1f ? 1f : y;

                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].count = count;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].coeff = 1f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].incr = count != 0f ? -1f / count : 0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].min = y; // Value to reach 
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].max = 2f;// was 1 with 2.05;

                        if (synth.VerboseCalcGen) Debug.Log(string.Format("Calc {0} y={1:0.:F7} count={2} incr={3:F7}", (fluid_gen_type)igen, y,
                            modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].count,
                            modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].incr));
                    }
                    break;

                case (int)fluid_gen_type.GEN_MODENVRELEASE:             /* SF2.01 section 8.1.3 # 30 */
                    {
                        float x = genVal < -12000f ? -12000f : genVal > 8000.0f ? 8000.0f : genVal;
                        uint count;
                        if (synth.MPTK_CorePlayer)
                        {
                            //NUM_BUFFERS_RELEASE
                            count = 1 + (uint)(output_rate * fluid_conv.fluid_tc2sec_release(x) / synth.FLUID_BUFSIZE);
                        }
                        else
                        {
                            //uint count = 1 + (uint)(output_rate * fluid_conv.fluid_tc2sec_release(x) / fluid_synth_t.FLUID_BUFSIZE);
                            uint rt = (uint)(Nano100ToMilli * fluid_conv.fluid_tc2sec_release(x) * 1000f);
                            count = rt < synth.MPTK_ReleaseTimeMin ? synth.MPTK_ReleaseTimeMin : rt;
                        }
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].count = count;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].coeff = 1f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].incr = count != 0 ? -1f / count : 0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].min = 0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].max = 2f;
                        if (synth.VerboseCalcGen) Debug.Log(string.Format("Calc {0} x={1:F7} count={2} ms. incr={3:F7} tc2msec_release(x)={4:F7}",
                            (fluid_gen_type)igen, x,
                            modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].count,
                            modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].incr,
                            fluid_conv.fluid_tc2sec_release(x)));
                    }
                    break;

                default:
                    break;
            }
        }

        private float CalculateGeneratorValue(int igen)
        {
            float genVal = gens[igen].Val;
#if MPTK_PRO
            if (MptkEvent != null && MptkEvent.GensModifier != null && MptkEvent.GensModifier[igen] != null)
            {
                switch (MptkEvent.GensModifier[igen].Mode)
                {
                    case MPTKModeGeneratorChange.Override:
                        genVal = MptkEvent.GensModifier[igen].SoundFontVal;
                        break;
                    case MPTKModeGeneratorChange.Reinforce:
                        genVal += MptkEvent.GensModifier[igen].SoundFontVal;
                        break;
                    case MPTKModeGeneratorChange.Restaure:
                        // the soundfont value is not modified
                        break;
                }
                if (synth.VerboseCalcGen)
                {
                    Debug.Log($"Genrator {(fluid_gen_type)igen} modified from {gens[igen].Val} to {genVal} {MptkEvent.GensModifier[igen].Mode}");
                }
            }
#endif
            return genVal + gens[igen].Mod; //+ gens[igen].nrpn
        }


        /**
         * Compute the pitch for a key after applying Fluidsynth's tuning functionality
         * and channel coarse/fine tunings.
         * @param chan fluid_channel_t
         * @param key MIDI note number (0-127)
         * @return the pitch of the key
         */
        float fluid_channel_get_key_pitch(int key)
        {
            //if (midiChannel.tuning != null)
            //{
            //    return fluid_tuning_get_pitch(chan->tuning, key)
            //        + 100.0f * fluid_channel_get_gen(chan, GEN_COARSETUNE)
            //        + fluid_channel_get_gen(chan, GEN_FINETUNE);
            //}
            //else
            {
                return key * 100.0f;
            }
        }

        /**
         * Return the effective MIDI key of the playing voice.
         *
         * @param voice Voice instance
         * @return The MIDI key this voice is playing at
         *
         * If the voice was started from an instrument which uses a fixed key generator, it returns that.
         * Otherwise returns the same value as \c fluid_voice_get_key.
         *
         * @note The result of this function is only valid if the voice is playing.
         *
         * @since 1.1.7
         */
        int fluid_voice_get_actual_key()
        {
            float x = gens[(int)fluid_gen_type.GEN_KEYNUM].Val + gens[(int)fluid_gen_type.GEN_KEYNUM].Mod; //+ gens[igen].nrpn // fluid_voice_gen_value(voice, GEN_KEYNUM);

            if (x >= 0)
            {
                return (int)x;
            }
            else
            {
                return key;
            }
        }

        float calculate_hold_decay_buffers_seconds(int gen_base, int gen_key2base, bool is_decay)
        {
            /* Purpose:
             *
             * Returns the number of DSP loops, that correspond to the hold
             * (is_decay=0) or decay (is_decay=1) time.
             * gen_base=GEN_VOLENVHOLD, GEN_VOLENVDECAY, GEN_MODENVHOLD,
             * GEN_MODENVDECAY gen_key2base=GEN_KEYTOVOLENVHOLD,
             * GEN_KEYTOVOLENVDECAY, GEN_KEYTOMODENVHOLD, GEN_KEYTOMODENVDECAY
             */

            /* SF2.01 section 8.4.3 # 31, 32, 39, 40
             * GEN_KEYTOxxxENVxxx uses key 60 as 'origin'.
             * The unit of the generator is timecents per key number.
             * If KEYTOxxxENVxxx is 100, a key one octave over key 60 (72)
             * will cause (60-72)*100=-1200 timecents of time variation.
             * The time is cut in half.
             */
            float keysteps = 60f - fluid_channel_get_key_pitch(fluid_voice_get_actual_key()) / 100f;
            float timecents = CalculateGeneratorValue(gen_base) + CalculateGeneratorValue(gen_key2base) * keysteps;

            /* Range checking */
            if (is_decay)
            {
                /* SF 2.01 section 8.1.3 # 28, 36 */
                if (timecents > 8000f)
                {
                    timecents = 8000f;
                }
            }
            else
            {
                /* SF 2.01 section 8.1.3 # 27, 35 */
                if (timecents > 5000f)
                {
                    timecents = 5000f;
                }
                /* SF 2.01 section 8.1.2 # 27, 35:
                 * The most negative number indicates no hold time
                 */
                if (timecents <= -32768f)
                {
                    return 0;
                }
            }
            /* SF 2.01 section 8.1.3 # 27, 28, 35, 36 */
            if (timecents < -12000f)
            {
                timecents = -12000f;
            }

            //seconds = fluid_conv.fluid_tc2sec(timecents);
            return Mathf.Pow(2f, timecents / 1200f);
            /* Each DSP loop processes FLUID_BUFSIZE samples. */
        }

        /*
         * calculate_hold_decay_buffers
         */
        uint calculate_hold_decay_buffers(int gen_base, int gen_key2base, bool is_decay)
        {
            /* round to next full number of buffers */
            return (uint)((output_rate * calculate_hold_decay_buffers_seconds(gen_base, gen_key2base, is_decay)) / synth.FLUID_BUFSIZE + 0.5f);
        }

        /// <summary>@brief
        /// Returns the number of DSP loops, that correspond to the hold (is_decay=0) or decay (is_decay=1) time.
        /// gen_base=GEN_VOLENVHOLD, GEN_VOLENVDECAY, GEN_MODENVHOLD, GEN_MODENVDECAY gen_key2base=GEN_KEYTOVOLENVHOLD, GEN_KEYTOVOLENVDECAY, GEN_KEYTOMODENVHOLD, GEN_KEYTOMODENVDECAY
        /// </summary>
        /// <param name="gen_base"></param>
        /// <param name="gen_key2base"></param>
        /// <param name="is_decay"></param>
        /// <returns></returns>
        float calculate_hold_decay_ms(int gen_base, int gen_key2base, bool is_decay)
        {
            // SF2.01 section 8.4.3 # 31, 32, 39, 40
            // GEN_KEYTOxxxENVxxx uses key 60 as 'origin'.
            // The unit of the generator is timecents per key number.
            // If KEYTOxxxENVxxx is 100, a key one octave over key 60 (72) will cause (60-72)*100=-1200 timecents of time variation. The time is cut in half.
            float timecents = CalculateGeneratorValue(gen_base) + CalculateGeneratorValue(gen_key2base) * (60f - key);

            // Range checking 
            if (is_decay)
            {
                // SF 2.01 section 8.1.3 # 28, 36 
                if (timecents > 8000f)
                {
                    timecents = 8000f;
                }
            }
            else
            {
                // SF 2.01 section 8.1.3 # 27, 35 
                if (timecents > 5000f)
                {
                    timecents = 5000f;
                }
                // SF 2.01 section 8.1.2 # 27, 35: The most negative number indicates no hold time
                if (timecents <= -32768f)
                {
                    return 0f;
                }
            }
            // SF 2.01 section 8.1.3 # 27, 28, 35, 36 
            if (timecents < -12000f)
            {
                timecents = -12000f;
            }

            //fluid_conv.fluid_tc2sec(timecents);
            float seconds = Mathf.Pow(2f, timecents / 1200f);

            // Each DSP loop processes FLUID_BUFSIZE samples. Round to next full number of buffers 
            //buffers = Convert.ToInt32((output_rate * seconds) / (double)fluid_synth_t.FLUID_BUFSIZE + 0.5);

            return seconds * 1000f;
        }

        /**
         * fluid_voice_modulate
         *
         * In this implementation, I want to make sure that all controllers
         * are event based: the parameter values of the DSP algorithm should
         * only be updates when a controller event arrived and not at every
         * iteration of the audio cycle (which would probably be feasible if
         * the synth was made in silicon).
         *
         * The update is done in three steps:
         *
         * - first, we look for all the modulators that have the changed
         * controller as a source. This will yield a list of generators that
         * will be changed because of the controller event.
         *
         * - For every changed generator, calculate its new value. This is the
         * sum of its original value plus the values of al the attached
         * modulators.
         *
         * - For every changed generator, convert its value to the correct
         * unit of the corresponding DSP parameter
         *
         * @fn int fluid_voice_modulate(fluid_voice_t* voice, int cc, int ctrl, int val)
         * @param voice the synthesis voice
         * @param cc flag to distinguish between a continous control and a channel control (pitch bend, ...)
         * @param ctrl the control number
         * */
        public void fluid_voice_modulate(int is_cc, int ctrl)
        {
            //if (synth.VerboseVoice)
            //{
            //    fluid_global.FLUID_LOG(fluid_log_level.FLUID_INFO, "Chan={0}, CC={1}, Src={2}", channel.channum, cc, pctrl);
            //}
            for (int i = 0; i < mods.Count; i++)
            {
                HiMod m = mods[i];
                // step 1: find all the modulators that have the changed controller as input source.

                if ((((m.Src1 == ctrl) && ((m.Flags1 & (byte)fluid_mod_flags.FLUID_MOD_CC) != 0) && (is_cc != 0)) ||
                    (((m.Src1 == ctrl) && ((m.Flags1 & (byte)fluid_mod_flags.FLUID_MOD_CC) == 0) && (is_cc == 0)))) ||
                    (((m.Src2 == ctrl) && ((m.Flags2 & (byte)fluid_mod_flags.FLUID_MOD_CC) != 0) && (is_cc != 0)) ||
                    (((m.Src2 == ctrl) && ((m.Flags2 & (byte)fluid_mod_flags.FLUID_MOD_CC) == 0) && (is_cc == 0)))))
                {

                    int igen = m.Dest; //fluid_mod_get_dest
                    float modval = 0.0f;

                    // step 2: for every changed modulator, calculate the modulation value of its associated generator
                    for (int j = 0; j < mods.Count; j++)
                    {
                        HiMod m1 = mods[j];
                        if (m1.Dest == igen) //fluid_mod_has_dest(mod, gen)((mod).dest == gen)
                        {
                            modval += m1.fluid_mod_get_value(midiChannel, key, vel);
                        }
                    }

                    gens[igen].Mod = modval; //fluid_gen_set_mod(_gen, _val)  { (_gen).mod = (double)(_val); }

                    // step 3: now that we have the new value of the generator, recalculate the parameter values that are derived from the generator */
                    if (synth.VerboseController)
                        Debug.Log($"Modulate Generator Channel:{midiChannel.Channel} Controller:{(fluid_mod_src)ctrl} Continue={is_cc} Gen:{(fluid_gen_type)igen} Value:{modval:F2}");

                    fluid_voice_update_param(igen);
                }
            }
        }

        /// <summary>@brief
        /// Update all the modulators. This function is called after a ALL_CTRL_OFF MIDI message has been received (CC 121). 
        /// </summary>
        /// <param name="voice"></param>
        /// <returns></returns>
        public void fluid_voice_modulate_all()
        {
            //fluid_mod_t* mod;
            //int i, k, gen;
            //float modval;

            //Loop through all the modulators.
            //    FIXME: we should loop through the set of generators instead of the set of modulators. We risk to call 'fluid_voice_update_param'
            //    several times for the same generator if several modulators have that generator as destination. It's not an error, just a wast of
            //    energy (think polution, global warming, unhappy musicians, ...) 

            foreach (HiMod m in mods)
            {
                gens[m.Dest].Mod += m.fluid_mod_get_value(midiChannel, key, vel);
                int igen = m.Dest; //fluid_mod_get_dest
                float modval = 0.0f;
                // Accumulate the modulation values of all the modulators with destination generator 'gen'
                foreach (HiMod m1 in mods)
                {
                    if (m1.Dest == igen) //fluid_mod_has_dest(mod, gen)((mod).dest == gen)
                    {
                        modval += m1.fluid_mod_get_value(midiChannel, key, vel);
                    }
                }
                gens[igen].Mod = modval; //fluid_gen_set_mod(_gen, _val)  { (_gen).mod = (double)(_val); }

                // Update the parameter values that are depend on the generator 'gen'
                fluid_voice_update_param(igen);
            }
        }

        /* Purpose:
         *
         * Make sure, that sample start / end point and loop points are in
         * proper order. When starting up, calculate the initial phase.
         */
        void fluid_voice_check_sample_sanity()
        {
            int min_index_nonloop = (int)sample.Start;
            int max_index_nonloop = (int)sample.End;

            /* make sure we have enough samples surrounding the loop */
            int min_index_loop = (int)sample.Start + FLUID_MIN_LOOP_PAD;
            int max_index_loop = (int)sample.End - FLUID_MIN_LOOP_PAD + 1;  /* 'end' is last valid sample, loopend can be + 1 */

            if (check_sample_sanity_flag == 0)
            {
                return;
            }

            //Debug.LogFormat("Sample from {0} to {1}", sample.Start, sample.End);
            //Debug.LogFormat("Sample loop from {0} {1}", sample.LoopStart, sample.LoopEnd);
            //Debug.LogFormat("Playback from {0} to {1}", start, end);
            //Debug.LogFormat("Playback loop from {0} to {1}", loopstart, loopend);

            /* Keep the start point within the sample data */
            if (start < min_index_nonloop)
            {
                start = min_index_nonloop;
            }
            else if (start > max_index_nonloop)
            {
                start = max_index_nonloop;
            }

            /* Keep the end point within the sample data */
            if (end < min_index_nonloop)
            {
                end = min_index_nonloop;
            }
            else if (end > max_index_nonloop)
            {
                end = max_index_nonloop;
            }

            /* Keep start and end point in the right order */
            if (start > end)
            {
                int temp = start;
                start = end;
                end = temp;
                /*FLUID_LOG(FLUID_DBG, "Loop / sample sanity check: Changing order of start / end points!"); */
            }

            /* Zero length? */
            if (start == end)
            {
                fluid_voice_off();
                return;
            }


            if (IsLoop)
            {
                /* Keep the loop start point within the sample data */
                if (loopstart < min_index_loop)
                {
                    loopstart = min_index_loop;
                }
                else if (loopstart > max_index_loop)
                {
                    loopstart = max_index_loop;
                }

                /* Keep the loop end point within the sample data */
                if (loopend < min_index_loop)
                {
                    loopend = min_index_loop;
                }
                else if (loopend > max_index_loop)
                {
                    loopend = max_index_loop;
                }

                /* Keep loop start and end point in the right order */
                if (loopstart > loopend)
                {
                    int temp = loopstart;
                    loopstart = loopend;
                    loopend = temp;
                    /*FLUID_LOG(FLUID_DBG, "Loop / sample sanity check: Changing order of loop points!"); */
                }

                /* Loop too short? Then don't loop. */
                if (loopend < loopstart + FLUID_MIN_LOOP_SIZE)
                {
                    gens[(int)fluid_gen_type.GEN_SAMPLEMODE].Val = (float)fluid_loop.FLUID_UNLOOPED;
                    IsLoop = false;
                }

                /* The loop points may have changed. Obtain a new estimate for the loop volume. */
                /* Is the voice loop within the sample loop? */
                if (loopstart >= (int)sample.LoopStart && loopend <= (int)sample.LoopEnd)
                {
                    /* Is there a valid peak amplitude available for the loop? */
                    //if (sample.amplitude_that_reaches_noise_floor_is_valid)
                    //{
                    //    amplitude_that_reaches_noise_floor_loop = sample.amplitude_that_reaches_noise_floor /*/ synth.gain*/;
                    //}
                    //else
                    //{
                    //    /* Worst case */
                    //    amplitude_that_reaches_noise_floor_loop = amplitude_that_reaches_noise_floor_nonloop;
                    //}
                    //Debug.LogFormat("amplitude_that_reaches_noise_floor_loop:{0}" , amplitude_that_reaches_noise_floor_loop);
                }

            } /* if sample mode is looped */

            /* Run startup specific code (only once, when the voice is started)
#define FLUID_SAMPLESANITY_STARTUP (1 << 1) 
            */
            if ((check_sample_sanity_flag & 2) != 0)
            {
                if (max_index_loop - min_index_loop < FLUID_MIN_LOOP_SIZE)
                {
                    if (gens[(int)fluid_gen_type.GEN_SAMPLEMODE].Val == (float)fluid_loop.FLUID_LOOP_UNTIL_RELEASE ||
                        gens[(int)fluid_gen_type.GEN_SAMPLEMODE].Val == (float)fluid_loop.FLUID_LOOP_DURING_RELEASE)
                    {
                        gens[(int)fluid_gen_type.GEN_SAMPLEMODE].Val = (float)fluid_loop.FLUID_UNLOOPED;
                    }
                }

                // Set the initial phase of the voice (using the result from the start offset modulators). 
                //#define fluid_phase_set_int(a, b)    ((a) = ((unsigned long long)(b)) << 32)
                //fluid_phase_set_int(phase, start);
                phase = ((ulong)start) << 32;
            } /* if startup */

            // Is this voice run in loop mode, or does it run straight to the end of the waveform data?
            // (((_SAMPLEMODE(voice) == FLUID_LOOP_UNTIL_RELEASE) && (volenv_section < FLUID_VOICE_ENVRELEASE)) || (_SAMPLEMODE(voice) == FLUID_LOOP_DURING_RELEASE))

            if ((gens[(int)fluid_gen_type.GEN_SAMPLEMODE].Val == (float)fluid_loop.FLUID_LOOP_UNTIL_RELEASE &&
                 volenv_section < fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE) ||
                 gens[(int)fluid_gen_type.GEN_SAMPLEMODE].Val == (float)fluid_loop.FLUID_LOOP_DURING_RELEASE)
            {
                /* Yes, it will loop as soon as it reaches the loop point.  In
                 * this case we must prevent, that the playback pointer (phase)
                 * happens to end up beyond the 2nd loop point, because the
                 * point has moved.  The DSP algorithm is unable to cope with
                 * that situation.  So if the phase is beyond the 2nd loop
                 * point, set it to the start of the loop. No way to avoid some
                 * noise here.  Note: If the sample pointer ends up -before the
                 * first loop point- instead, then the DSP loop will just play
                 * the sample, enter the loop and proceed as expected => no
                 * actions required.

                  Purpose: Return the index and the fractional part, respectively. 
#define fluid_phase_index(_x) ((uint)((_x) >> 32))
                  int index_in_sample = fluid_phase_index(phase);
                */

                int index_in_sample = ((int)phase) >> 32;
                if (index_in_sample >= loopend)
                {
                    /* FLUID_LOG(FLUID_DBG, "Loop / sample sanity check: Phase after 2nd loop point!"); 
#define fluid_phase_set_int(a, b)    ((a) = ((unsigned long long)(b)) << 32)
                       fluid_phase_set_int(phase, loopstart);
                    */
                    phase = ((ulong)loopstart) << 32;
                }
            }
            /*    FLUID_LOG(FLUID_DBG, "Loop / sample sanity check: Sample from %i to %i, loop from %i to %i", start, end, loopstart, loopend); */
            // Sample sanity has been assured. Don't check again, until some sample parameter is changed by modulation. 
            check_sample_sanity_flag = 0;

            //Debug.LogFormat("Sane? playback loop from {0} to {1}", loopstart , loopend );
        }

        /*
         * fluid_voice_write - called from OnAudioFilterRead for each voices
         *
         * This is where it all happens. This function is called by the
         * synthesizer to generate the sound samples. The synthesizer passes
         * four audio buffers: left, right, reverb out, and chorus out.
         *
         * The biggest part of this function sets the correct values for all
         * the dsp parameters (all the control data boil down to only a few
         * dsp parameters). The dsp routine is #included in several places (fluid_dsp_core.c).
         */
        public int fluid_voice_write(
            long onAudioFilterTicks,
            float[] dsp_left_buf, float[] dsp_right_buf,
            float[] dsp_reverb_buf, float[] dsp_chorus_buf)
        {
            //uint i;
            //float incr;
            //float locfres;
            float target_amp;    /* target amplitude */
            int count;

            //int dsp_interp_method = interp_method;

            fluid_env_data env_data;
            float x;

            ticks = onAudioFilterTicks;
            if (synth.VerboseSynth)
            {
                NewTimeWrite = ticks;
                DeltaTimeWrite = NewTimeWrite - LastTimeWrite;
                LastTimeWrite = NewTimeWrite;
                TimeFromStart = NewTimeWrite - TimeAtStart;
            }

            Array.Clear(dsp_buf, 0, synth.FLUID_BUFSIZE);

            // A single tick represents one hundred nanoseconds or one ten-millionth of a second.
            // There are 10,000 ticks in a millisecond, or 10 million ticks in a second. 
            if (DurationTick >= 0 && volenv_section != fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE && ticks > TimeAtEnd)
            {
                // V2.89.0 The old soundcards would play any sample that didn't have loop points as a one-shot.  
                // In other words, once the sample was triggered by a NoteOn event it would be played once through to the end and could not be interrupted by a NoteOff event.  
                // This was used for drum samples for the most part.  I have several old MIDI files that take advantage of this.  
                // In one extreme example, nearly every NoteOn for the drum part is immediately followed by a NoteOff on the next tick.
                if (IsLoop || !synth.keepPlayingNonLooped)
                {
                    if (synth.VerboseEnvVolume) DebugVolEnv("Over Time, send noteoff");
                    fluid_voice_noteoff();
                }
                //else Debug.Log($"ignore noteoff Channel:{chan} key:{key} Isloop:{IsLoop} Ignore:{synth.keepPlayingNonLooped} DurationTick:{DurationTick} Name:{sample.Name}");
            }

            /******************* sample **********************/

            /* Range checking for sample- and loop-related parameters 
             * Initial phase is calculated here*/
            fluid_voice_check_sample_sanity();

            /******************* vol env **********************/

            env_data = volenv_data[(int)volenv_section];

            /* skip to the next section of the envelope if necessary */
            while (volenv_count >= env_data.count)
            {
                volenv_section++;
                env_data = volenv_data[(int)volenv_section];
                volenv_count = 0;
                if (synth.VerboseEnvVolume) DebugVolEnv("Next");
            }


            /* calculate the envelope value and check for valid range */
            x = env_data.coeff * volenv_val + env_data.incr;
            //Debug.LogFormat("t:{0} calc --> coeff:{1} * volenv_val:{2:F7} + incr:{3:F10} --> x:{4:F7} section:{5} ({6})",
            //     (System.DateTime.UtcNow.Ticks - TimeAtStart) / fluid_voice.Nano100ToMilli, 
            //    env_data.coeff, volenv_val, env_data.incr, x, volenv_section, (int)volenv_section);

            if (x < env_data.min)
            {
                x = env_data.min;
                volenv_section++;
                volenv_count = 0;
                if (synth.VerboseEnvVolume) DebugVolEnv("Min");
            }
            else if (x > env_data.max)
            {
                x = env_data.max;
                volenv_section++;
                volenv_count = 0;
                if (synth.VerboseEnvVolume) DebugVolEnv("Max");
            }

            volenv_val = x;
            volenv_count++;

            if (volenv_section == fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED)
            {
                //fluid_profile(FLUID_PROF_VOICE_RELEASE, ref);
                fluid_voice_off();
                return 0;
            }

            //fluid_check_fpe("voice_write vol env");

            /******************* mod env **********************/
            if (synth.MPTK_ApplyRealTimeModulator)
            {

                env_data = modenv_data[(int)modenv_section];

                /* skip to the next section of the envelope if necessary */
                while (modenv_count >= env_data.count)
                {
                    modenv_section++;
                    env_data = modenv_data[(int)modenv_section];
                    modenv_count = 0;
                    if (synth.VerboseEnvModulation) DebugModEnv("Next");
                }

                /* calculate the envelope value and check for valid range */
                x = env_data.coeff * modenv_val + env_data.incr;

                if (x < env_data.min)
                {
                    x = env_data.min;
                    modenv_section++;
                    modenv_count = 0;
                    if (synth.VerboseEnvModulation) DebugModEnv("Min");
                }
                else if (x > env_data.max)
                {
                    x = env_data.max;
                    modenv_section++;
                    modenv_count = 0;
                    if (synth.VerboseEnvModulation) DebugModEnv("Max");
                }

                modenv_val = x;
                modenv_count++;
                //fluid_check_fpe("voice_write mod env");
            }

            /******************* mod lfo **********************/

            if (synth.MPTK_ApplyModLfo && FluidTicks >= modlfo_delay)
            {
                modlfo_val += modlfo_incr;

                if (modlfo_val > 1f)
                {
                    //debug_lfo(voice, "modlfo_val > 1");
                    modlfo_incr = -modlfo_incr;
                    modlfo_val = 2f - modlfo_val;
                }
                else if (modlfo_val < -1f)
                {
                    //debug_lfo(voice, "modlfo_val <-1");
                    modlfo_incr = -modlfo_incr;
                    modlfo_val = -2f - modlfo_val;
                }
                //DebugLFO("TimeFromStartPlayNote >= modlfo_delay");
            }
            //else DebugLFO("TimeFromStartPlayNote < modlfo_delay");

            //fluid_check_fpe("voice_write mod LFO");

            /******************* vib lfo **********************/

            if (synth.MPTK_ApplyVibLfo && FluidTicks >= viblfo_delay)
            {
                viblfo_val += viblfo_incr;
                //DebugVib("viblfo_delay");

                if (viblfo_val > 1f)
                {
                    //DebugVib("viblfo_val > 1 freq:" + (TimeFromStartPlayNote - last_modvib_val_supp_1).ToString());
                    viblfo_incr = -viblfo_incr;
                    viblfo_val = 2f - viblfo_val;
                }
                else if (viblfo_val < -1f)
                {
                    //DebugVib("viblfo_val < -1");
                    viblfo_incr = -viblfo_incr;
                    viblfo_val = -2f - viblfo_val;
                }
            }
            //else DebugVib("TimeFromStartPlayNote < viblfo_delay");fluid_ct2hz_real

            // fluid_check_fpe("voice_write Vib LFO");

            /******************* amplitude **********************/

            /* calculate final amplitude
             * - initial gain
             * - amplitude envelope
             */

            if (volenv_section == fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY)
                goto post_process;  /* The volume amplitude is in hold phase. No sound is produced. */

            float amp_max = 0f;

            if (volenv_section == fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK)
            {
                /* the envelope is in the attack section: ramp linearly to max value.
                 * A positive modlfo_to_vol should increase volume (negative attenuation).
                 */
                //target_amp = fluid_atten2amp(voice->attenuation) * fluid_cb2amp(voice->modlfo_val * -voice->modlfo_to_vol) * voice->volenv_val;
                target_amp = fluid_conv.fluid_atten2amp(attenuation) * fluid_conv.fluid_cb2amp(modlfo_val * -modlfo_to_vol) * volenv_val;

                /*printf("ATTACK time:%d target_amp:'%0.3f' attenuation:'%0.3f' volenv_val:'%0.3f' fluid_atten2amp:'%0.3f' fluid_cb2amp:'%0.3f'  \n",
                    debug_time(),
                    target_amp,
                    attenuation,
                    volenv_val,
                    fluid_atten2amp(attenuation),
                    fluid_cb2amp(modlfo_val * -modlfo_to_vol) * volenv_val);*/
            }
            else
            {
                //target_amp = fluid_atten2amp(voice->attenuation) * fluid_cb2amp(960.0f * (1.0f - voice->volenv_val) + voice->modlfo_val * -voice->modlfo_to_vol);
                target_amp = fluid_conv.fluid_atten2amp(attenuation) * fluid_conv.fluid_cb2amp(960f * (1f - volenv_val) + modlfo_val * -modlfo_to_vol);

                //fprintf(stdout, "target_amp:'%f' attenuation:'%f' volenv_val:'%f' fluid_atten2amp:'%f' fluid_cb2amp:'%f'  \n", 
                //	target_amp,
                //	attenuation, 
                //	volenv_val,
                //	fluid_atten2amp(attenuation),
                //	fluid_cb2amp(960.0f * (1.0f - volenv_val), modlfo_val * -modlfo_to_vol));

                /* We turn off a voice, if the volume has dropped low enough. */

                /* A voice can be turned off, when an estimate for the volume
                 * (upper bound) falls below that volume, that will drop the
                 * sample below the noise floor.
                 */

                /* If the loop amplitude is known, we can use it if the voice loop is within
                 * the sample loop
                 */

                /* Is the playing pointer already in the loop? */
                //if (has_looped)
                //    amplitude_that_reaches_noise_floor = amplitude_that_reaches_noise_floor_loop;
                //else
                //    amplitude_that_reaches_noise_floor = amplitude_that_reaches_noise_floor_nonloop;
                //amplitude_that_reaches_noise_floor = 0.1f;

                /* attenuation_min is a lower boundary for the attenuation
                 * now and in the future (possibly 0 in the worst case).  Now the
                 * amplitude of sample and volenv cannot exceed amp_max (since
                 * volenv_val can only drop):
                 */

                amp_max = fluid_conv.fluid_atten2amp(min_attenuation_cB) * volenv_val;

                //Debug.LogFormat("t:{0} calc amp_max:{1:F7} volenv_val:{2:F7} section:{3} ({4})", (System.DateTime.UtcNow.Ticks - TimeAtStart) / fluid_voice.Nano100ToMilli, amp_max, volenv_val, volenv_section, (int)volenv_section);

                /* And if amp_max is already smaller than the known amplitude,
                 * which will attenuate the sample below the noise floor, then we
                 * can safely turn off the voice. Duh. */
                if (amp_max <= synth.MPTK_CutOffVolume)
                {
                    if (synth.VerboseVolume) DebugVolEnv($"CutOff {amp_max:F4} < {synth.MPTK_CutOffVolume:F4}");
                    fluid_voice_off();
                    goto post_process;
                }
            }

            /* Volume increment to go from amp to target_amp in FLUID_BUFSIZE steps */
            amp_incr = (target_amp - amp) / synth.FLUID_BUFSIZE;
            //Debug.Log($"{volenv_section} {target_amp} {amp}");

            //fluid_check_fpe("voice_write amplitude calculation");
            if (synth.VerboseVolume)
                DebugVolume($"amp_max:{amp_max:F2} target_amp:{target_amp:F2} amp:{amp:F2} CutOff:{synth.MPTK_CutOffVolume:F2}");

            /* no volume and not changing? - No need to process 
                removed in 2.9.1 - not sure of its utility
             */
            //if ((amp == 0f) && (amp_incr == 0f))
            //{
            //    //if (synth.VerboseVolume)
            //        Debug.Log($"no volume and not changing? - No need to process amp={amp} amp_incr={amp_incr}");
            //    goto post_process;
            //}
            /* Calculate the number of samples, that the DSP loop advances
             * through the original waveform with each step in the output
             * buffer. It is the ratio between the frequencies of original
             * waveform and output waveform.*/
            phase_incr = fluid_conv.fluid_ct2hz_real(
                pitch_update + modlfo_val * modlfo_to_pitch
                + viblfo_val * viblfo_to_pitch
                + modenv_val * modenv_to_pitch) / root_pitch;

            //fluid_check_fpe("voice_write phase calculation");

            /* if phase_incr is not advancing, set it to the minimum fraction value (prevent stuckage) */
            if (phase_incr == 0) phase_incr = 1;

            /*********************** run the dsp chain ************************
             * The sample is mixed with the output buffer.
             * The buffer has to be filled from 0 to FLUID_BUFSIZE-1.
             * Depending on the position in the loop and the loop size, this
             * may require several runs. */

            count = 0;
            switch (synth.InterpolationMethod)
            {
                case fluid_interp.None:
                    count = fluid_dsp_float.fluid_dsp_float_interpolate_none(this);
                    break;
                case fluid_interp.Linear:
                default:
                    count = fluid_dsp_float.fluid_dsp_float_interpolate_linear(this);
                    break;
                case fluid_interp.Cubic:
                    count = fluid_dsp_float.fluid_dsp_float_interpolate_4th_order(this);
                    break;
                case fluid_interp.Order7:
                    count = fluid_dsp_float.fluid_dsp_float_interpolate_7th_order(this);
                    break;
            }

#if MPTK_PRO
            CalcAndApplyFilter(count);
#endif
            if (count > 0)
                fluid_voice_effects(count, dsp_left_buf, dsp_right_buf, dsp_reverb_buf, dsp_chorus_buf);

            /* turn off voice if short count (sample ended and not looping) */
            if (count < synth.FLUID_BUFSIZE)
            {
                //fluid_profile(FLUID_PROF_VOICE_RELEASE, ref);
                fluid_voice_off();
            }

        post_process:
            FluidTicks += (uint)synth.FLUID_BUFSIZE;
            //fluid_check_fpe("voice_write postprocess");
            return 0;
        }

#if DEBUGTIME
        public int countIteration;
        public double cumulDeltaTime;
        public double averageDeltaTime;
        public double cumulProcessTime;
        public double averageProcessTime;
        private double startProcessTime;
#endif

        /* Purpose:
         *
         * - mixes the processed sample to left and right output using the pan setting
         * - sends the processed sample to chorus and reverb
         */
        void fluid_voice_effects(int count, float[] dsp_left_buf, float[] dsp_right_buf, float[] dsp_reverb_buf, float[] dsp_chorus_buf)
        {
            int dsp_i;
            float v;

            /* pan (Copy the signal to the left and right output buffer) The voice
            * panning generator has a range of -500 .. 500.  
            * If it is centered, it's close to 0. amp_left and amp_right are then the
            * same, and we can save one multiplication per voice and sample.
            */
            if (!synth.MPTK_EnablePanChange || (-0.5f < pan) && (pan < 0.5f))
            {
                /* The voice is centered. Use amp_left twice (with mptkChannel.volume). */
                for (dsp_i = 0; dsp_i < count; dsp_i++)
                {
                    v = amp_left * dsp_buf[dsp_i];
                    dsp_left_buf[dsp_i] += v;
                    dsp_right_buf[dsp_i] += v;
                    //if (dsp_i < 50)Debug.LogFormat("dsp_i:{0} amp_left:{1,0:F7}  dsp_buf[dsp_i]:{2,0:F7} dsp_left_buf[dsp_i]:{3,0:F7}", dsp_i, amp_left, dsp_buf[dsp_i], dsp_left_buf[dsp_i]);
                }
            }
            else    /* The voice is not centered. Stereo samples have one side zero. */
            {
                if (amp_left != 0f)
                {
                    for (dsp_i = 0; dsp_i < count; dsp_i++)
                    {
                        dsp_left_buf[dsp_i] += amp_left * dsp_buf[dsp_i];
                        //if (dsp_i < 50) Debug.LogFormat("dsp_i:{0} amp_left:{1,0:F7}  dsp_buf[dsp_i]:{2,0:F7} dsp_left_buf[dsp_i]:{3,0:F7}", dsp_i, amp_left, dsp_buf[dsp_i], dsp_left_buf[dsp_i]);
                    }
                }

                if (amp_right != 0f)
                {
                    for (dsp_i = 0; dsp_i < count; dsp_i++)
                        dsp_right_buf[dsp_i] += amp_right * dsp_buf[dsp_i];
                }
            }

#if MPTK_PRO
            ApplyEffect(count, dsp_reverb_buf, dsp_chorus_buf);
#endif
        }

        public IEnumerator<float> Release()
        {
            //Debug.Log("Release " + IdVoice);
            fluid_voice_noteoff(true);
            yield return 0;
        }

        /// <summary>@brief
        /// Move phase enveloppe to release
        /// </summary>
        public void fluid_voice_noteoff(bool force = false)
        {
            if (status == fluid_voice_status.FLUID_VOICE_ON || status == fluid_voice_status.FLUID_VOICE_SUSTAINED)
            {
                if (!weakDevice)
                {
                    if (!force && midiChannel != null && midiChannel.Controller(MPTKController.Sustain) >= 64)
                    {
                        status = fluid_voice_status.FLUID_VOICE_SUSTAINED;
                    }
                    else
                    {
                        //Debug.Log($"fluid_voice_noteoff Channel:{chan} key:{key} Isloop:{IsLoop} Ignore:{synth.keepPlayingNonLooped} DurationTick:{DurationTick} Name:{sample.Name}");
                        if (volenv_section == fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK)
                        {
                            // A voice is turned off during the attack section of the volume envelope.  
                            // The attack section ramps up linearly with amplitude. 
                            // The other sections use logarithmic scaling. 
                            // Calculate new volenv_val to achieve equivalent amplitude during the release phase for seamless volume transition.

                            //if (synth.VerboseEnvVolume) DebugVolEnv("noteoff ATTACK");
                            if (volenv_val > 0)
                            {
                                float env_value;
                                if (synth.MPTK_ApplyModLfo)
                                {
                                    float lfo = modlfo_val * -modlfo_to_vol;
                                    float vol = volenv_val * Mathf.Pow(10f, lfo / -200f);
                                    env_value = -((-200f * Mathf.Log(vol) / Mathf.Log(10f) - lfo) / 960f - 1f);
                                }
                                else
                                {
                                    env_value = Convert.ToInt64(-((-200f * Mathf.Log(volenv_val) / Mathf.Log(10f)) / 960f - 1f));
                                }
                                volenv_val = env_value > 1 ? 1 : env_value < 0 ? 0 : env_value;
                            }
                            if (synth.VerboseEnvVolume) DebugVolEnv("noteoff ATTACK");
                        }
                        volenv_section = fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE;
                        volenv_count = 0;
                        if (synth.VerboseEnvVolume) DebugVolEnv("noteoff");

                        modenv_section = fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE;
                        modenv_count = 0;
                        if (synth.VerboseEnvModulation) DebugModEnv("noteoff");
                    }
                }
                else
                {
                    volenv_section = fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE;
                    volenv_count = 0;
                }
            }
        }


        /*
         * fluid_voice_kill_excl
         *
         * Percussion sounds can be mutually exclusive: for example, a 'closed
         * hihat' sound will terminate an 'open hihat' sound ringing at the
         * same time. This behaviour is modeled using 'exclusive classes',
         * turning on a voice with an exclusive class other than 0 will kill
         * all other voices having that exclusive class within the same preset
         * or channel.  fluid_voice_kill_excl gets called, when 'voice' is to
         * be killed for that reason.
         */
        public int fluid_voice_kill_excl()
        {
            if (!(status == fluid_voice_status.FLUID_VOICE_ON) || status == fluid_voice_status.FLUID_VOICE_SUSTAINED)
            {
                return 0;
            }

            // Turn off the exclusive class information for this voice, so that it doesn't get killed twice fluid_voice_gen_set(voice, GEN_EXCLUSIVECLASS, 0);
            gens[(int)fluid_gen_type.GEN_EXCLUSIVECLASS].Val = 0f;
            gens[(int)fluid_gen_type.GEN_EXCLUSIVECLASS].flags = fluid_gen_flags.GEN_SET_INSTRUMENT;

            if (synth.VerboseKillByExclusive) DebugKillByExclusive($"Release sample with class {gens[(int)fluid_gen_type.GEN_EXCLUSIVECLASS].Val}");

            /* If the voice is not yet in release state, put it into release state */
            if (volenv_section != fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE)
            {
                volenv_section = fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE;
                volenv_count = 0;
                modenv_section = fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE;
                modenv_count = 0;
            }

            // Speed up the volume envelope 
            // The value was found through listening tests with hi-hat samples. 
            //fluid_voice_gen_set(voice, GEN_VOLENVRELEASE, -200);
            gens[(int)fluid_gen_type.GEN_VOLENVRELEASE].Val = -200f;
            gens[(int)fluid_gen_type.GEN_VOLENVRELEASE].flags = fluid_gen_flags.GEN_SET_INSTRUMENT;
            fluid_voice_update_param((int)fluid_gen_type.GEN_VOLENVRELEASE);

            // Speed up the modulation envelope 
            //fluid_voice_gen_set(voice, GEN_MODENVRELEASE, -200);
            gens[(int)fluid_gen_type.GEN_MODENVRELEASE].Val = -200f;
            gens[(int)fluid_gen_type.GEN_MODENVRELEASE].flags = fluid_gen_flags.GEN_SET_INSTRUMENT;
            fluid_voice_update_param((int)fluid_gen_type.GEN_MODENVRELEASE);

            return 0;
        }

        /*
        * fluid_voice_off
        *
        * Purpose:
        * Turns off a voice, meaning that it is not processed
        * anymore by the DSP loop.
        */
        public void fluid_voice_off()
        {
            chan = NO_CHANNEL;
            volenv_section = fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED;
            if (synth.VerboseVoice) DebugSynth("fluid_voice_off");
            volenv_count = 0;
            if (VoiceAudio != null && VoiceAudio.Audiosource != null)
            {
                VoiceAudio.Audiosource.volume = 0;
            }
            modenv_section = fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED;
            modenv_count = 0;
            status = fluid_voice_status.FLUID_VOICE_OFF;
        }

        public void DebugKillByExclusive(string info)
        {
            Debug.LogFormat($"KillByExclusive - [{IdVoice,4}] TimeFromStart:{TicksToMilli(ticks - TimeAtStart)} ms Delta:{TicksToMilliF(DeltaTimeWrite):F2} ms {sample.Name} {info}");
        }

        public void DebugSynth(string info)
        {
            Debug.LogFormat($"Synth - [{IdVoice,4}] TimeFromStart:{TicksToMilli(ticks - TimeAtStart)} ms Delta:{TicksToMilliF(DeltaTimeWrite):F2} ms {info}");
        }

        public void DebugOverload(string info)
        {
            Debug.LogFormat($"Overload - [{IdVoice,4}] TimeFromStart:{TicksToMilli(ticks - TimeAtStart)} ms Delta:{TicksToMilliF(DeltaTimeWrite):F2} ms {info}");
        }

        public void DebugVolume(string info)
        {
            Debug.LogFormat($"Volume - [{IdVoice,4}] TimeFromStart:{TicksToMilli(ticks - TimeAtStart)} ms Delta:{TicksToMilliF(DeltaTimeWrite):F2} ms {volenv_section} {info}");
        }

        public void DebugVolEnv(string info)
        {
            Debug.LogFormat("VolEnv - [{0,4}] {1,-25} TimeFromStart:{2} ms Delta:{3:F2} ms section:{4} volenv_val:{5:0.000} volenv_count:{6} incr:{7:0.0000} coeff:{8:0.00}",
               IdVoice, info, TicksToMilli(ticks - TimeAtStart), TicksToMilliF(DeltaTimeWrite),
               volenv_section, volenv_val, volenv_data[(int)volenv_section].count, volenv_data[(int)volenv_section].incr, volenv_data[(int)volenv_section].coeff);
        }

        public void DebugModEnv(string info)
        {
            Debug.LogFormat("ModEnv - [{0,4}] {1,-15} TimeFromStart:{2:0.000} Delta:{3:0.000} section:{4} modenv_val:{5:0.000} modenv_count:{6} incr:{7:0.0000}",
           IdVoice, info, TicksToMilli(ticks - TimeAtStart), TicksToMilliF(DeltaTimeWrite),
           modenv_section, modenv_val, modenv_data[(int)modenv_section].count, modenv_data[(int)modenv_section].incr);
        }

        public void DebugLFO(string info)
        {
            Debug.LogFormat("[{0,4}] {1,-15} TimeFromStart:{2:00000.000} Delta:{3:0.000} modlfo_delay:{4} modlfo_incr:{5:0.000} modlfo_val:{6:0.000} modlfo_to_vol:{7:0.000}",
               IdVoice, info, TicksToMilli(ticks - TimeAtStart), TicksToMilliF(DeltaTimeWrite), modlfo_delay, modlfo_incr, modlfo_val, modlfo_to_vol);
        }
        public void DebugVib(string info)
        {
            Debug.LogFormat("[{0,4}] {1,-15} TimeFromStart:{2:00000.000} Delta:{3:0.000} viblfo_delay:{4} viblfo_incr:{5:0.000} viblfo_val:{6:0.000} viblfo_to_pitch:{7:0.000} -. pitch mod:{8:0.000}",
               IdVoice, info, TicksToMilli(ticks - TimeAtStart), TicksToMilliF(DeltaTimeWrite), viblfo_delay, viblfo_incr, viblfo_val, viblfo_to_pitch, (float)(1d + viblfo_val * viblfo_to_pitch / 1000d));
        }
    }
}
