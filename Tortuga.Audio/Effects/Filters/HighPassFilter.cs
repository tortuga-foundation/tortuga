using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.Effect
{
    /// <summary>
    /// fitler
    /// </summary>
    public class HighPassFilter :  Filter
    {
        /// <summary>
        /// type of filter
        /// </summary>
        public override FilterType Type => FilterType.HighPass;

        /// <summary>
        /// high pass filter gain
        /// </summary>
        public float Gain
        {
            get
            {
                var val = new float[1];
                alGetFilterfv(_handle, ALHighPassFilter.Gain, val);
                alHandleError("failed to get filter gain");
                return val[0];
            }
            set
            {
                alFilterfv(_handle, ALHighPassFilter.Gain, new float[]{ value });
                alHandleError("failed to set filter gain");
            }
        }

        /// <summary>
        /// high pass filter gain low frequency
        /// </summary>
        public float GainLF
        {
            get
            {
                var val = new float[1];
                alGetFilterfv(_handle, ALHighPassFilter.GainLF, val);
                alHandleError("failed to get filter Gain LF");
                return val[0];
            }
            set
            {
                alFilterfv(_handle, ALHighPassFilter.GainLF, new float[]{ value });
                alHandleError("failed to set filter Gain LF");
            }
        }

        /// <summary>
        /// constructor for high pass filter
        /// </summary>
        public HighPassFilter() : base()
        {
            alFilteriv(_handle, ALFilter.Type, new int[]{ (int)ALFilter.HighPass });
            alHandleError("failed to setup high pass filter");
        }
    }
}