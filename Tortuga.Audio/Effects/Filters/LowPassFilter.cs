using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.Effect
{
    /// <summary>
    /// fitler
    /// </summary>
    public class LowPassFilter :  Filter
    {
        /// <summary>
        /// type of filter
        /// </summary>
        public override FilterType Type => FilterType.LowPass;

        /// <summary>
        /// low pass filter gain
        /// </summary>
        public float Gain
        {
            get
            {
                var val = new float[1];
                alGetFilterfv(_handle, ALLowPassFilter.Gain, val);
                alHandleError("failed to get filter gain");
                return val[0];
            }
            set
            {
                alFilterfv(_handle, ALLowPassFilter.Gain, new float[]{ value });
                alHandleError("failed to set filter gain");
            }
        }

        /// <summary>
        /// low pass filter gain high frequency
        /// </summary>
        public float GainHF
        {
            get
            {
                var val = new float[1];
                alGetFilterfv(_handle, ALLowPassFilter.GainHF, val);
                alHandleError("failed to get filter Gain HF");
                return val[0];
            }
            set
            {
                alFilterfv(_handle, ALLowPassFilter.GainHF, new float[]{ value });
                alHandleError("failed to set filter Gain HF");
            }
        }

        /// <summary>
        /// constructor for low pass filter
        /// </summary>
        public LowPassFilter() : base()
        {
            alFilteriv(_handle, ALFilter.Type, new int[]{ (int)ALFilter.LowPass });
            alHandleError("failed to setup low pass filter");
        }
    }
}