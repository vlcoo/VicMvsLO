using System.Collections.Generic;
using UnityEngine;

namespace MidiPlayerTK
{

    /* Flags telling the polarity of a modulator.  Compare with SF2.01
       section 8.2. Note: The numbers of the bits are different!  (for
       example: in the flags of a SF modulator, the polarity bit is bit
       nr. 9) */
    public enum fluid_mod_flags
    {
        FLUID_MOD_POSITIVE = 0,
        FLUID_MOD_NEGATIVE = 1,
        FLUID_MOD_UNIPOLAR = 0,
        FLUID_MOD_BIPOLAR = 2,
        FLUID_MOD_LINEAR = 0,
        FLUID_MOD_CONCAVE = 4,
        FLUID_MOD_CONVEX = 8,
        FLUID_MOD_SWITCH = 12,
        FLUID_MOD_GC = 0,
        FLUID_MOD_CC = 16
    }

    /* Flags telling the source of a modulator.  This corresponds to
     * SF2.01 section 8.2.1 */
    public enum fluid_mod_src
    {
        FLUID_MOD_NONE = 0,
        FLUID_MOD_VELOCITY = 2,
        FLUID_MOD_KEY = 3,
        FLUID_MOD_KEYPRESSURE = 10,
        FLUID_MOD_CHANNELPRESSURE = 13,
        FLUID_MOD_PITCHWHEEL = 14,
        FLUID_MOD_PITCHWHEELSENS = 16
    }

    /// <summary>@brief
    /// Defined Modulator from fluid_mod_t
    /// </summary>
    public class HiMod
    {
        /* Maximum number of modulators in a voice */
        public const int FLUID_NUM_MOD = 64;

        public byte Dest;
        public byte Src1;
        public byte Flags1;
        public byte Src2;
        public byte Flags2;
        public float Amount;

        // Modulator structure read from SF
        public ushort SfSrc;        /* source modulator */
        public ushort SfAmtSrc;     /* second source controls amnt of first */
        public ushort SfTrans;      /* transform applied to source */

