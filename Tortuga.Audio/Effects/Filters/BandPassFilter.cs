using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.Effect
{
    /// <summary>
    /// fitler
    /// </summary>
    public class BandPassFilter :  Filter
    {
        /// <summary>
        /// type of filter
        /// </summary>
        public override FilterType Type => FilterType.BandPass;

        /// <summary>
        /// band pass filter gain
        /// </summary>
        public float Gain
        {
            get
            {
                var val = new float[1];
                alGetFilterfv(_handle, ALBandPassFilter.Gain, val);
                alHandleError("failed to get filter gain");
                return val[0];
            }
            set
            {
                alFilterfv(_handle, ALBandPassFilter.Gain, new float[]{ value });
                alHandleError("failed to set filter gain");
            }
        }

        /// <summary>
        /// band pass filter gain low frequency
        /// </summary>
        public float GainLF
        {
            get
            {
                var val = new float[1];
                alGetFilterfv(_handle, ALBandPassFilter.GainLF, val);
                alHandleError("failed to get filter Gain LF");
                return val[0];
            }
            set
            {
                alFilterfv(_handle, ALBandPassFilter.GainLF, new float[]{ value });
                alHandleError("failed to set filter Gain LF");
            }
        }

        /// <summary>
        /// band pass filter gain high frequency
        /// </summary>
        public float GainHF
        {
            get
            {
                var val = new float[1];
                alGetFilterfv(_handle, ALBandPassFilter.GainHF, val);
                alHandleError("failed to get filter Gain HF");
                return val[0];
            }
            set
            {
                alFilterfv(_handle, ALBandPassFilter.GainHF, new float[]{ value });
                alHandleError("failed to set filter Gain HF");
            }
        }

        /// <summary>
        /// constructor for band pass filter
        /// </summary>
        public BandPassFilter() : base()
        {
            alFilteriv(_handle, ALFilter.Type, new int[]{ (int)ALFilter.BandPass });
            alHandleError("failed to setup band pass filter");
        }
    }
}