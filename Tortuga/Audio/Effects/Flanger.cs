using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.Effect
{
    /// <summary>
    /// flanger audio effect
    /// </summary>
    public class Flanger : AudioEffect
    {
        /// <summary>
        /// flanger waveform
        /// </summary>
        public WaveformType Waveform
        {
            get
            {
                var val = new int[1];
                alGetEffectiv(_effect, ALFlanger.Waveform, val);
                alHandleError("failed to get flanger variable");
                return (WaveformType)val[0];
            }
            set
            {
                alEffectiv(_effect, ALFlanger.Waveform, new int[]{ (int)value });
                alHandleError("failed to set flanger variable");
            }
        }

        /// <summary>
        /// flanger phase
        /// </summary>
        public int Phase
        {
            get
            {
                var val = new int[1];
                alGetEffectiv(_effect, ALFlanger.Phase, val);
                alHandleError("failed to get flanger variable");
                return val[0];
            }
            set
            {
                alEffectiv(_effect, ALFlanger.Phase, new int[]{ value });
                alHandleError("failed to set flanger variable");
            }
        }

        /// <summary>
        /// flanger delay
        /// </summary>
        public float Delay
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALFlanger.Delay, val);
                alHandleError("failed to get flanger variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALFlanger.Delay, new float[]{ value });
                alHandleError("failed to set flanger variable");
            }
        }
        
        /// <summary>
        /// flanger rate
        /// </summary>
        public float Rate
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALFlanger.Rate, val);
                alHandleError("failed to get flanger variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALFlanger.Rate, new float[]{ value });
                alHandleError("failed to set flanger variable");
            }
        }
        
        /// <summary>
        /// flanger depth
        /// </summary>
        public float Depth
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALFlanger.Depth, val);
                alHandleError("failed to get flanger variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALFlanger.Depth, new float[]{ value });
                alHandleError("failed to set flanger variable");
            }
        }

        /// <summary>
        /// flanger feedback
        /// </summary>
        public float Feedback
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALFlanger.Feedback, val);
                alHandleError("failed to get flanger variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALFlanger.Feedback, new float[]{ value });
                alHandleError("failed to set flanger variable");
            }
        }

        /// <summary>
        /// constructor for flanger
        /// </summary>
        public Flanger() : base()
        {
            alEffectiv(_effect, ALEffect.Type, new int[]{ (int)ALEffect.Flanger });
            alHandleError("failed to setup echo: ");
        }
    }
}