        public float fluid_mod_get_value(MPTKChannel chan, int key, int vel)
        {
            float v1 = 0.0f, v2 = 1.0f;
            float range1 = 127.0f, range2 = 127.0f;

            if (chan == null)
            {
                return 0.0f;
            }

            /* 'special treatment' for default controller
             *
             *  Reference: SF2.01 section 8.4.2
             *
             * The GM default controller 'vel-to-filter cut off' is not clearly defined: If implemented according to the specs, the filter
             * frequency jumps between vel=63 and vel=64.  To maintain compatibility with existing sound fonts, the implementation is
             * 'hardcoded', it is impossible to implement using only one modulator otherwise.
             *
             * I assume here, that the 'intention' of the paragraph is one octave (1200 cents) filter frequency shift between vel=127 and
             * vel=64.  'amount' is (-2400), at least as long as the controller is set to default.
             *
             * Further, the 'appearance' of the modulator (source enumerator, destination enumerator, flags etc) is different from that
             * described in section 8.4.2, but it matches the definition used in several SF2.1 sound fonts (where it is used only to turn it off).
             * */
            if ((Dest == (byte)fluid_gen_type.GEN_FILTERFC) &&
                (Src2 == (int)fluid_mod_src.FLUID_MOD_VELOCITY) &&
                (Src1 == (int)fluid_mod_src.FLUID_MOD_VELOCITY) &&
                (Flags1 == ((byte)fluid_mod_flags.FLUID_MOD_GC | (byte)fluid_mod_flags.FLUID_MOD_UNIPOLAR | (byte)fluid_mod_flags.FLUID_MOD_NEGATIVE | (byte)fluid_mod_flags.FLUID_MOD_LINEAR)) &&
                (Flags2 == ((byte)fluid_mod_flags.FLUID_MOD_GC | (byte)fluid_mod_flags.FLUID_MOD_UNIPOLAR | (byte)fluid_mod_flags.FLUID_MOD_POSITIVE | (byte)fluid_mod_flags.FLUID_MOD_SWITCH)))
               
            {
                if (vel < 64)
                {
                    return Amount / 2.0f;
                }
                else
                {
                    return Amount * (127f - vel) / 127f;
                }
            }

            /* get the initial value of the first source */
            if (Src1 > 0)
            {
                if ((Flags1 & (byte)fluid_mod_flags.FLUID_MOD_CC) > 0)
                {
                    v1 = ((Src1 >= 0) && (Src1 < 128)) ? chan.Controller((MPTKController)Src1) : 0;
                    //if (src1 == 10) Debug.Log("retreive pan " + v1);
                }
                else
                {
                    /* source 1 is one of the direct controllers */
                    switch (Src1)
                    {
                        case (int)fluid_mod_src.FLUID_MOD_NONE:         /* SF 2.01 8.2.1 item 0: src enum=0 => value is 1 */
                            v1 = range1;
                            break;
                        case (int)fluid_mod_src.FLUID_MOD_VELOCITY:
                            v1 = vel;
                            break;
                        case (int)fluid_mod_src.FLUID_MOD_KEY:
                            v1 = key;
                            break;
                        case (int)fluid_mod_src.FLUID_MOD_KEYPRESSURE:
                            v1 = chan.key_pressure;
                            break;
                        case (int)fluid_mod_src.FLUID_MOD_CHANNELPRESSURE:
                            v1 = chan.channel_pressure;
                            break;
                        case (int)fluid_mod_src.FLUID_MOD_PITCHWHEEL:
                            v1 = chan.pitch_bend;
                            range1 = 0x4000;
                            break;
                        case (int)fluid_mod_src.FLUID_MOD_PITCHWHEELSENS:
                            v1 = chan.pitch_wheel_sensitivity;
                            break;
                        default:
                            v1 = 0.0f;
                            break;
                    }
                }

                /* transform the input value */
                switch (Flags1 & 0x0f)
                {
                    case 0: /* linear, unipolar, positive */
                        v1 /= range1;
                        break;
                    case 1: /* linear, unipolar, negative */
                        v1 = 1.0f - v1 / range1;
                        break;
                    case 2: /* linear, bipolar, positive */
                        v1 = -1.0f + 2.0f * v1 / range1;
                        break;
                    case 3: /* linear, bipolar, negative */
                        v1 = -1.0f + 2.0f * v1 / range1;
                        break;
                    case 4: /* concave, unipolar, positive */
                        v1 = fluid_conv.fluid_concave(v1);
                        break;
                    case 5: /* concave, unipolar, negative */
                        v1 = fluid_conv.fluid_concave(127 - v1);
                        break;
                    case 6: /* concave, bipolar, positive */
                        v1 = (v1 > 64) ? fluid_conv.fluid_concave(2 * (v1 - 64)) : -fluid_conv.fluid_concave(2 * (64 - v1));
                        break;
                    case 7: /* concave, bipolar, negative */
                        v1 = (v1 > 64) ? -fluid_conv.fluid_concave(2 * (v1 - 64)) : fluid_conv.fluid_concave(2 * (64 - v1));
                        break;
                    case 8: /* convex, unipolar, positive */
                        v1 = fluid_conv.fluid_convex(v1);
                        break;
                    case 9: /* convex, unipolar, negative */
                        v1 = fluid_conv.fluid_convex(127 - v1);
                        break;
                    case 10: /* convex, bipolar, positive */
                        v1 = (v1 > 64) ? -fluid_conv.fluid_convex(2 * (v1 - 64)) : fluid_conv.fluid_convex(2 * (64 - v1));
                        break;
                    case 11: /* convex, bipolar, negative */
                        v1 = (v1 > 64) ? -fluid_conv.fluid_convex(2 * (v1 - 64)) : fluid_conv.fluid_convex(2 * (64 - v1));
                        break;
                    case 12: /* switch, unipolar, positive */
                        v1 = (v1 >= 64) ? 1.0f : 0.0f;
                        break;
                    case 13: /* switch, unipolar, negative */
                        v1 = (v1 >= 64) ? 0.0f : 1.0f;
                        break;
                    case 14: /* switch, bipolar, positive */
                        v1 = (v1 >= 64) ? 1.0f : -1.0f;
                        break;
                    case 15: /* switch, bipolar, negative */
                        v1 = (v1 >= 64) ? -1.0f : 1.0f;
                        break;
                }
            }
            else
            {
                return 0.0f;
            }

            /* no need to go further */
            if (v1 == 0.0f)
            {
                return 0.0f;
            }

            /* get the second input source */
            if (Src2 > 0)
            {
                if ((Flags2 & (byte)fluid_mod_flags.FLUID_MOD_CC) > 0)
                {
                    v2 = ((Src2 >= 0) && (Src2 < 128)) ? chan.Controller((MPTKController)Src2) : 0;

                }
                else
                {
                    switch (Src2)
                    {
                        case (int)fluid_mod_src.FLUID_MOD_NONE:         /* SF 2.01 8.2.1 item 0: src enum=0 => value is 1 */
                            v2 = range2;
                            break;
                        case (int)fluid_mod_src.FLUID_MOD_VELOCITY:
                            v2 = vel;
                            break;
                        case (int)fluid_mod_src.FLUID_MOD_KEY:
                            v2 = key;
                            break;
                        case (int)fluid_mod_src.FLUID_MOD_KEYPRESSURE:
                            v2 = chan.key_pressure;
                            break;
                        case (int)fluid_mod_src.FLUID_MOD_CHANNELPRESSURE:
                            v2 = chan.channel_pressure;
                            break;
                        case (int)fluid_mod_src.FLUID_MOD_PITCHWHEEL:
                            v2 = chan.pitch_bend;
                            break;
                        case (int)fluid_mod_src.FLUID_MOD_PITCHWHEELSENS:
                            v2 = chan.pitch_wheel_sensitivity;
                            break;
                        default:
                            v1 = 0.0f;
                            break;
                    }
                }

                /* transform the second input value */
                switch (Flags2 & 0x0f)
                {
                    case 0: /* linear, unipolar, positive */
                        v2 /= range2;
                        break;
                    case 1: /* linear, unipolar, negative */
                        v2 = 1.0f - v2 / range2;
                        break;
                    case 2: /* linear, bipolar, positive */
                        v2 = -1.0f + 2.0f * v2 / range2;
                        break;
                    case 3: /* linear, bipolar, negative */
                        v2 = -1.0f + 2.0f * v2 / range2;
                        break;
                    case 4: /* concave, unipolar, positive */
                        v2 = fluid_conv.fluid_concave(v2);
                        break;
                    case 5: /* concave, unipolar, negative */
                        v2 = fluid_conv.fluid_concave(127 - v2);
                        break;
                    case 6: /* concave, bipolar, positive */
                        v2 = (v2 > 64) ? fluid_conv.fluid_concave(2 * (v2 - 64)) : -fluid_conv.fluid_concave(2 * (64 - v2));
                        break;
                    case 7: /* concave, bipolar, negative */
                        v2 = (v2 > 64) ? -fluid_conv.fluid_concave(2 * (v2 - 64)) : fluid_conv.fluid_concave(2 * (64 - v2));
                        break;
                    case 8: /* convex, unipolar, positive */
                        v2 = fluid_conv.fluid_convex(v2);
                        break;
                    case 9: /* convex, unipolar, negative */
                        v2 = 1.0f - fluid_conv.fluid_convex(v2);
                        break;
                    case 10: /* convex, bipolar, positive */
                        v2 = (v2 > 64) ? -fluid_conv.fluid_convex(2 * (v2 - 64)) : fluid_conv.fluid_convex(2 * (64 - v2));
                        break;
                    case 11: /* convex, bipolar, negative */
                        v2 = (v2 > 64) ? -fluid_conv.fluid_convex(2 * (v2 - 64)) : fluid_conv.fluid_convex(2 * (64 - v2));
                        break;
                    case 12: /* switch, unipolar, positive */
                        v2 = (v2 >= 64) ? 1.0f : 0.0f;
                        break;
                    case 13: /* switch, unipolar, negative */
                        v2 = (v2 >= 64) ? 0.0f : 1.0f;
                        break;
                    case 14: /* switch, bipolar, positive */
                        v2 = (v2 >= 64) ? 1.0f : -1.0f;
                        break;
                    case 15: /* switch, bipolar, negative */
                        v2 = (v2 >= 64) ? -1.0f : 1.0f;
                        break;
                }
            }
            else
            {
                v2 = 1.0f;
            }

            /* it's as simple as that: */
            return Amount * v1 * v2;
        }

        public override string ToString()
        {
            return string.Format("Mod amount:{0} src1:{1} flags1:{2} src2:{3} flags2:{4} dest:{5}", this.Amount, this.Src1, this.Flags1, this.Src2, this.Flags2, (fluid_gen_type)this.Dest);
        }

        static public void DebugLog(string info, List<HiMod> mods)
        {
            foreach (HiMod mod in mods)
                Debug.Log(info + mod.ToString());
        }
    }

}
