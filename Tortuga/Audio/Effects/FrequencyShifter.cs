using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.Effect
{
    /// <summary>
    /// frequency shifter audio effect
    /// </summary>
    public class FrequencyShifter : AudioEffect
    {
        /// <summary>
        /// frequency direction type
        /// </summary>
        public enum DirectionType
        {
            /// <summary>
            /// down
            /// </summary>
            Down = 0,
            /// <summary>
            /// up
            /// </summary>
            Up = 1,
            /// <summary>
            /// off
            /// </summary>
            Off = 0
        }

        /// <summary>
        /// frequency shifter frequency
        /// </summary>
        public float Frequency
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALFrequencyShifter.Frequency, val);
                alHandleError("failed to get flanger variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALFrequencyShifter.Frequency, new float[]{ value });
                alHandleError("failed to set flanger variable");
            }
        }

        /// <summary>
        /// frequency shifter left direction
        /// </summary>
        public DirectionType LeftDirection
        {
            get
            {
                var val = new int[1];
                alGetEffectiv(_effect, ALFrequencyShifter.LeftDirection, val);
                alHandleError("failed to get flanger variable");
                return (DirectionType)val[0];
            }
            set
            {
                alEffectiv(_effect, ALFrequencyShifter.LeftDirection, new int[]{ (int)value });
                alHandleError("failed to set flanger variable");
            }
        }

        /// <summary>
        /// frequency shifter right direction
        /// </summary>
        public DirectionType RightDirection
        {
            get
            {
                var val = new int[1];
                alGetEffectiv(_effect, ALFrequencyShifter.RightDirection, val);
                alHandleError("failed to get flanger variable");
                return (DirectionType)val[0];
            }
            set
            {
                alEffectiv(_effect, ALFrequencyShifter.RightDirection, new int[]{ (int)value });
                alHandleError("failed to set flanger variable");
            }
        }

        /// <summary>
        /// constructor for flanger
        /// </summary>
        public FrequencyShifter() : base()
        {
            alEffectiv(_effect, ALEffect.Type, new int[]{ (int)ALEffect.FrequencyShifter });
            alHandleError("failed to setup echo: ");
        }
    }
}