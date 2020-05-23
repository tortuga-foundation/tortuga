using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.Effect
{
    /// <summary>
    /// different types of filters that can be applied to an effect
    /// </summary>
    public enum FilterType
    {
        /// <summary>
        /// none
        /// </summary> 
        None,
        /// <summary>
        /// low pass
        /// </summary>
        LowPass,
        /// <summary>
        /// high pass
        /// </summary>
        HighPass,
        /// <summary>
        /// band pass
        /// </summary>
        BandPass
    }

    /// <summary>
    /// fitler
    /// </summary>
    public abstract class Filter
    {
        /// <summary>
        /// type of filter
        /// </summary>
        public virtual FilterType Type => FilterType.None;
    
        internal uint Handle => _handle;
        /// <summary>
        /// open al filter handle
        /// </summary>
        protected uint _handle;

        /// <summary>
        /// constructor for filter base class
        /// </summary>
        public Filter()
        {
            alGenFilters(out uint _handle);
            alHandleError("failed to generate filter: ");
        }
    }
}