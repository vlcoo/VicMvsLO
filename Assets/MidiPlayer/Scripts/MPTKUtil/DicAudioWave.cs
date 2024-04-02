using System.Collections.Generic;


namespace MidiPlayerTK
{
    //! @cond NODOC

    /// <summary>
    /// HiSample cache wich contains samples (core mode) 
    /// </summary>
    public class DicAudioWave
    {
        private static Dictionary<string, HiSample> dicWave;
        public static void Init()
        {
            dicWave = new Dictionary<string, HiSample>();
        }

        public static void Add(HiSample smpl)
        {
            HiSample c;
            try
            {
                if (dicWave!= null && !dicWave.TryGetValue(smpl.Name, out c))
                {
                    dicWave.Add(smpl.Name, smpl);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
        public static bool Exist(string name)
        {
            try
            {
                if (dicWave != null)
                {
                    HiSample c;
                    return dicWave.TryGetValue(name, out c);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return false;
        }
        public static HiSample Get(string name)
        {
            try
            {
                HiSample c;
                dicWave.TryGetValue(name, out c);
                return c;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return null;
        }
        public static HiSample GetWave(string name)
        {
            try
            {
                HiSample c;

                dicWave.TryGetValue(name, out c);
                return c;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return null;
        }
    }
    //! @endcond
}
