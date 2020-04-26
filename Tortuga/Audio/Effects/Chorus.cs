using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.Effect
{
    /// <summary>
    /// Chorus audio effect
    /// </summary>
    public class Chorus : AudioEffect
    {

        /// <summary>
        /// Types of waveform
        /// </summary>
        public enum WaveformType
        {
            /// <summary>
            /// Sin
            /// </summary>
            Sin  = 0,
            /// <summary>
            /// Tirangle
            /// </summary>
            Triangle = 1
        }

        /// <summary>
        /// chorus rate
        /// </summary>
        public float Rate
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALChorus.Rate, val);
                alHandleError("failed to set chorus variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALChorus.Rate, new float[]{ value });
                alHandleError("failed to set chorus variable");
            }
        }

        /// <summary>
        /// chorus phase
        /// </summary>
        public int Phase
        {
            get
            {
                var val = new int[1];
                alGetEffectiv(_effect, ALChorus.Phase, val);
                alHandleError("failed to set chorus variable");
                return val[0];
            }
            set
            {
                alEffectiv(_effect, ALChorus.Phase, new int[]{ value });
                alHandleError("failed to set chorus variable");
            }
        }

        /// <summary>
        /// chorus waveform
        /// </summary>
        public WaveformType Waveform
        {
            get
            {
                var val = new int[1];
                alGetEffectiv(_effect, ALChorus.Waveform, val);
                alHandleError("failed to set chorus variable");
                return (WaveformType)val[0];
            }
            set
            {
                alEffectiv(_effect, ALChorus.Waveform, new int[]{ (int)value });
                alHandleError("failed to set chorus variable");
            }
        }

        /// <summary>
        /// chorus depth
        /// </summary>
        public float Depth
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALChorus.Depth, val);
                alHandleError("failed to set chorus variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALChorus.Depth, new float[]{ value });
                alHandleError("failed to set chorus variable");
            }
        }

        /// <summary>
        /// chorus feedback
        /// </summary>
        public float Feedback
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALChorus.Feedback, val);
                alHandleError("failed to set chorus variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALChorus.Feedback, new float[]{ value });
                alHandleError("failed to set chorus variable");
            }
        }

        /// <summary>
        /// chorus delay
        /// </summary>
        public float Delay
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALChorus.Delay, val);
                alHandleError("failed to set chorus variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALChorus.Delay, new float[]{ value });
                alHandleError("failed to set chorus variable");
            }
        }

        /// <summary>
        /// constructor for chorus
        /// </summary>
        public Chorus() : base()
        {
            alEffectiv(_effect, ALEffect.Type, new int[]{ (int)ALEffect.Chorus });
            alHandleError("failed to setup chorus: ");
        }
    }
}